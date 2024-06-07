using System;
using System.IO;
using System.ComponentModel;
using CFCompareFolders.Enums;
using System.Linq;

namespace CFCompareFolders.Utilities
{
    internal class InternalUtilities
    {
        public static string GetEnumDescription(Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (field == null)
                return enumValue.ToString();

            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return enumValue.ToString();
        }

        public static bool IsFolderDifferenceType(DifferenceTypes differenceType)
        {
            var enums = new[] { DifferenceTypes.Folder1NotExists, DifferenceTypes.Folder2NotExists, DifferenceTypes.FolderAttributesDifferent, DifferenceTypes.FolderModifiedDifferent };
            return enums.Contains(differenceType);
        }

        public static bool IsFileDifferenceType(DifferenceTypes differenceType)
        {
            return !IsFolderDifferenceType(differenceType);
        }

        public static void CompareFileLines(string file1, int startLine1, out int difference1, string file2, int startLine2, out int difference2)
        {
            difference1 = -1;
            difference2 = -1;

            using (StreamReader reader1 = new StreamReader(file1))
            {
                int lineNo1 = -1;
                int lineNo2 = -1;                        
                using (StreamReader reader2 = new StreamReader(file2))
                {
                    for (int index1 = 0; index1 < startLine1; index1++)
                    {
                        lineNo1++;
                        reader1.ReadLine();
                    }

                    for (int index2 = 0; index2 < startLine2; index2++)
                    {
                        lineNo2++;
                        reader2.ReadLine();
                    }

                    while (!reader1.EndOfStream || !reader2.EndOfStream)
                    {
                        string line1 = "";
                        string line2 = "";
                        if (!reader1.EndOfStream)
                        {
                            lineNo1++;
                            line1 = reader1.ReadLine();
                        }
                        if (!reader2.EndOfStream)
                        {
                            lineNo2++;
                            line2 = reader2.ReadLine();
                        }

                        if (line1 != line2)     // Difference found
                        {
                            difference1 = lineNo1;
                            difference2 = lineNo2;
                            break;
                        }
                    }
                    reader2.Close();
                }
                reader1.Close();
            }
        }
    }
}
