using CFCompareFolders.Enums;
using System.Collections.Generic;

namespace CFCompareFolders.Models
{
    /// <summary>
    /// Options for comparing pair of folders
    /// </summary>
    public class CompareOptions
    {
        public bool OnlyItemsWithDifferences { get; set; }
        public bool IncludeSubFolders { get; set; }
        public bool IncludeHiddenFolders { get; set; }
        public bool IncludeHiddenFiles { get; set; }

        public List<string> FileExtensionsToIgnore = new List<string>();
        public List<string> FileExtensionsToInclude = new List<string>();

        public List<string> FolderNamesToIgnore = new List<string>();

        public List<DifferenceTypes> DifferenceTypes = new List<DifferenceTypes>();
    }
}
