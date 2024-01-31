using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCompareFolders
{
    public class CompareOptions
    {
        public bool IncludeSubFolders { get; set; }
        public bool IncludeHiddenFolders { get; set; }
        public bool IncludeHiddenFiles { get; set; }

        public List<string> FileExtensionsToIgnore = new List<string>();
        public List<string> FileExtensionsToInclude = new List<string>();

        public List<string> FolderNamesToIgnore = new List<string>();
    }
}
