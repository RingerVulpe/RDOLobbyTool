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

            CheckForUpdates();
        }

        private void LaunchUpdaterAndQuit()
        {
            int myPid = Process.GetCurrentProcess().Id;
            string script = Path.Combine(AppContext.BaseDirectory, "update.ps1");

            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-ExecutionPolicy Bypass -File \"{script}\" -installDir \"{AppContext.BaseDirectory}\" -pid {myPid}",
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = true
            };

            Process.Start(psi);
            Application.Exit();
        }

        private void InitializeComponent()
        {
            Text = "RDO Lobby Tool";
            ClientSize = new System.Drawing.Size(340, 240);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            int buttonWidth = 180;
            int buttonHeight = 36;
            int leftMargin = (ClientSize.Width - buttonWidth) / 2;
            int topMargin = 30;
            int verticalSpacing = 12;

            var addBtn = new Button
            {
                Text = "Add File",
                Location = new System.Drawing.Point(leftMargin, topMargin),
                Size = new System.Drawing.Size(buttonWidth, buttonHeight)
            };
            addBtn.Click += AddBtn_Click;

            var remBtn = new Button
            {
                Text = "Remove File",
                Location = new System.Drawing.Point(leftMargin, topMargin + (buttonHeight + verticalSpacing) * 1),
                Size = new System.Drawing.Size(buttonWidth, buttonHeight)
            };
            remBtn.Click += RemBtn_Click;

            var horseBtn = new Button
            {
                Text = "Horse Neigh",
                Location = new System.Drawing.Point(leftMargin, topMargin + (buttonHeight + verticalSpacing) * 2),
                Size = new System.Drawing.Size(buttonWidth, buttonHeight)
            };
            horseBtn.Click += HorseBtn_Click;

            var updateBtn = new Button
            {
                Text = "Check for Updates",
                Size = new System.Drawing.Size(120, 28),
                Location = new System.Drawing.Point(ClientSize.Width - 120 - 16, ClientSize.Height - 28 - 16),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            updateBtn.Click += UpdateBtn_Click;

            var toolTip = new ToolTip();
            toolTip.SetToolTip(addBtn, "Copy the template file to the target folder.");
            toolTip.SetToolTip(remBtn, "Remove the template file from the target folder.");
            toolTip.SetToolTip(horseBtn, "Play a horse neigh sound effect.");
            toolTip.SetToolTip(updateBtn, "Check for and install available updates.");

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
                    BackgroundImageLayout = ImageLayout.Stretch;
                }
                catch
                {
                    // ignore
                }
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var source = Path.Combine(AppContext.BaseDirectory, "Templates", FileName);
            var dest = Path.Combine(_targetFolder, FileName);

            try
            {
                File.Copy(source, dest, true);
                MessageBox.Show($"Copied to:\n{dest}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemBtn_Click(object sender, EventArgs e)
        {
            var target = Path.Combine(_targetFolder, FileName);
            if (File.Exists(target))
            {
                File.Delete(target);
                MessageBox.Show("File deleted", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("File not found", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show($"Could not play sound:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("horse.wav not found in the sound folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            LaunchUpdaterAndQuit();
        }

        private async void CheckForUpdates()
        {
            string latestVersion = null;
            try
            {
                using var http = new HttpClient();
                latestVersion = (await http.GetStringAsync(VersionFileName)).Trim();
            }
            catch
            {
                return;
            }

            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";

            if (!string.IsNullOrEmpty(latestVersion) && latestVersion != currentVersion)
            {
                var result = MessageBox.Show(
                    $"A new version ({latestVersion}) is available!\nYou are running {currentVersion}.\n\nUpdate now?",
                    "Update Available",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                if (result == DialogResult.Yes)
                    LaunchUpdaterAndQuit();
            }
        }

    }
}
