using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Timers;

namespace C_Drive_Cleanup
{
    public class MainForm : Form
    {
        private int postponeCount = 0;
        private const int maxPostpone = 3;
        private readonly string postponeFolderPath;
        private readonly string postponeFilePath;
        private Label countdownLabel = new(); // Initialize directly
        private Label messageLabel = new(); // Initialize directly
        private System.Timers.Timer countdownTimer = new(1000); // Initialize directly
        private DateTime targetTime;
        private bool isPostponed = false; // Flag to indicate if the window is being closed due to a postpone action
        private bool isConfirmed = false; // Flag to indicate if the window is being closed due to an OK action

        public MainForm()
        {
            InitializeComponent();
            // Initialize the paths in the constructor
            postponeFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "C_Drive_Cleanup");
            postponeFilePath = Path.Combine(postponeFolderPath, "PostponeCount.txt");
            postponeCount = GetPostponeCount();
            CleanUpTempTask(); // Clean up the temporary task if it exists
            UpdateCountdown();
            UpdateMessageLabel(); // Update the message label with the correct postpone count
        }

        private void InitializeComponent()
        {
            Text = "C Drive Clean Up";
            Width = 300;
            Height = 200;
            StartPosition = FormStartPosition.Manual;
            TopMost = true; // Make the window topmost

            // Set the window position to 45 pixels off from the bottom
            var primaryScreen = Screen.PrimaryScreen;
            if (primaryScreen != null)
            {
                Left = primaryScreen.WorkingArea.Width - Width - 10;
                Top = primaryScreen.WorkingArea.Height - Height - 30;
            }

            messageLabel.Width = 280;
            messageLabel.Height = 80;
            messageLabel.Top = 20;
            messageLabel.Left = 10;
            messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(messageLabel);

            countdownLabel.Width = 280;
            countdownLabel.Height = 20;
            countdownLabel.Top = 100;
            countdownLabel.Left = 10;
            countdownLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(countdownLabel);

            Button postponeButton = new()
            {
                Text = "Postpone",
                Top = 130,
                Left = 50
            };
            postponeButton.Click += new EventHandler(PostponeButton_Click);
            Controls.Add(postponeButton);

            Button okButton = new()
            {
                Text = "OK",
                Top = 130,
                Left = 150
            };
            okButton.Click += new EventHandler(OkButton_Click);
            Controls.Add(okButton);

            countdownTimer.Elapsed += OnTimedEvent;
            countdownTimer.AutoReset = true;
            countdownTimer.Enabled = true;

            FormClosing += new FormClosingEventHandler(MainForm_FormClosing); // Handle form closing event
        }

