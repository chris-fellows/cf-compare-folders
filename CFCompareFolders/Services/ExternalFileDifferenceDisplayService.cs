using CFCompareFolders.Interfaces;
using System.Diagnostics;

namespace CFCompareFolders.Services
{
    /// <summary>
    /// Displays file differences using external tool (E.g. KDiff)
    /// </summary>
    public class ExternalFileDifferenceDisplayService : IFileDifferenceDisplay
    {
        private readonly string _toolPath;
        private readonly string _arguments;

        public ExternalFileDifferenceDisplayService(string toolPath, string arguments)
        {
            _toolPath = toolPath;
            _arguments = arguments;
        }

        public void Display(string file1, string file2)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = _toolPath;

            var arguments = _arguments;
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
    }
}
