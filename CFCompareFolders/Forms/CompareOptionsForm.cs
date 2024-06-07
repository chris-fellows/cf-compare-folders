using CFCompareFolders.Enums;
using CFCompareFolders.Models;
using CFCompareFolders.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CFCompareFolders.Forms
{
    /// <summary>
    /// Compare options form
    /// </summary>
    public partial class CompareOptionsForm : Form
    {
        private readonly CompareOptions _compareOptions;

        public CompareOptionsForm()
        {
            InitializeComponent();
        }

        public CompareOptionsForm(CompareOptions compareOptions)
        {            
            InitializeComponent();

            _compareOptions = compareOptions;
            ModelToView(_compareOptions);
        }

        private void ModelToView(CompareOptions compareOptions)
        {
            chkIncludeSubFolders.Checked = compareOptions.IncludeSubFolders;
            chkIncludeHiddenFolders.Checked = compareOptions.IncludeHiddenFolders;
            chkIncludeHiddenFiles.Checked = compareOptions.IncludeHiddenFiles;

            // Set differences
            clbDifferences.Items.Clear();
            foreach(DifferenceTypes differenceType in Enum.GetValues(typeof(DifferenceTypes)))
            {
                var description = InternalUtilities.GetEnumDescription(differenceType);
                clbDifferences.Items.Add(description, compareOptions.DifferenceTypes.Contains(differenceType));
            }
        }

        private void ViewToModel(CompareOptions compareOptions)
        {
            compareOptions.IncludeSubFolders = chkIncludeSubFolders.Checked;
            compareOptions.IncludeHiddenFolders = chkIncludeHiddenFolders.Checked;
            compareOptions.IncludeHiddenFiles = chkIncludeHiddenFiles.Checked;

            // Set differences
            compareOptions.DifferenceTypes = new List<DifferenceTypes>();
            var differenceTypes = Enum.GetValues(typeof(DifferenceTypes));
            foreach(DifferenceTypes differenceType in differenceTypes)
            {
                if (clbDifferences.GetItemChecked(Array.IndexOf(differenceTypes, differenceType)))
                {
                    compareOptions.DifferenceTypes.Add(differenceType);
                }
            }            
        }

        private void ApplyChanges()
        {
            ViewToModel(_compareOptions);
        }

        private List<string> ValidateBeforeApplyChanges()
        {
            var messages = new List<string>();

            return messages;               
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var messages = ValidateBeforeApplyChanges();
            if (messages.Any())
            {
                MessageBox.Show(messages[0], "Error");
            }
            else
            {
                ApplyChanges();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

}
