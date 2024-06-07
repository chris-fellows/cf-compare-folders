using CFCompareFolders.Enums;
using CFCompareFolders.Interfaces;
using CFCompareFolders.Models;
using System;
using System.IO;
using System.Linq;

namespace CFCompareFolders.Services
{
    internal class CompareFilesService : ICompareFilesService
    {         
        public CompareItemFile CompareFiles(string file1, string file2, string folderId, CompareOptions compareOptions)
        {            
            CompareItemFile compareItemFile = new CompareItemFile() { FolderID = folderId, Object1 = file1, Object2 = file2 };

            if (!File.Exists(file1) && compareOptions.DifferenceTypes.Contains(DifferenceTypes.File1NotExists))
            {
                compareItemFile.DifferenceTypeList.Add(DifferenceTypes.File1NotExists);
            }
            if (!File.Exists(file2) && compareOptions.DifferenceTypes.Contains(DifferenceTypes.File2NotExists))
            {
                compareItemFile.DifferenceTypeList.Add(DifferenceTypes.File2NotExists);
            }

            if (File.Exists(file1) && File.Exists(file2))
            {
                FileInfo fileInfo1 = new FileInfo(file1);
                FileInfo fileInfo2 = new FileInfo(file2);

                // Compare contents
                if (compareOptions.DifferenceTypes.Contains(DifferenceTypes.FileContentsDifferent) &&
                    !IsFileContentsTheSame(file1, file2))
                {
                    compareItemFile.DifferenceTypeList.Add(DifferenceTypes.FileContentsDifferent);
                }

                // Compare created timestamp
                if (fileInfo1.CreationTime != fileInfo2.CreationTime &&
                    compareOptions.DifferenceTypes.Contains(DifferenceTypes.FileCreatedDifferent))
                {
                    compareItemFile.DifferenceTypeList.Add(DifferenceTypes.FileCreatedDifferent);
                }

                // Compare modified timestamp
                if (fileInfo1.LastWriteTime != fileInfo2.LastWriteTime &&
                    compareOptions.DifferenceTypes.Contains(DifferenceTypes.FileModifiedDifferent))
                {
                    compareItemFile.DifferenceTypeList.Add(DifferenceTypes.FileModifiedDifferent);
                }

                // Compare attributes
                if (fileInfo1.Attributes != fileInfo2.Attributes &&
                    compareOptions.DifferenceTypes.Contains(DifferenceTypes.FileAttributesDifferent))
                {
                    compareItemFile.DifferenceTypeList.Add(DifferenceTypes.FileAttributesDifferent);
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
