using CFCompareFolders.Interfaces;
using CFCompareFolders.Forms;
using CFCompareFolders.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFCompareFolders
{
    static class Program
    {
        ///// <summary>
        ///// The main entry point for the application.
        ///// </summary>
        //[STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new MainForm());
        //}

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(ServiceProvider.GetRequiredService<MainForm>());
        }

        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Create a host builder to build the service provider
        /// </summary>
        /// <returns></returns>
        static IHostBuilder CreateHostBuilder()
        {            
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddTransient<IFileDifferenceDisplay>((scope) =>
                    {
                        return new ExternalFileDifferenceDisplayService(System.Configuration.ConfigurationSettings.AppSettings.Get("FileDiffTool.Path"),
                                            System.Configuration.ConfigurationSettings.AppSettings.Get("FileDiffTool.Arguments"));
                    });
                    services.AddTransient<ICompareFilesService, CompareFilesService>();
                    services.AddTransient<ICompareFoldersServices, CompareFoldersService>();
                    services.AddTransient<MainForm>();
                });
        }     
    }
}
