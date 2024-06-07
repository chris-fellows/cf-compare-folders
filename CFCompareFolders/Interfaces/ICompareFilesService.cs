using CFCompareFolders.Models;

namespace CFCompareFolders.Interfaces
{
    /// <summary>
    /// Interface for comparing files
    /// </summary>
    internal interface ICompareFilesService
    {
        CompareItemFile CompareFiles(string file1, string file2, string folderId, CompareOptions compareOptions);
    }
}
