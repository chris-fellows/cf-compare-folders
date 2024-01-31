using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCompareFolders
{
    public abstract class CompareItem
    {
        public enum DifferenceTypes : byte
        {
            Folder1NotExists = 0,
            Folder2NotExists = 1,
            FolderModifiedDifferent = 2,
            FolderAttributesDifferent = 3,
            File1NotExists = 100,
            File2NotExists = 101,
            FileContentsDifferent = 102,
            FileModifiedDifferent = 103,
            FileCreatedDifferent = 104,
            FileAttributesDifferent = 105
        }

        public string FolderID { get; set; }
        
        public string Object1 { get; set; }
        public string Object2 { get; set; }

        public List<DifferenceTypes> DifferenceTypeList = new List<DifferenceTypes>();

        public bool ContainsAnyDifferenceType(CompareItem.DifferenceTypes[] differenceTypes)
        {
            foreach(CompareItem.DifferenceTypes differenceType in differenceTypes)
            {
                if (DifferenceTypeList.Contains(differenceType))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
