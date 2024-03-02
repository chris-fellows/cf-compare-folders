using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCompareFolders
{
    internal abstract class Difference
    {
        public enum DifferenceTypes : byte
        {
            FolderNotExists = 0,
            FileNotExists = 100,
            FileContentsDifferent = 101,
            FileModifiedDifferent = 102,
            FileCreatedDifferent = 103,
            FileAttributesDifferent = 104
        }

        public DifferenceTypes DifferenceType { get; set; }
        public string Message { get; set; }
        public string Object1 { get; set; }
        public string Object2 { get; set; }
    }
}
