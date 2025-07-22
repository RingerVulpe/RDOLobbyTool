using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Net.Http;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;  

namespace FileTool
{
    public class MainForm : Form
    {
        private readonly string _targetFolder;
        private const string FileName = "startup.meta";
        private const string VersionFileName = "version.txt";

        public MainForm(string targetFolder)
        {
            _targetFolder = targetFolder;
            InitializeComponent();
            SetBackgroundImage();

            var iconPath = Path.Combine(AppContext.BaseDirectory, "images", "RDOLobby.ico");
            if (File.Exists(iconPath))
            {
                this.Icon = new Icon(iconPath);
            }

            CheckForUpdates(); // <-- Add this line
        }

        private void InitializeComponent()
        {
            Text = "RDO Lobby Tool";
            ClientSize = new System.Drawing.Size(340, 240); // 16:11 aspect ratio, adjust as needed
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Main action buttons
            int buttonWidth = 180;
            int buttonHeight = 36;
            int leftMargin = (ClientSize.Width - buttonWidth) / 2;
            int topMargin = 30;
            int verticalSpacing = 12;

            var addBtn = new Button {
                Text = "Add File",
                Location = new System.Drawing.Point(leftMargin, topMargin),
                Size = new System.Drawing.Size(buttonWidth, buttonHeight)
            };
            addBtn.Click += AddBtn_Click;

            var remBtn = new Button {
                Text = "Remove File",
                Location = new System.Drawing.Point(leftMargin, topMargin + (buttonHeight + verticalSpacing) * 1),
                Size = new System.Drawing.Size(buttonWidth, buttonHeight)
            };
            remBtn.Click += RemBtn_Click;

            var horseBtn = new Button {
                Text = "Horse Neigh",
                Location = new System.Drawing.Point(leftMargin, topMargin + (buttonHeight + verticalSpacing) * 2),
                Size = new System.Drawing.Size(buttonWidth, buttonHeight)
            };
            horseBtn.Click += HorseBtn_Click;

            // Update button: small, bottom right
            var updateBtn = new Button {
                Text = "Check for Updates",
                Size = new System.Drawing.Size(120, 28),
                Location = new System.Drawing.Point(ClientSize.Width - 120 - 16, ClientSize.Height - 28 - 16),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            updateBtn.Click += UpdateBtn_Click;

            Controls.Add(addBtn);
            Controls.Add(remBtn);
            Controls.Add(horseBtn);
            Controls.Add(updateBtn);
        }

        private void SetBackgroundImage()
        {
            var imagePath = Path.Combine(AppContext.BaseDirectory, "images", "background.jpg");
            if (File.Exists(imagePath))
            {
                try
                {
                    BackgroundImage = Image.FromFile(imagePath);
                    BackgroundImageLayout = ImageLayout.Stretch; // Fills the form, no borders
                }
                catch
                {
                    // Optionally handle image load errors
                }
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var source = Path.Combine(AppContext.BaseDirectory, "Templates", FileName);
            var dest   = Path.Combine(_targetFolder, FileName);

            try
            {
                File.Copy(source, dest, true);
                MessageBox.Show($"Copied to:\n{dest}", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            var ps = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-ExecutionPolicy Bypass -File update.ps1",
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false
            };
            Process.Start(ps);
        }

        private void RemBtn_Click(object sender, EventArgs e)
        {
            var target = Path.Combine(_targetFolder, FileName);
            if (File.Exists(target))
            {
                File.Delete(target);
                MessageBox.Show("File deleted", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("File not found", "Oops",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void HorseBtn_Click(object sender, EventArgs e)
        {
            var soundPath = Path.Combine(AppContext.BaseDirectory, "sound", "horse.wav");
            if (File.Exists(soundPath))
            {
                try
                {
                    using var player = new SoundPlayer(soundPath);
                    player.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not play sound:\n{ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("horse.wav not found in the sound folder.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void CheckForUpdates()
        {
            string? latestVersion = null;

            try
            {
                using var http = new HttpClient();
                latestVersion = await http.GetStringAsync(VersionFileName);
                latestVersion = latestVersion.Trim();
            }
            catch
            {
                // Optionally handle network errors
                return;
            }

            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

            if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
            {
                MessageBox.Show(
                    $"A new version ({latestVersion}) is available!\nYou are running {currentVersion}.",
                    "Update Available",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                // Optionally, offer to download or open a URL
            }
        }
    }
}
