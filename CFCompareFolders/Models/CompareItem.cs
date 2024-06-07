using CFCompareFolders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CFCompareFolders.Models
{
    /// <summary>
    /// Compare item details (Folder, file)
    /// </summary>
    public abstract class CompareItem
    {        
        /// <summary>
        /// Unique folder ID
        /// </summary>
        public string FolderID { get; set; }
        
        /// <summary>
        /// Folder or file 1
        /// </summary>
        public string Object1 { get; set; }

        /// <summary>
        /// Folder or file 2
        /// </summary>
        public string Object2 { get; set; }

        /// <summary>
        /// Difference list
        /// </summary>
        public List<DifferenceTypes> DifferenceTypeList = new List<DifferenceTypes>();

        public bool ContainsAnyDifferenceType(IEnumerable<DifferenceTypes> differenceTypes)
        {
            return (DifferenceTypeList.Intersect(differenceTypes)).Any();
        }
    }
}