        private void PostponeButton_Click(object? sender, EventArgs e)
        {
            if (postponeCount < maxPostpone)
            {
                postponeCount++;
                SetPostponeCount(postponeCount);
                MessageBox.Show("The cleanup has been postponed by 1 hour.");

                // Create a new temporary task to trigger the original task in 1 hour
                string originalTaskName = $"{Environment.UserName}_CleanUpTask";
                string tempTaskName = $"{Environment.UserName}_TempCleanUpTask";

                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/Create /TN \"{tempTaskName}\" /TR \"schtasks /Run /TN \\\"{originalTaskName}\\\"\" /SC ONCE /ST {DateTime.Now.AddHours(1):HH:mm} /F",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                }
                catch
                {
                    Console.WriteLine("Error creating temporary task.");
                }

                isPostponed = true; // Set the flag to indicate the window is being closed due to a postpone action
                Close();
            }
            else
            {
                MessageBox.Show("You have reached the maximum number of postpones.");
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("The cleanup is starting now.", "Cleanup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            PerformCleanup();

            isConfirmed = true; // Set the flag to indicate the window is being closed due to an OK action
            Close();
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!isPostponed && !isConfirmed)
            {
                // Postpone by 1 minute if the window is closed without clicking a button
                string originalTaskName = $"{Environment.UserName}_CleanUpTask";
                string tempTaskName = $"{Environment.UserName}_TempCleanUpTask";

                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "schtasks",
                        Arguments = $"/Create /TN \"{tempTaskName}\" /TR \"schtasks /Run /TN \\\"{originalTaskName}\\\"\" /SC ONCE /ST {DateTime.Now.AddMinutes(1):HH:mm} /F",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(startInfo);
                }
                catch
                {
                    Console.WriteLine("Error creating temporary task.");
                }
            }
        }

        /* Unused function. Used to be for check for date when running daily
                private bool ShouldRunCleanup()
                {
                    return IsFirstSpecifiedDay(DayOfWeek.Monday) || IsOnDemand();
                }

                private bool IsFirstSpecifiedDay(DayOfWeek dayOfWeek)
                {
                    DateTime today = DateTime.Today;
                    DateTime firstSpecifiedDay = new(today.Year, today.Month, 1);

                    while (firstSpecifiedDay.DayOfWeek != dayOfWeek)
                    {
                        firstSpecifiedDay = firstSpecifiedDay.AddDays(1);
                    }

                    return today == firstSpecifiedDay;
                }


                private bool IsOnDemand()
                {
                    string taskName = $"{Environment.UserName}_CleanUpTask";
                    try
                    {
                        Process process = new();
                        process.StartInfo.FileName = "schtasks";
                        process.StartInfo.Arguments = $"/Query /TN \"{taskName}\"";
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();

                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();

                        return output.Contains("Ready") || output.Contains("Running");
                    }
                    catch
                    {
                        Console.WriteLine("Error querying scheduled task.");
                    }
                    return false;
                } */

        private int GetPostponeCount()
        {
            if (File.Exists(postponeFilePath))
            {
                return int.Parse(File.ReadAllText(postponeFilePath));
            }
            return 0;
        }

        private void SetPostponeCount(int count)
        {
            try
            {
                // Ensure the folder exists
                if (!Directory.Exists(postponeFolderPath))
                {
                    Directory.CreateDirectory(postponeFolderPath);
                }

                // Write to the file and set it as hidden
                File.WriteAllText(postponeFilePath, count.ToString());
                // File.SetAttributes(postponeFilePath, File.GetAttributes(postponeFilePath) | FileAttributes.Hidden);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show($"Access to the path '{postponeFilePath}' is denied. Please check your permissions.\n\n{ex.Message}", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformCleanup()
        {
            //TODO Uncomment
            // // Clean Downloads folder
            // string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            // foreach (var directory in Directory.GetDirectories(downloadsPath))
            // {
            //     Directory.Delete(directory, true);
            // }
            // foreach (var file in Directory.GetFiles(downloadsPath))
            // {
            //     File.Delete(file);
            // }

            // Clean current user's Recycle Bin using PowerShell
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process != null)
                {
                    process.WaitForExit();
                }
                else
                {
                    Console.WriteLine("Failed to start the process.");
                }
            }

            // Reset postpone count
            SetPostponeCount(0);
        }

        private void UpdateCountdown()
        {
            //TODO Change to correct time addminutes
            targetTime = DateTime.Now.AddSeconds(10); // Set the countdown target time to 10 minutes from now
            countdownTimer.Start();
        }

        private void OnTimedEvent(object? source, ElapsedEventArgs e)
        {
            TimeSpan remainingTime = targetTime - DateTime.Now;
            if (remainingTime.TotalSeconds > 0)
            {
                countdownLabel?.Invoke((MethodInvoker)delegate
                {
                    countdownLabel.Text = $"Time remaining: {remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
                });
            }
            else
            {
                countdownTimer.Stop();
                countdownLabel?.Invoke((MethodInvoker)delegate
                {
                    countdownLabel.Text = "Time's up!";
                });
                MessageBox.Show("The cleanup is starting now.", "Cleanup", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                PerformCleanup();

                isConfirmed = true; // Set the flag to indicate the window is being closed due to an OK action
                Close();
            }
        }

        private void CleanUpTempTask()
        {
            string tempTaskName = $"{Environment.UserName}_TempCleanUpTask";
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/Delete /TN \"{tempTaskName}\" /F",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process.Start(startInfo);
            }
            catch
            {
                Console.WriteLine("Error deleting temporary task.");
            }
        }

        private void UpdateMessageLabel()
        {
            messageLabel.Text = $"Your scheduled cleanup will start soon.\nClick 'Postpone' to delay by 1 hour or 'OK' to start now.\n\nPostpones left: {maxPostpone - postponeCount}";
        }
    }
}