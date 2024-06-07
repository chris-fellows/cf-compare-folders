using CFCompareFolders.Enums;
using CFCompareFolders.Interfaces;
using CFCompareFolders.Models;
using CFCompareFolders.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace CFCompareFolders.Forms
{
    public partial class MainForm : Form
    {
        private CompareOptions _compareOptions;
        private readonly ICompareFoldersServices _compareFoldersServices;
        private readonly IFileDifferenceDisplay _fileDifferenceDisplay;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _compareTask;            

        public MainForm(ICompareFoldersServices compareFoldersServices,
                        IFileDifferenceDisplay fileDifferenceDisplay)
        {            
            InitializeComponent();

            _compareFoldersServices = compareFoldersServices;
            _fileDifferenceDisplay = fileDifferenceDisplay;

            // Set default compare options. 
            _compareOptions = new CompareOptions()
            {
                OnlyItemsWithDifferences = false,       // All items compared
                IncludeSubFolders = true,
                IncludeHiddenFiles = true,
                IncludeHiddenFolders = true,
                DifferenceTypes = new List<DifferenceTypes>()
                {
                    // Folder
                    DifferenceTypes.Folder1NotExists,
                    DifferenceTypes.Folder2NotExists,
                    //DifferenceTypes.FolderAttributesDifferent,
                    //DifferenceTypes.FolderModifiedDifferent,

                    // Files
                    DifferenceTypes.File1NotExists,
                    DifferenceTypes.File2NotExists,
                    DifferenceTypes.FileContentsDifferent,
                    //DifferenceTypes.FileCreatedDifferent,
                    //DifferenceTypes.FileModifiedDifferent,
                    //DifferenceTypes.FileAttributesDifferent
                }
            };

            ReadCommandLine();         
        }

        private void ReadCommandLine()
        {
            string folder1 = "";
            string folder2 = "";
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)       // One folder specified
            {
                folder1 = args[1];
            }
            else if (args.Length >= 3)  // Two folders specified, compare them
            {
                folder1 = args[1];
                folder2 = args[2];                
            }
            txtFolder1.Text = folder1;
            txtFolder2.Text = folder2;
            
            if (!String.IsNullOrEmpty(folder1) && !String.IsNullOrEmpty(folder2))
            {
                string message = ValidateBeforeCompare();
                if (String.IsNullOrEmpty(message))
                {
                    _compareTask = DoCompareAsync();
                }
                else
                {
                    MessageBox.Show(message, "Error");
                }
            }            
        }

        private string ValidateBeforeCompare()
        {
            string message = "";
            if (String.IsNullOrEmpty(txtFolder1.Text) || String.IsNullOrEmpty(txtFolder2.Text))
            {
                message = "One or more folders is not set";
            }
            if (String.IsNullOrEmpty(message) && !String.IsNullOrEmpty(txtFolder1.Text))
            {
                if (!System.IO.Directory.Exists(txtFolder1.Text))
                {
                    message = string.Format("Folder {0} does not exist", txtFolder1.Text);
                }
            }
            if (String.IsNullOrEmpty(message) && !String.IsNullOrEmpty(txtFolder2.Text))
            {
                if (!System.IO.Directory.Exists(txtFolder2.Text))
                {
                    message = string.Format("Folder {0} does not exist", txtFolder2.Text);
                }
            }
            return message;
        }

        /// <summary>
        /// Starts folder compare asynchronously
        /// </summary>
        /// <returns></returns>
        private Task DoCompareAsync()
        {
            var task = Task.Factory.StartNew(() =>
            {
                _cancellationTokenSource = new CancellationTokenSource();

                this.Invoke((Action)delegate
                {
                    tsbCompare.Text = "Cancel";
                    DisplayStatus("Comparing folders");
                    DisplayResults(new List<CompareItem>(), false);    // Empty results
                });

                // Set handler for streamed results, set of results per folder
                _compareFoldersServices.SetReceiveFolderResults((results) =>
                {
                    this.Invoke((Action)delegate
                    {
                        DisplayResults(results, true);
                    });                    
                });

                _compareFoldersServices.SetFolderCheckActionStart((folder1, folder2) =>
                {
                    this.Invoke((Action)delegate
                    {
                        //DisplayStatus(string.Format("Checking {0} - {1}", folder1, folder2));
                        DisplayStatus($"Checking {folder1}");
                    });                    
                });
                _compareFoldersServices.SetFolderCheckActionEnd((folder1, folder2) =>
                {

                });
                //_compareFoldersServices.SetFileCheckActionStart((file1, file2) =>
                //{

                //});
                //_compareFoldersServices.SetFileCheckActionEnd((file1, file2) =>
                //{

                //});

                //service.OnStatusCheckingFolder += HandleStatusCheckingFolder;
                //CompareOptions compareOptions = new CompareOptions()
                //{
                //    OnlyItemsWithDifferences = false,       // All items compared
                //    IncludeSubFolders = true,
                //    IncludeHiddenFiles = true,
                //    IncludeHiddenFolders = true
                //};
                //compareOptions.FileExtensionsToIgnore.AddRange(new string[] { ".dll", ".exe", ".git", ".gitignore", ".pdb", ".cache" });
                //compareOptions.FolderNamesToIgnore.AddRange(new string[] { ".git" });
                
                var compareItems = _compareFoldersServices.CompareFolders(txtFolder1.Text, 
                                            txtFolder2.Text, _compareOptions, _cancellationTokenSource.Token);
                
                this.Invoke((Action)delegate
                {
                    // No need to display full list as we stream them
                    //DisplayResults(compareItems, false);    // Just refresh file list
                    tsbCompare.Text = "Compare";
                    DisplayStatus("Comparison complete");
                });
            });

            return task;
        }

        private void DisplayStatus(string status)
        {
            tsslStatus.Text = string.Format(" {0}", status);
            statusStrip1.Refresh();
        }     

        private static List<CompareItemFile> GetCompareItemsForFolderID(string folderId, List<CompareItem> compareItems, 
                                                                       IEnumerable<DifferenceTypes> differenceTypes)
        {
            List<CompareItemFile> compareItemsNew = new List<CompareItemFile>();
            foreach(CompareItem compareItem in compareItems)
            {
                if (compareItem is CompareItemFile)
                {
                    CompareItemFile compareItemFile = (CompareItemFile)compareItem;
                    if (compareItemFile.FolderID.Equals(folderId, StringComparison.InvariantCultureIgnoreCase))
                    {                        
                        if (compareItemFile.ContainsAnyDifferenceType(differenceTypes))
                        {
                            compareItemsNew.Add(compareItemFile);
                        }
                    }
                }
            }
            return compareItemsNew;
        }

        /// <summary>
        /// Displays compare items, either clears existing or appends
        /// </summary>
        /// <param name="compareItems"></param>
        /// <param name="append"></param>
        private void DisplayResults(List<CompareItem> compareItems, bool append)
        {
            // Refresh headers
            if (!append)
            {
                listView1.Columns.Clear();
                listView1.Columns.Add("Folder", 300);
                listView1.Columns.Add("Name", 170);
                listView1.Columns.Add("Created", 70);
                listView1.Columns.Add("Modified", 70);

                ColumnHeader header = listView1.Columns.Add("Difference", 100);
                header.TextAlign = HorizontalAlignment.Center;

                listView1.Columns.Add("Folder", 300);
                listView1.Columns.Add("Name", 170);
                listView1.Columns.Add("Created", 70);
                listView1.Columns.Add("Modified", 70);
            }

            //DifferenceTypes[] fileDifferenceTypesToUse = new DifferenceTypes[] { DifferenceTypes.File1NotExists, DifferenceTypes.File2NotExists, DifferenceTypes.FileContentsDifferent };
            //DifferenceTypes[] folderDifferenceTypesToUse = new DifferenceTypes[] { DifferenceTypes.Folder1NotExists, DifferenceTypes.Folder2NotExists };

            // Get folder & file difference types
            var fileDifferenceTypesToUse = _compareOptions.DifferenceTypes.Where(dt => InternalUtilities.IsFileDifferenceType(dt));
            var folderDifferenceTypesToUse = _compareOptions.DifferenceTypes.Where(dt => InternalUtilities.IsFolderDifferenceType(dt));

            foreach (CompareItem compareItem in compareItems)
            {
                ListViewItem listViewItem = null;
                if (compareItem is CompareItemFolder)
                {
                    var compareItemFolder = (CompareItemFolder)compareItem;

                    // Get all files for this folder, only need certain differences, don't care about timestamps/attributes
                    var compareItemFiles = GetCompareItemsForFolderID(compareItemFolder.FolderID, compareItems, fileDifferenceTypesToUse);

                    if (compareItemFolder.ContainsAnyDifferenceType(folderDifferenceTypesToUse) || 
                            compareItemFiles.Count > 0)
                    {
                        DirectoryInfo directoryInfo1 = Directory.Exists(compareItemFolder.Object1) ? new DirectoryInfo(compareItemFolder.Object1) : null;
                        DirectoryInfo directoryInfo2 = Directory.Exists(compareItemFolder.Object2) ? new DirectoryInfo(compareItemFolder.Object2) : null;

                        StringBuilder folderDifferences = new StringBuilder("Different");                        
                        if (directoryInfo1 == null)    // Folder 1 missing
                        {                            
                            folderDifferences.Length = 0;
                            folderDifferences.Append("<- Not found");
                        }
                        else if (directoryInfo2 == null)    // Folder 2 missing
                        {                            
                            folderDifferences.Length = 0;
                            folderDifferences.Append("Not found ->");
                        }
                        else if (compareItemFiles.Count > 0)    // Folder has files with differences
                        {
                            // Summarise differences
                            bool isFilesMissing = false;
                            bool isFilesDifference = false;
                            bool isFilesCreatedDifferent = false;
                            bool isFilesModifiedDifferent = false;
                            bool isFilesAddtributesDifferent = false;
                            foreach (CompareItemFile compareItemFile in compareItemFiles)
                            {
                                foreach (DifferenceTypes differenceType in compareItemFile.DifferenceTypeList)
                                {
                                    switch (differenceType)
                                    {
                                        case DifferenceTypes.File1NotExists:
                                        case DifferenceTypes.File2NotExists:
                                            isFilesMissing = true;
                                            break;
                                        case DifferenceTypes.FileContentsDifferent:
                                            isFilesDifference = true;
                                            break;
                                        case DifferenceTypes.FileCreatedDifferent:
                                            isFilesCreatedDifferent = true;
                                            break;
                                        case DifferenceTypes.FileModifiedDifferent:
                                            isFilesModifiedDifferent = true;
                                            break;
                                        case DifferenceTypes.FileAttributesDifferent:
                                            isFilesAddtributesDifferent = true;
                                            break;
                                    }
                                }
                            }

                            // Only summarise if there might not be any files listed below folder. The individual file lists the individual
                            // differences
                            if (isFilesMissing && isFilesDifference)
                            {
                                folderDifferences.Length = 0;
                                folderDifferences.Append("Files missing/different");
                            }
                            else if (isFilesMissing)
                            {
                                folderDifferences.Length = 0;
                                folderDifferences.Append("Files missing");
                            }                            

                            /*
                            folderDifferences.Length = 0;
                            if (isFilesMissing) folderDifferences.Append((folderDifferences.Length == 0 ? "" : "; ") + "File missing");
                            if (isFilesDifference) folderDifferences.Append((folderDifferences.Length == 0 ? "" : "; ") + "File contents different");
                            if (isFilesCreatedDifferent) folderDifferences.Append((folderDifferences.Length == 0 ? "" : "; ") + "File created different");
                            if (isFilesModifiedDifferent) folderDifferences.Append((folderDifferences.Length == 0 ? "" : "; ") + "File modified different");
                            if (isFilesAddtributesDifferent) folderDifferences.Append((folderDifferences.Length == 0 ? "" : "; ") + "File attributes different");
                            */
                        }

                        string name1 = (directoryInfo1 == null ? directoryInfo2.Name : directoryInfo1.Name);
                        string name2 = (directoryInfo2 == null ? directoryInfo1.Name : directoryInfo2.Name);

                        string created1 = (directoryInfo1 == null ? "" : directoryInfo1.CreationTime.ToString());
                        string created2 = (directoryInfo2 == null ? "" : directoryInfo2.CreationTime.ToString());

                        string modified1 = (directoryInfo1 == null ? "" : directoryInfo1.LastWriteTime.ToString());
                        string modified2 = (directoryInfo2 == null ? "" : directoryInfo2.LastWriteTime.ToString());
                        
                        string[] items = { compareItemFolder.Object1, name1, created1, modified1, folderDifferences.ToString(), compareItemFolder.Object2, name2, created2, modified2 };
                        listViewItem = new ListViewItem(items);
                        listViewItem.Tag = compareItem;
                        listView1.Items.Add(listViewItem);                        

                        // Process files
                        foreach (CompareItemFile compareItemFile in compareItemFiles)
                        {
                            // Check if we want to display it
                            bool useFile = compareItemFile.ContainsAnyDifferenceType(fileDifferenceTypesToUse);

                            if (useFile)
                            {
                                FileInfo fileInfo1 = File.Exists(compareItemFile.Object1) ? new FileInfo(compareItemFile.Object1) : null;
                                FileInfo fileInfo2 = File.Exists(compareItemFile.Object2) ? new FileInfo(compareItemFile.Object2) : null;

                                // Set file difference description.
                                StringBuilder fileDifference = new StringBuilder("Different");                                
                                if (fileInfo1 == null)
                                {
                                    fileDifference.Length = 0;
                                    fileDifference.Append("<- Not found");
                                }
                                else if (fileInfo2 == null)
                                {
                                    fileDifference.Length = 0;
                                    fileDifference.Append("Not found ->");
                                }                                                              
                                else if (compareItemFile.DifferenceTypeList.Any())  // Both files exist
                                {
                                    // List differences
                                    fileDifference.Length = 0;
                                    if (compareItemFile.DifferenceTypeList.Contains(DifferenceTypes.FileContentsDifferent))
                                    {
                                        fileDifference.Append((fileDifference.Length == 0 ? "" : "; ") + "Content");
                                    }
                                    if (compareItemFile.DifferenceTypeList.Contains(DifferenceTypes.FileCreatedDifferent))
                                    {
                                        fileDifference.Append((fileDifference.Length == 0 ? "" : "; ") + "Created t/s");
                                    }
                                    if (compareItemFile.DifferenceTypeList.Contains(DifferenceTypes.FileModifiedDifferent))
                                    {
                                        fileDifference.Append((fileDifference.Length == 0 ? "" : "; ") + "Modified t/s");
                                    }
                                    if (compareItemFile.DifferenceTypeList.Contains(DifferenceTypes.FileAttributesDifferent))
                                    {
                                        fileDifference.Append((fileDifference.Length == 0 ? "" : "; ") + "Attributes");
                                    }
                                }

                                string fileName1 = (fileInfo1 == null ? fileInfo2.Name : fileInfo1.Name);
                                string fileName2 = (fileInfo2 == null ? fileInfo1.Name : fileInfo2.Name);

                                string fileCreated1 = (fileInfo1 == null ? "" : fileInfo1.CreationTime.ToString());
                                string fileCreated2 = (fileInfo2 == null ? "" : fileInfo2.CreationTime.ToString());

                                string fileModified1 = (fileInfo1 == null ? "" : fileInfo1.LastWriteTime.ToString());
                                string fileModified2 = (fileInfo2 == null ? "" : fileInfo2.LastWriteTime.ToString());

                                string[] fileItems = { "", fileName1, fileCreated1, fileModified1, fileDifference.ToString(), "", fileName2, fileCreated2, fileModified2 };
                                listViewItem = new ListViewItem(fileItems);
                                listViewItem.Tag = compareItemFile;
                                listView1.Items.Add(listViewItem);
                            }
                        }
                    }
                }                
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].Tag is CompareItemFile)
            {
                CompareItemFile compareItemFile = (CompareItemFile)listView1.SelectedItems[0].Tag;
                _fileDifferenceDisplay.Display(compareItemFile.Object1, compareItemFile.Object2);
            }            
        }

        private string SelectFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog() { Description = "Select Folder", ShowNewFolderButton = false };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return "";
        }

        private void btnSelectFolder1_Click(object sender, EventArgs e)
        {
            string folder = SelectFolder();
            if (!String.IsNullOrEmpty(folder))
            {
                txtFolder1.Text = folder;
            }
        }

        private void btnSelectFolder2_Click(object sender, EventArgs e)
        {
            string folder = SelectFolder();
            if (!String.IsNullOrEmpty(folder))
            {
                txtFolder2.Text = folder;
            }
        }

        private void tsbCompare_Click(object sender, EventArgs e)
        {
            switch (tsbCompare.Text)
            {
                case "Compare":
                    string message = ValidateBeforeCompare();
                    if (String.IsNullOrEmpty(message))
                    {
                        _compareTask = DoCompareAsync();
                    }
                    else
                    {
                        MessageBox.Show(message, "Error");
                    }
                    break;
                case "Cancel":
                    _cancellationTokenSource.Cancel();
                    break;
            }
        }

        private void tsbCompareOptions_Click(object sender, EventArgs e)
        {
            var form = new CompareOptionsForm(_compareOptions);
            form.ShowDialog();
        }
    }
}
