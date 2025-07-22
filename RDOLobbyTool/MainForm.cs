using System;
using System.IO;
using System.Windows.Forms;
using System.Media;
using System.Drawing; // Add this for Image
using System.Net.Http; // Add this at the top
using System.Reflection; // For getting the current version

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
            ClientSize = new System.Drawing.Size(260, 140);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            var addBtn = new Button {
                Text = "Add File",
                Location = new System.Drawing.Point(20, 30),
                Size = new System.Drawing.Size(100, 30)
            };
            addBtn.Click += AddBtn_Click;

            var remBtn = new Button {
                Text = "Remove File",
                Location = new System.Drawing.Point(140, 30),
                Size = new System.Drawing.Size(100, 30)
            };
            remBtn.Click += RemBtn_Click;

            var horseBtn = new Button {
                Text = "Horse Neigh",
                Location = new System.Drawing.Point(20, 80),
                Size = new System.Drawing.Size(220, 30)
            };
            horseBtn.Click += HorseBtn_Click;

            Controls.Add(addBtn);
            Controls.Add(remBtn);
            Controls.Add(horseBtn);
        }

        private void SetBackgroundImage()
        {
            var imagePath = Path.Combine(AppContext.BaseDirectory, "images", "background.jpg");
            if (File.Exists(imagePath))
            {
                try
                {
                    BackgroundImage = Image.FromFile(imagePath);
                    BackgroundImageLayout = ImageLayout.Stretch; // or Tile, Center, Zoom, etc.
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
