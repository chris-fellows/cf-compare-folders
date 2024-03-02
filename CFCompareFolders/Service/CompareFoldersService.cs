using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CFCompareFolders
{
    /// <summary>
    /// Compares folders
    /// </summary>
    internal class CompareFoldersService
    {
        public delegate void StatusCheckingFolder(object sender, string folder1, string folder2);
        public StatusCheckingFolder OnStatusCheckingFolder;

        public delegate void StatusCheckingFile(object sender, string file1, string file2);
        public StatusCheckingFile OnStatusCheckingFile;

        /// <summary>
        /// Compares two folders
        /// </summary>
        /// <param name="folder1"></param>
        /// <param name="folder2"></param>
        /// <returns></returns>
        public List<CompareItem> CompareFolders(string folder1, string folder2, CompareOptions compareOptions)
        {
            List<CompareItem> compareItems = new List<CompareItem>();

            System.Diagnostics.Debug.WriteLine(string.Format("Comparing {0} to {1}", folder1, folder2));

            if (OnStatusCheckingFolder != null)
            {
                OnStatusCheckingFolder(this, folder1, folder2);
            }

            CompareItemFolder compareItemFolder = new CompareItemFolder() { FolderID = Guid.NewGuid().ToString(), Object1 = folder1, Object2 = folder2 };
            compareItems.Add(compareItemFolder);
            if (!Directory.Exists(folder1))
            {
                compareItemFolder.DifferenceTypeList.Add(CompareItem.DifferenceTypes.Folder1NotExists);
                return compareItems;        // No point in continuing comparing items in this folder
            }
            if (!Directory.Exists(folder2))
            {
                compareItemFolder.DifferenceTypeList.Add(CompareItem.DifferenceTypes.Folder2NotExists);
                return compareItems;       // No point in continuing comparing items in this folder
            }

            DirectoryInfo directoryInfo1 = new DirectoryInfo(folder1);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(folder2);

            if (directoryInfo1.LastWriteTime != directoryInfo2.LastWriteTime)
            {                
                compareItemFolder.DifferenceTypeList.Add(CompareItem.DifferenceTypes.FolderModifiedDifferent);
            }
            if (directoryInfo1.Attributes != directoryInfo2.Attributes)
            {
                compareItemFolder.DifferenceTypeList.Add(CompareItem.DifferenceTypes.FolderAttributesDifferent);
            }

            // Check files in folder 1
            List<string> filesChecked = new List<string>();
            if (Directory.Exists(folder1))
            {
                foreach (string file1 in Directory.GetFiles(folder1))
                {
                    FileInfo fileInfo1 = new FileInfo(file1);
                    string file2 = Path.Combine(folder2, Path.GetFileName(file1));

                    bool isCompareFiles = IsNeedToCheckFile(file1, fileInfo1, compareOptions);                    
                    if (isCompareFiles)
                    {
                        CompareItemFile compareItemFile = CompareFiles(file1, file2, compareItemFolder.FolderID);
                        compareItems.Add(compareItemFile);
                        filesChecked.Add(Path.GetFileName(file1).ToLower());
                    }
                }
            }

            // Check files that only exist in folder 2, already checked files that exist in both or in folder 1      
            if (Directory.Exists(folder2))
            {
                foreach (string file2 in Directory.GetFiles(folder2))
                {
                    if (!filesChecked.Contains(Path.GetFileName(file2).ToLower()))
                    {
                        FileInfo fileInfo2 = new FileInfo(file2);
                        string file1 = Path.Combine(folder1, Path.GetFileName(file2));

                        bool isCompareFiles = IsNeedToCheckFile(file2, fileInfo2, compareOptions);                       
                        if (isCompareFiles)
                        {
                            CompareItemFile compareItemFile = CompareFiles(file1, file2, compareItemFolder.FolderID);
                            compareItems.Add(compareItemFile);                            
                        }
                    }
                }
            }

            // Check sub-folders
            if (compareOptions.IncludeSubFolders)
            {
                List<string> subFoldersChecked = new List<string>();

                // Check sub-folders in 1
                if (Directory.Exists(folder1))
                {
                    foreach (string subFolder1 in Directory.GetDirectories(folder1))
                    {
                        DirectoryInfo subDirectoryInfo1 = new DirectoryInfo(subFolder1);
                        string subFolder2 = string.Format(@"{0}\{1}", folder2, subDirectoryInfo1.Name);
                        subFoldersChecked.Add(directoryInfo1.Name);

                        bool isCompareFolders = IsNeedToCheckFolder(subFolder1, subDirectoryInfo1, compareOptions);
                        if (isCompareFolders)
                        {
                            List<CompareItem> subCompareItems = CompareFolders(subFolder1, subFolder2, compareOptions);
                            subCompareItems.ForEach(item => compareItems.Add(item));                      
                        }
                    }
                }

                // Check sub-folders in 2 that aren't in 1
                if (Directory.Exists(folder2))
                {
                    foreach (string subFolder2 in Directory.GetDirectories(folder2))
                    {
                        DirectoryInfo subDirectoryInfo2 = new DirectoryInfo(subFolder2);
                        string subFolder1 = string.Format(@"{0}\{1}", folder1, subDirectoryInfo2.Name);

                        if (!subFoldersChecked.Contains(directoryInfo2.Name))
                        {
                            bool isCompareFolders = IsNeedToCheckFolder(subFolder2, subDirectoryInfo2, compareOptions);
                            if (isCompareFolders)
                            {
                                List<CompareItem> subCompareItems = CompareFolders(subFolder1, subFolder2, compareOptions);
                                subCompareItems.ForEach(item => compareItems.Add(item));
                            }
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine(string.Format("Compared {0} to {1}", folder1, folder2));
            System.Threading.Thread.Sleep(3);
            return compareItems;
        }        

        private static bool IsNeedToCheckFolder(string folder, DirectoryInfo directoryInfo, CompareOptions compareOptions)
        {
            if (!compareOptions.IncludeHiddenFolders)  // Ignore hidden folders
            {
                if (directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    return false;
                }
            }

            if (compareOptions.FolderNamesToIgnore.Count > 0)
            {
                string folderName = directoryInfo.Name;
                if (compareOptions.FolderNamesToIgnore.Contains(folderName))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNeedToCheckFile(string file, FileInfo fileInfo, CompareOptions compareOptions)
        {            
            if (!compareOptions.IncludeHiddenFiles) // Ignore hidden files
            {
                if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    return false;
                }
            }

            if (compareOptions.FileExtensionsToIgnore.Count > 0)
            {
                string extension = Path.GetExtension(file);
                if (compareOptions.FileExtensionsToIgnore.Contains(extension))
                {
                    return false;
                }
                return true;
            }
            else if (compareOptions.FileExtensionsToInclude.Count > 0)
            {
                string extension = Path.GetExtension(file);
                if (compareOptions.FileExtensionsToInclude.Contains(extension))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        private CompareItemFile CompareFiles(string file1, string file2, string folderId)
        {
            if (OnStatusCheckingFile != null)
            {
                OnStatusCheckingFile(this, file1, file2);
            }

            CompareItemFile compareItemFile = new CompareItemFile() { FolderID = folderId, Object1 = file1, Object2 = file2 };

            if (!File.Exists(file1))
            {
                compareItemFile.DifferenceTypeList.Add(CompareItem.DifferenceTypes.File1NotExists);                
            }
            if (!File.Exists(file2))
            {
                compareItemFile.DifferenceTypeList.Add(CompareItem.DifferenceTypes.File2NotExists);                
            }

            if (File.Exists(file1) && File.Exists(file2))
            {
                FileInfo fileInfo1 = new FileInfo(file1);
                FileInfo fileInfo2 = new FileInfo(file2);

                if (!IsFileContentsTheSame(file1, file2))
                {
                    compareItemFile.DifferenceTypeList.Add(CompareItem.DifferenceTypes.FileContentsDifferent);
                }

                    if (fileInfo1.CreationTime != fileInfo2.CreationTime)
                    {
                        compareItemFile.DifferenceTypeList.Add(CompareItem.DifferenceTypes.FileCreatedDifferent);
                    }
                    if (fileInfo1.LastWriteTime != fileInfo2.LastWriteTime)
                    {
                        compareItemFile.DifferenceTypeList.Add(CompareItem.DifferenceTypes.FileModifiedDifferent);                        
                    }
                    if (fileInfo1.Attributes != fileInfo2.Attributes)
                    {
                        compareItemFile.DifferenceTypeList.Add(CompareItem.DifferenceTypes.FileAttributesDifferent);                        
                    }                                
            }
            return compareItemFile;
        }

        private bool IsFileContentsTheSame(string file1, string file2)
        {
            // TO DO: Optimize for large files, not very efficient loading all in to memory
            long file1Length = new FileInfo(file1).Length;
            long file2Length = new FileInfo(file2).Length;
            if (file1Length == file2Length)
            {
                byte[] file1Bytes = File.ReadAllBytes(file1);
                byte[] file2Bytes = File.ReadAllBytes(file2);
                if (!file1Bytes.SequenceEqual(file2Bytes))
                {                  
                    return false;
                }               
                return true;
            }
            return false;
        }
    }
}
