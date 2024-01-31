using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CFCompareFolders
{
    public partial class FileUserControl : UserControl
    {
        private string _file;
        private int _lineNo;

        public FileUserControl()
        {
            InitializeComponent();
        }

        public string File
        {
            set
            {
                _file = value;
                DisplayFile();
            }
        }

        public int LineNo
        {
            set
            {
                _lineNo = value;
                dataGridView1.Rows[_lineNo].DefaultCellStyle.ForeColor = Color.Red;
            }
        }


        private void DisplayFile()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            int columnIndex = dataGridView1.Columns.Add("Line", "Line");
            columnIndex = dataGridView1.Columns.Add("Data", "Data");

            if (!String.IsNullOrEmpty(_file))
            {
                using (StreamReader reader = new StreamReader(_file))
                {
                    int lineCount = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        lineCount++;

                        using (DataGridViewRow row = new DataGridViewRow())
                        {
                            using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                            {
                                cell.Value = lineCount;
                                row.Cells.Add(cell);
                            }
                            using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                            {
                                cell.Value = line;
                                row.Cells.Add(cell);
                            }
                            dataGridView1.Rows.Add(row);
                        }
                    }
                    reader.Close();
                }
                dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            }
        }
    }
}
