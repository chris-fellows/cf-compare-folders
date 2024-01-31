using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CFCompareFolders
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

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
                    DoCompare();
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

        private void DoCompare()
        {
            btnCompare.Enabled = false;

            // Compare
            CompareManager manager = new CompareManager();
            manager.OnStatusCheckingFolder += HandleStatusCheckingFolder;
            CompareOptions compareOptions = new CompareOptions()
            {
                IncludeSubFolders = true,
                IncludeHiddenFiles = true,
                IncludeHiddenFolders = true
            };
            compareOptions.FileExtensionsToIgnore.AddRange(new string[] { ".dll", ".exe", ".git", ".gitignore", ".pdb", ".cache" });
            compareOptions.FolderNamesToIgnore.AddRange(new string[] { ".git" });

            DisplayStatus("Comparing folders");
            List<CompareItem> compareItems = manager.CompareFolders(txtFolder1.Text, txtFolder2.Text, compareOptions);  

            // Display results
            DisplayResults(compareItems);            

            btnCompare.Enabled = true;
            DisplayStatus("Comparison complete");
        }

        private void HandleStatusCheckingFolder(object sender, string folder1, string folder2)
        {
            DisplayStatus(string.Format("Checking {0} - {1}", folder1, folder2));
        }

        private void DisplayStatus(string status)
        {
            tsslStatus.Text = string.Format(" {0}", status);
            statusStrip1.Refresh();
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            string message = ValidateBeforeCompare();
            if (String.IsNullOrEmpty(message))
            {
                DoCompare();
            }
            else
            {
                MessageBox.Show(message, "Error");
            }
        }

        private List<CompareItemFile> GetCompareItemsForFolderID(string folderId, List<CompareItem> compareItems, 
                                                                CompareItem.DifferenceTypes[] differenceTypes)
        {
            List<CompareItemFile> compareItemsNew = new List<CompareItemFile>();
            foreach(CompareItem compareItem in compareItems)
            {
                if (compareItem is CompareItemFile)
                {
                    CompareItemFile compareItemFile = (CompareItemFile)compareItem;
                    if (compareItemFile.FolderID.Equals(folderId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Check for difference types
                        bool useFile = compareItemFile.ContainsAnyDifferenceType(differenceTypes);                       
                        if (useFile)
                        {
                            compareItemsNew.Add(compareItemFile);
                        }
                    }
                }
            }
            return compareItemsNew;
        }

        private void DisplayResults(List<CompareItem> compareItems)
        {            
            listView1.Columns.Clear();                        
            listView1.Columns.Add("Folder", 200);
            listView1.Columns.Add("Name", 170);
            listView1.Columns.Add("Modified", 80);

            ColumnHeader header = listView1.Columns.Add("Difference", 100);
            header.TextAlign = HorizontalAlignment.Center;
            
            listView1.Columns.Add("Folder", 200);
            listView1.Columns.Add("Name", 170);
            listView1.Columns.Add("Modified", 80);            

            CompareItem.DifferenceTypes[] fileDifferenceTypesToUse = new CompareItem.DifferenceTypes[] { CompareItem.DifferenceTypes.File1NotExists, CompareItem.DifferenceTypes.File2NotExists, CompareItem.DifferenceTypes.FileContentsDifferent };
            CompareItem.DifferenceTypes[] folderDifferenceTypesToUse = new CompareItem.DifferenceTypes[] { CompareItem.DifferenceTypes.Folder1NotExists, CompareItem.DifferenceTypes.Folder2NotExists };

            foreach (CompareItem compareItem in compareItems)
            {
                ListViewItem listViewItem = null;
                if (compareItem is CompareItemFolder)
                {
                    CompareItemFolder compareItemFolder = (CompareItemFolder)compareItem;

                    // Get all files for this folder, only need certain differences, don't care about timestamps/attributes
                    List<CompareItemFile> compareItemFiles = GetCompareItemsForFolderID(compareItemFolder.FolderID, compareItems, fileDifferenceTypesToUse);

                    if (compareItemFolder.ContainsAnyDifferenceType(folderDifferenceTypesToUse) || compareItemFiles.Count > 0)
                    {
                        DirectoryInfo directoryInfo1 = Directory.Exists(compareItemFolder.Object1) ? new DirectoryInfo(compareItemFolder.Object1) : null;
                        DirectoryInfo directoryInfo2 = Directory.Exists(compareItemFolder.Object2) ? new DirectoryInfo(compareItemFolder.Object2) : null;
                      
                        string difference = "Different";
                        if (directoryInfo1 == null)
                        {
                            difference = "<- Not found";
                        }
                        else if (directoryInfo2 == null)
                        {
                            difference = "Not found ->";
                        }
                        else if (compareItemFiles.Count > 0)
                        {
                            bool isFilesMissing = false;
                            bool isFilesDifference = false;
                            foreach (CompareItemFile compareItemFile in compareItemFiles)
                            {
                                foreach (CompareItemFile.DifferenceTypes differenceType in compareItemFile.DifferenceTypeList)
                                {
                                    switch (differenceType)
                                    {
                                        case CompareItem.DifferenceTypes.File1NotExists:
                                        case CompareItem.DifferenceTypes.File2NotExists:
                                            isFilesMissing = true;
                                            break;
                                        case CompareItem.DifferenceTypes.FileContentsDifferent:
                                            isFilesDifference = true;
                                            break;
                                    }
                                }
                            }

                            if (isFilesMissing && isFilesDifference)
                            {
                                difference = "Files missing/different";
                            }
                            else if (isFilesMissing)
                            {
                                difference = "Files missing";
                            }
                            else if (isFilesDifference)
                            {
                                difference = "Files different";
                            }
                        }

                        string name1 = (directoryInfo1 == null ? directoryInfo2.Name : directoryInfo1.Name);
                        string name2 = (directoryInfo2 == null ? directoryInfo1.Name : directoryInfo2.Name);

                        string modified1 = (directoryInfo1 == null ? "" : directoryInfo1.LastWriteTime.ToString());
                        string modified2 = (directoryInfo2 == null ? "" : directoryInfo2.LastWriteTime.ToString());

                        string[] items = { compareItemFolder.Object1, name1, modified1, difference, compareItemFolder.Object2, name2, modified2 };
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
                                
                                string fileDifference = "Different";
                                if (fileInfo1 == null)
                                {
                                    fileDifference = "<- Not found";
                                }
                                else if (fileInfo2 == null)
                                {
                                    fileDifference = "Not found ->";
                                }                                

                                string fileName1 = (fileInfo1 == null ? fileInfo2.Name : fileInfo1.Name);
                                string fileName2 = (fileInfo2 == null ? fileInfo1.Name : fileInfo2.Name);

                                string fileModified1 = (fileInfo1 == null ? "" : fileInfo1.LastWriteTime.ToString());
                                string fileModified2 = (fileInfo2 == null ? "" : fileInfo2.LastWriteTime.ToString());

                                string[] fileItems = { "", fileName1, fileModified1, fileDifference, "", fileName2, fileModified2 };
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
                InternalUtilities.StartFileDiffTool(compareItemFile.Object1, compareItemFile.Object2);
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
    }
}
