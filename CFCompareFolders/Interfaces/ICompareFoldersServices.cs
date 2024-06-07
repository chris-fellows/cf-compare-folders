using CFCompareFolders.Models;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CFCompareFolders.Interfaces
{
    /// <summary>
    /// Interface for comparing folders
    /// </summary>
    public interface ICompareFoldersServices
    {
        /// <summary>
        /// Compares folders
        /// </summary>
        /// <param name="folder1"></param>
        /// <param name="folder2"></param>
        /// <param name="compareOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        List<CompareItem> CompareFolders(string folder1, string folder2, CompareOptions compareOptions,
                                CancellationToken cancellationToken);

        /// <summary>
        /// Sets action to handle results for single folder
        /// </summary>
        /// <param name="results"></param>
        void SetReceiveFolderResults(Action<List<CompareItem>> results);

        /// <summary>
        /// Sets action to handle starting check folder
        /// </summary>
        /// <param name="action"></param>
        void SetFolderCheckActionStart(Action<string, string> action);

        /// <summary>
        /// Sets action to handle completed check folder
        /// </summary>
        /// <param name="action"></param>
        void SetFolderCheckActionEnd(Action<string, string> action);

        ///// <summary>
        ///// Sets action to handle starting checking file
        ///// </summary>
        ///// <param name="action"></param>
        //void SetFileCheckActionStart(Action<string, string> action);

        ///// <summary>
        ///// Sets action to handle completed checking file
        ///// </summary>
        ///// <param name="action"></param>
        //void SetFileCheckActionEnd(Action<string, string> action);
    }
}
