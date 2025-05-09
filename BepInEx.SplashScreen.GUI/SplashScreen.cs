﻿using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace BepInEx.SplashScreen
{
    public partial class SplashScreen : Form
    {
        private const uint WM_SETICON = 0x0080;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;

        private void SetExeIcon(Process gameProcess)
        {
            // Validate the process
            if (gameProcess == null || gameProcess.HasExited)
                return;

            try
            {
                // Get the game's executable path
                string gameExePath = gameProcess.MainModule?.FileName;

                if (!string.IsNullOrEmpty(gameExePath))
                {
                    // Extract the icon from the game's EXE
                    Icon gameIcon = Icon.ExtractAssociatedIcon(gameExePath);

                    // Apply to our window
                    this.Icon = gameIcon;
                    SendMessage(this.Handle, WM_SETICON, (IntPtr)ICON_SMALL, gameIcon.Handle);
                    SendMessage(this.Handle, WM_SETICON, (IntPtr)ICON_BIG, gameIcon.Handle);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to set game icon: {ex.Message}");
                // Fallback to our app icon
                this.Icon = SystemIcons.Application;
            }
        }
        // Windows API imports for taskbar progress
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("shell32.dll")]
        private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        // SendMessage
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Taskbar progress COM interface
        [ComImport()]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            void HrInit();
            void AddTab(IntPtr hwnd);
            void DeleteTab(IntPtr hwnd);
            void ActivateTab(IntPtr hwnd);
            void SetActiveAlt(IntPtr hwnd);
            void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);
            void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);
            void SetProgressState(IntPtr hwnd, TaskbarProgressState tbpFlags);
        }

        [Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
        [ClassInterface(ClassInterfaceType.None)]
        [ComImport()]
        private class TaskbarInstance { }

        private enum TaskbarProgressState
        {
            NoProgress = 0,
            Indeterminate = 0x1,
            Normal = 0x2,
            Error = 0x4,
            Paused = 0x8
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_APPWINDOW = 0x40000;
        private const string AppUserModelID = "BepInEx.SplashScreen";
        private ITaskbarList3 _taskbar;

        private const string WorkingStr = "...";
        private const string DoneStr = "...Done";
        private string _gameLocation;
        private int _pluginPercentDone;
        private readonly Action<string, bool> _logAction;
        private readonly Process _gameProcess;
        private bool _closedByScript = false;


        public SplashScreen(Action<string, bool> logAction, Process gameProcess)
        {
            _logAction = logAction;
            _gameProcess = gameProcess;
            InitializeComponent();
            SetExeIcon(_gameProcess);

            // Handle form closing
            this.FormClosed += (sender, e) =>
            {
                if (!_closedByScript && !_gameProcess.HasExited)
                {
                    _logAction?.Invoke("Splash screen closed unexpectedly - terminating game process", true);
                    _gameProcess.Kill();
                }
            };

            // Initialize taskbar progress (Windows 7+)
            try
            {
                _taskbar = (ITaskbarList3)new TaskbarInstance();
                _taskbar.HrInit();
            }
            catch
            {
                _taskbar = null;
            }

            // Set AppUserModelID
            SetCurrentProcessExplicitAppUserModelID(AppUserModelID);

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100 + checkedListBox1.Items.Count * 15;
            progressBar1.Value = 0;

            AppendToItem(0, WorkingStr);

            // Force window to appear in taskbar
            this.ShowInTaskbar = true;
            int currentStyle = GetWindowLong(this.Handle, GWL_EXSTYLE).ToInt32();
            SetWindowLong(this.Handle, GWL_EXSTYLE, currentStyle | WS_EX_APPWINDOW);
        }

        private void UpdateProgress()
        {
            int newValue = checkedListBox1.CheckedItems.Count * 15 + _pluginPercentDone;
            progressBar1.Value = newValue;

            // Update taskbar progress
            if (_taskbar != null)
            {
                _taskbar.SetProgressState(this.Handle, TaskbarProgressState.Normal);
                _taskbar.SetProgressValue(
                    this.Handle,
                    (ulong)newValue,
                    (ulong)progressBar1.Maximum
                );
            }
        }

        public void ProcessEvent(LoadEvent e)
        {
            switch (e)
            {
                case LoadEvent.PreloaderStart:
                    checkedListBox1.SetItemChecked(0, true);
                    AppendToItem(0, DoneStr);
                    AppendToItem(1, WorkingStr);
                    SetStatusMain("BepInEx patchers are being applied...");
                    break;

                case LoadEvent.PreloaderFinish:
                    checkedListBox1.SetItemChecked(1, true);
                    AppendToItem(1, DoneStr);
                    SetStatusMain("Finished applying patchers.");
                    SetStatusDetail("Plugins should start loading soon.");//\nIn case loading is stuck, check your entry point.");
                    break;

                case LoadEvent.ChainloaderStart:
                    AppendToItem(2, WorkingStr);
                    SetStatusMain("BepInEx plugins are being loaded...");
                    break;

                case LoadEvent.ChainloaderFinish:
                    _pluginPercentDone = 100;
                    checkedListBox1.SetItemChecked(2, true);
                    AppendToItem(2, DoneStr);
                    AppendToItem(3, WorkingStr);
                    SetStatusMain("Finished loading plugins.");
                    SetStatusDetail("Waiting for the game to start...");//\nSome plugins might need more time to finish loading.");
                                                                        //Game window starts?
                                                                        //this.ShowInTaskbar = false; //Moved to program.cs 315

                    break;

                case LoadEvent.LoadFinished:
                    //AppendToItem(3, "Done");
                    //checkedListBox1.SetItemCheckState(3, CheckState.Checked);
                    //Environment.Exit(0);
                    SafeClose();
                    return;

                default:
                    return;
            }

            UpdateProgress();
            checkedListBox1.Invalidate();
        }

        public void SafeClose()
        {
            _closedByScript = true;
            this.Close();
        }

        private void AppendToItem(int index, string str)
        {
            var current = checkedListBox1.Items[index].ToString();
            checkedListBox1.Items[index] = current + str;
        }

        public void SetStatusMain(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                labelBot.Text = msg;
            }
        }
        public void SetStatusDetail(string msg)
        {
            labelBot.Text = msg;
        }
        public static string GetBepInExConfigValue(string section, string key, string defaultValue = "")
        {
            string currentDir = Application.StartupPath;
            int levelsUp = 0;

            while (currentDir != null && levelsUp < 5)
            {
                string bepInExPath = Path.Combine(currentDir, "BepInEx");
                if (Directory.Exists(bepInExPath))
                {
                    string configDir = Path.Combine(bepInExPath, "config");
                    string configPath = Path.Combine(configDir, "Bertogim.LoadingScreen.cfg");

                    if (File.Exists(configPath))
                    {
                        string[] configLines = File.ReadAllLines(configPath);
                        bool inTargetSection = false;

                        foreach (string line in configLines)
                        {
                            string trimmedLine = line.Trim();

                            // Find the [LoadingScreen] section
                            if (trimmedLine.StartsWith($"[{section}]"))
                            {
                                inTargetSection = true;
                                continue;
                            }
                            else if (trimmedLine.StartsWith("[") && inTargetSection)
                            {
                                // Exit if another section is found
                                break;
                            }

                            // Find the key within the section
                            if (inTargetSection && trimmedLine.StartsWith(key))
                            {
                                int equalsIndex = trimmedLine.IndexOf('=');
                                if (equalsIndex > 0)
                                {
                                    string value = trimmedLine.Substring(equalsIndex + 1).Trim();
                                    return value;
                                }
                            }
                        }
                    }
                    return defaultValue;
                }

                // Go up one level in the directory
                currentDir = Directory.GetParent(currentDir)?.FullName;
                levelsUp++;
            }

            return defaultValue;
        }
        public static string SplashScreenWindowType => GetBepInExConfigValue("LoadingScreen", "WindowType", "FakeGame");
        public static int SplashScreenWindowWidth => int.Parse(GetBepInExConfigValue("LoadingScreen", "WindowWidth", "640"));

        public void CenterWindow()
        {
            this.StartPosition = FormStartPosition.Manual;

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            int formWidth = this.Width;
            int formHeight = this.Height;

            this.Location = new Point(
                (screenWidth - formWidth) / 2,
                (screenHeight - formHeight) / 2
            );
        }

        public void SetIconImage(string imagePath)
        {

            // Load the image from file
            Image img = Image.FromFile(imagePath);

            // Fixed width for the form
            int fixedWidth = SplashScreenWindowWidth;

            // Calculate scaled height to maintain aspect ratio
            float scale = (float)fixedWidth / img.Width;
            int scaledHeight = (int)(img.Height * scale);

            // Additional space for label and progress bar
            int labelHeight = 30;
            int progressHeight = 10;
            int totalHeight = scaledHeight + labelHeight + progressHeight;

            // Set form and picture box sizes
            this.ClientSize = new Size(fixedWidth, totalHeight);
            pictureBox1.Size = new Size(fixedWidth, scaledHeight);
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = img;

            // Adjust label and progress bar positions
            labelBot.Size = new Size(fixedWidth, labelHeight);
            labelBot.Location = new Point(0, scaledHeight);

            progressBar1.Size = new Size(fixedWidth, progressHeight);
            progressBar1.Location = new Point(0, scaledHeight + labelHeight);

            CenterWindow();
        }

        public static string BepInExRootPath
        {
            get
            {
                // Try to find BepInEx directory by searching upwards
                string currentDir = Application.StartupPath;
                int levelsUp = 0;

                while (currentDir != null && levelsUp < 5)
                {
                    string bepInExPath = Path.Combine(currentDir, "BepInEx");
                    if (Directory.Exists(bepInExPath))
                    {
                        return bepInExPath;
                    }

                    // Move up one directory level
                    currentDir = Directory.GetParent(currentDir)?.FullName;
                    levelsUp++;
                }

                // Return empty string
                return "";
            }
        }

        public void SetIcon(Image fallbackIcon)
        {
            try
            {

                if (SplashScreenWindowType == "FakeGame")
                {
                    //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle; //Already default
                    this.TopMost = false; //Till the game window shows up
                }
                if (SplashScreenWindowType == "FixedWindow")
                {
                    this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    this.ShowInTaskbar = false;
                }

                // Check if root path is null or empty
                if (string.IsNullOrEmpty(BepInExRootPath))
                {
                    _logAction?.Invoke("Couldn't find BepInEx root directory, using fallback icon", true);
                    UseFallbackIcon(fallbackIcon);
                    return;
                }

                // 1. First check in patchers/Bertogim-LoadingScreen for any image file
                string patchersPath = Path.Combine(BepInExRootPath, "patchers");
                string patcherFolderPath = Path.Combine(patchersPath, "Bertogim-LoadingScreen");

                if (Directory.Exists(patcherFolderPath))
                {
                    string[] patcherImages = Directory.GetFiles(patcherFolderPath, "*.png");
                    if (patcherImages.Length > 0)
                    {
                        _logAction?.Invoke("Using loading image: " + patcherImages[0], false);
                        SetIconImage(patcherImages[0]);
                        return;
                    }
                }

                // 2. Then check in plugins/*/LoadingScreen/LoadingImage.png
                string pluginsPath = Path.Combine(BepInExRootPath, "plugins");
                if (Directory.Exists(pluginsPath))
                {
                    string[] loadingScreenFolders = Directory.GetDirectories(pluginsPath, "LoadingScreen", SearchOption.AllDirectories);
                    foreach (var folder in loadingScreenFolders)
                    {
                        string imagePath = Path.Combine(folder, "LoadingImage.png");
                        if (File.Exists(imagePath))
                        {
                            _logAction?.Invoke("Using loading image: " + imagePath, false);
                            SetIconImage(imagePath);
                            return;
                        }
                    }
                }

                // If all checks fail, use fallback
                _logAction?.Invoke("No suitable loading image found, using fallback icon", false);
                UseFallbackIcon(fallbackIcon);
            }
            catch (Exception ex)
            {
                _logAction?.Invoke($"Error setting icon: {ex.Message}", true);
                UseFallbackIcon(fallbackIcon);
            }
        }

        // Helper function to set the fallback icon
        private void UseFallbackIcon(Image icon)
        {
            if (icon != null)
            {
                // Fixed width for the form
                int fixedWidth = SplashScreenWindowWidth;

                // Calculate scaled height to maintain aspect ratio
                float scale = (float)fixedWidth / 640;
                int scaledHeight = (int)(360 * scale);

                // Additional space for label and progress bar
                int labelHeight = 30;
                int progressHeight = 10;
                int totalHeight = scaledHeight + labelHeight + progressHeight;

                // Set form and picture box sizes
                this.ClientSize = new Size(fixedWidth, totalHeight);

                pictureBox1.SizeMode = icon.Height < pictureBox1.Height ? PictureBoxSizeMode.CenterImage : PictureBoxSizeMode.Zoom;
                pictureBox1.Image = icon;

                // Adjust label and progress bar positions
                labelBot.Size = new Size(fixedWidth, labelHeight);
                labelBot.Location = new Point(0, scaledHeight);

                progressBar1.Size = new Size(fixedWidth, progressHeight);
                progressBar1.Location = new Point(0, scaledHeight + labelHeight);

                CenterWindow();
            }
        }

        public void SetPluginProgress(int percentDone)
        {
            _pluginPercentDone = Math.Min(100, Math.Max(Math.Max(0, percentDone), _pluginPercentDone));
            UpdateProgress();
        }

        protected override void OnActivated(EventArgs e) // Is this even used?
        {
            base.OnActivated(e);
            this.TopMost = true;
        }

    }
}


