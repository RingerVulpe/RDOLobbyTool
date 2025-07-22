using System;
using System.IO;
using System.Windows.Forms;

namespace FileTool
{
    public class MainForm : Form
    {
        private readonly string _folderPath;
        private const string FileName = "toolfile.txt";

        public MainForm(string folderPath)
        {
            _folderPath = folderPath;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "File Tool";
            ClientSize = new System.Drawing.Size(280, 100);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            var addButton = new Button
            {
                Text = "Add File",
                Location = new System.Drawing.Point(30, 30),
                Size = new System.Drawing.Size(100, 30)
            };
            addButton.Click += AddButton_Click;

            var removeButton = new Button
            {
                Text = "Remove File",
                Location = new System.Drawing.Point(150, 30),
                Size = new System.Drawing.Size(100, 30)
            };
            removeButton.Click += RemoveButton_Click;

            Controls.Add(addButton);
            Controls.Add(removeButton);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            string fullPath = Path.Combine(_folderPath, FileName);
            File.WriteAllText(fullPath, "Sample content");
            MessageBox.Show($"Created {FileName}", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            string fullPath = Path.Combine(_folderPath, FileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                MessageBox.Show($"Deleted {FileName}", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"{FileName} not found", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
