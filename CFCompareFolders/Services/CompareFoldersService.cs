using CFCompareFolders.Enums;
using CFCompareFolders.Interfaces;
using CFCompareFolders.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CFCompareFolders.Services
{
    /// <summary>
    /// Compares folders
    /// </summary>
    internal class CompareFoldersService : ICompareFoldersServices
    {
        private readonly ICompareFilesService _compareFilesService;

        private Action<List<CompareItem>> _receiveFolderResults;
        private Action<string, string> _folderCheckActionStart;
        private Action<string, string> _folderCheckActionEnd;
        //private Action<string, string> _fileCheckActionStart;
        //private Action<string, string> _fileCheckActionEnd;

        public CompareFoldersService(ICompareFilesService compareFilesService)
        {
            _compareFilesService = compareFilesService;
        }

        public void SetReceiveFolderResults(Action<List<CompareItem>> results)
        {
            _receiveFolderResults = results;
        }

        public void SetFolderCheckActionStart(Action<string, string> action)
        {
            _folderCheckActionStart = action;
        }

        public void SetFolderCheckActionEnd(Action<string, string> action)
        {
            _folderCheckActionEnd = action;
        }

        //public void SetFileCheckActionStart(Action<string, string> action)
        //{
        //    _fileCheckActionStart = action;
        //}
        //public void SetFileCheckActionEnd(Action<string, string> action)
        //{
        //    _fileCheckActionEnd = action;
        //}

        /// <summary>
        /// Compares two folders
        /// </summary>
        /// <param name="folder1"></param>
        /// <param name="folder2"></param>
        /// <returns></returns>
        public List<CompareItem> CompareFolders(string folder1, string folder2, CompareOptions compareOptions,
                                                CancellationToken cancellationToken)
        {
            return CompareFoldersInternal(folder1, folder2, compareOptions, cancellationToken);
        }

        private List<CompareItem> CompareFoldersInternal(string folder1, string folder2, CompareOptions compareOptions,
                                                CancellationToken cancellationToken)
        {
            List<CompareItem> compareItems = new List<CompareItem>();

            System.Diagnostics.Debug.WriteLine(string.Format("Comparing {0} to {1}", folder1, folder2));

            _folderCheckActionStart(folder1, folder2);

            CompareItemFolder compareItemFolder = new CompareItemFolder() { FolderID = Guid.NewGuid().ToString(), Object1 = folder1, Object2 = folder2 };
            compareItems.Add(compareItemFolder);
            if (!Directory.Exists(folder1))
            {
                if (compareOptions.DifferenceTypes.Contains(DifferenceTypes.Folder1NotExists))
                {
                    compareItemFolder.DifferenceTypeList.Add(DifferenceTypes.Folder1NotExists);
                }
                _folderCheckActionEnd(folder1, folder2);
                _receiveFolderResults(compareItems.Where(ci => ci.FolderID == compareItemFolder.FolderID).ToList());
                return compareItems;        // No point in continuing comparing items in this folder
            }
            if (!Directory.Exists(folder2))
            {
                if (compareOptions.DifferenceTypes.Contains(DifferenceTypes.Folder2NotExists))
                {
                    compareItemFolder.DifferenceTypeList.Add(DifferenceTypes.Folder2NotExists);
                }
                _folderCheckActionEnd(folder1, folder2);
                _receiveFolderResults(compareItems.Where(ci => ci.FolderID == compareItemFolder.FolderID).ToList());
                return compareItems;       // No point in continuing comparing items in this folder
            }

            DirectoryInfo directoryInfo1 = new DirectoryInfo(folder1);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(folder2);

            if (directoryInfo1.LastWriteTime != directoryInfo2.LastWriteTime &&
                compareOptions.DifferenceTypes.Contains(DifferenceTypes.FolderModifiedDifferent))
            {
                compareItemFolder.DifferenceTypeList.Add(DifferenceTypes.FolderModifiedDifferent);
            }
            if (directoryInfo1.Attributes != directoryInfo2.Attributes &&
                compareOptions.DifferenceTypes.Contains(DifferenceTypes.FolderAttributesDifferent))
            {
                compareItemFolder.DifferenceTypeList.Add(DifferenceTypes.FolderAttributesDifferent);
            }

            // Check files in folder 1
            var filesChecked = new List<string>();
            if (Directory.Exists(folder1) && !cancellationToken.IsCancellationRequested)
            {
                foreach (string file1 in Directory.GetFiles(folder1))
                {
                    FileInfo fileInfo1 = new FileInfo(file1);
                    string file2 = Path.Combine(folder2, Path.GetFileName(file1));

                    bool isCompareFiles = IsNeedToCheckFile(file1, fileInfo1, compareOptions);
                    if (isCompareFiles)
                    {
                        //_fileCheckActionStart(file1, file2);
                        CompareItemFile compareItemFile = _compareFilesService.CompareFiles(file1, file2,
                                    compareItemFolder.FolderID, compareOptions);
                        //_fileCheckActionEnd(file1, file2);
                        if (IsNeedToReturnItem(compareItemFile, compareOptions)) compareItems.Add(compareItemFile);
                        filesChecked.Add(Path.GetFileName(file1).ToLower());
                    }

                    if (cancellationToken.IsCancellationRequested) break;
                    System.Threading.Thread.Yield();
                }
            }

            // Check files that only exist in folder 2, already checked files that exist in both or in folder 1      
            if (Directory.Exists(folder2) && !cancellationToken.IsCancellationRequested)
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
                            //_fileCheckActionStart(file1, file2);
                            CompareItemFile compareItemFile = _compareFilesService.CompareFiles(file1, file2, 
                                        compareItemFolder.FolderID, compareOptions);
                            //_fileCheckActionEnd(file1, file2);
                            if (IsNeedToReturnItem(compareItemFile, compareOptions)) compareItems.Add(compareItemFile);
                        }
                    }

                    if (cancellationToken.IsCancellationRequested) break;
                    System.Threading.Thread.Yield();
                }
            }

            // Check sub-folders
            if (compareOptions.IncludeSubFolders && !cancellationToken.IsCancellationRequested)
            {
                var subFoldersChecked = new List<string>();

                // Check sub-folders in 1
                if (Directory.Exists(folder1) && !cancellationToken.IsCancellationRequested)
                {
                    foreach (string subFolder1 in Directory.GetDirectories(folder1))
                    {
                        DirectoryInfo subDirectoryInfo1 = new DirectoryInfo(subFolder1);
                        //string subFolder2 = string.Format(@"{0}\{1}", folder2, subDirectoryInfo1.Name);
                        string subFolder2 = Path.Combine(folder2, subDirectoryInfo1.Name);
                        subFoldersChecked.Add(directoryInfo1.Name);

                        bool isCompareFolders = IsNeedToCheckFolder(subFolder1, subDirectoryInfo1, compareOptions);
                        if (isCompareFolders)
                        {
                            List<CompareItem> subCompareItems = CompareFoldersInternal(subFolder1, subFolder2, compareOptions, cancellationToken);
                            subCompareItems.Where(item => IsNeedToReturnItem(item, compareOptions)).ToList()
                                            .ForEach(item => compareItems.Add(item));
                        }

                        if (cancellationToken.IsCancellationRequested) break;
                        System.Threading.Thread.Yield();
                    }
                }

                // Check sub-folders in 2 that aren't in 1
                if (Directory.Exists(folder2) && !cancellationToken.IsCancellationRequested)
                {
                    foreach (string subFolder2 in Directory.GetDirectories(folder2))
                    {
                        DirectoryInfo subDirectoryInfo2 = new DirectoryInfo(subFolder2);
                        //string subFolder1 = string.Format(@"{0}\{1}", folder1, subDirectoryInfo2.Name);
                        string subFolder1 = Path.Combine(folder1, subDirectoryInfo2.Name);

                        if (!subFoldersChecked.Contains(directoryInfo2.Name))
                        {
                            bool isCompareFolders = IsNeedToCheckFolder(subFolder2, subDirectoryInfo2, compareOptions);
                            if (isCompareFolders)
                            {
                                List<CompareItem> subCompareItems = CompareFoldersInternal(subFolder1, subFolder2, compareOptions, cancellationToken);
                                subCompareItems.Where(item => IsNeedToReturnItem(item, compareOptions)).ToList()
                                                .ForEach(item => compareItems.Add(item));
                            }
                        }

                        if (cancellationToken.IsCancellationRequested) break;
                        System.Threading.Thread.Yield();
                    }
                }
            }

            //System.Diagnostics.Debug.WriteLine(string.Format("Compared {0} to {1}", folder1, folder2));
            System.Threading.Thread.Yield();

            _folderCheckActionEnd(folder1, folder2);

            compareItems = compareItems.Where(item => IsNeedToReturnItem(item, compareOptions)).ToList();

            // Stream results for current folder
            _receiveFolderResults(compareItems.Where(ci => ci.FolderID == compareItemFolder.FolderID).ToList());

            return compareItems.ToList();
        }

        /// <summary>
        /// Whether to return compare item. Might need to return either all items compared or only those with differences
        /// </summary>
        /// <param name="compareItem"></param>
        /// <param name="compareOptions"></param>
        /// <returns></returns>
        private static bool IsNeedToReturnItem(CompareItem compareItem, CompareOptions compareOptions)
        {           
            return (compareOptions.OnlyItemsWithDifferences && compareItem.DifferenceTypeList.Any()) ||
                (!compareOptions.OnlyItemsWithDifferences);
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

        /// <summary>
        /// Whether file needs to be checked based on compare options
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fileInfo"></param>
        /// <param name="compareOptions"></param>
        /// <returns></returns>
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
    }
}
