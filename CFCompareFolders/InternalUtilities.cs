using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace CFCompareFolders
{
    class InternalUtilities
    {
        public static void StartFileDiffTool(string file1, string file2)
        {           
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            //startInfo.FileName = @"C:\Program Files\KDiff3\kdiff3.exe";  

            startInfo.FileName = System.Configuration.ConfigurationSettings.AppSettings.Get("FileDiffTool.Path");

            string arguments = System.Configuration.ConfigurationSettings.AppSettings.Get("FileDiffTool.Arguments");
            arguments = arguments.Replace("{file1}", file1);
            arguments = arguments.Replace("{file2}", file2);

            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = arguments;
            //startInfo.Arguments = string.Format("\"{0}\" \"{1}\"", file1, file2);

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
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
