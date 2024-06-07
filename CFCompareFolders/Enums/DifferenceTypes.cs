using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CFCompareFolders.Enums
{
    public enum DifferenceTypes : byte
    {
        // Folder differences
        [Description("Folder 1 does not exist")]
        Folder1NotExists = 0,

        [Description("Folder 2 does not exist")]
        Folder2NotExists = 1,

        [Description("Folder modified timestamp different")]
        FolderModifiedDifferent = 2,

        [Description("Folder attributes different")]
        FolderAttributesDifferent = 3,

        // File differences
        [Description("File 1 does not exist")]
        File1NotExists = 100,

        [Description("File 2 does not exist")]
        File2NotExists = 101,

        [Description("File contents different")]
        FileContentsDifferent = 102,

        [Description("File modified timestamp different")]
        FileModifiedDifferent = 103,

        [Description("File created timestamp different")]
        FileCreatedDifferent = 104,

        [Description("File attributes different")]
        FileAttributesDifferent = 105
    }
}
