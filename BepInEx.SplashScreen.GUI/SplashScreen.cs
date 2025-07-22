using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using System.IO;


public static class MathUtils
{
    public static float Clamp(float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }

    public static int Clamp(int value, int min, int max)
    {
        return value < min ? min : value > max ? max : value;
    }

    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * MathUtils.Clamp(t, 0, 1);
    }
}




public enum ProgressBarCurve
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut,
    SmootherStep,
    Exponential,
    Elastic,
    Bounce,
    BackIn,
    BackOut,
    Spring
}



public class CustomProgressBar : ProgressBar
{
    private Color _barColor;
    private Color _backgroundColor;
    private Color _borderColor;
    private int _borderSize;
    private int _smoothness;
    public int Smoothness
    {
        get => _smoothness;
        set
        {
            _smoothness = value;
        }
    }

    private ProgressBarCurve _curve;

    private ProgressBar _sourceProgressBar;
    private double _currentDisplayedValue;
    private double _velocity = 0;

    private Timer _animationTimer;

    private double _animationProgress = 1.0; // Valor de 0 a 1 para interpolar
    private double _lastTarget;

    private const int PROGRESS_HEIGHT = 10;

    public CustomProgressBar(ProgressBar source,
                             Color barColor,
                             Color backgroundColor,
                             Color borderColor,
                             int borderSize,
                             int smoothness,
                             ProgressBarCurve curve = ProgressBarCurve.SmootherStep)
    {
        _sourceProgressBar = source;
        _barColor = barColor;
        _backgroundColor = backgroundColor;
        _borderColor = borderColor;
        _borderSize = Math.Max(0, borderSize);
        _smoothness = Math.Max(0, Math.Min(100, smoothness));
        _curve = curve;

        _currentDisplayedValue = _sourceProgressBar.Value;
        _lastTarget = _currentDisplayedValue;

        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        this.Height = PROGRESS_HEIGHT;
        this.Dock = DockStyle.Bottom;
        this.Style = ProgressBarStyle.Continuous;

        _animationTimer = new Timer { Interval = 16 }; // ~60 FPS
        _animationTimer.Tick += (s, e) => AnimateProgress();
        _animationTimer.Start();
    }

    private double _animationStartValue;
    private double _animationTargetValue;
    private bool _isAnimating = false;

    private void AnimateProgress()
    {
        int target = MathUtils.Clamp(_sourceProgressBar.Value, _sourceProgressBar.Minimum, _sourceProgressBar.Maximum);

        if (_smoothness == 0)
        {
            _currentDisplayedValue = target;
            _isAnimating = false;
        }
        else
        {
            if (!_isAnimating || Math.Abs(target - _animationTargetValue) > 0.01)
            {
                _animationStartValue = _currentDisplayedValue;
                _animationTargetValue = target;
                _animationProgress = 0.0;
                _isAnimating = true;
            }

            if (_isAnimating)
            {
                double duration = 1.0 + (_smoothness / 100.0) * 4.0;
                double deltaTime = _animationTimer.Interval / 1000.0;

                _animationProgress += deltaTime / duration;
                if (_animationProgress >= 1.0)
                {
                    _animationProgress = 1.0;
                    _isAnimating = false;
                }

                double curvedT = ApplyCurveToT(_animationProgress, _curve);

                _currentDisplayedValue = Lerp(_animationStartValue, _animationTargetValue, curvedT);
            }
        }

        Invalidate();
    }


    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle outerRect = new Rectangle(0, 0, Width, Height);
        Rectangle innerRect = outerRect;

        if (_borderSize > 0)
            innerRect.Inflate(-_borderSize, -_borderSize);

        using (SolidBrush bgBrush = new SolidBrush(_backgroundColor))
            e.Graphics.FillRectangle(bgBrush, innerRect);

        if (_borderSize > 0)
        {
            using (Pen borderPen = new Pen(_borderColor, _borderSize))
            {
                Rectangle borderRect = new Rectangle(
                    _borderSize / 2,
                    _borderSize / 2,
                    Width - _borderSize,
                    Height - _borderSize
                );
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }
        }

        if (_currentDisplayedValue > 0)
        {
            double percent = (_currentDisplayedValue - Minimum) / (double)(Maximum - Minimum);
            int progressWidth = (int)(innerRect.Width * percent);

            if (progressWidth > 0)
            {
                Rectangle progressRect = new Rectangle(
                    innerRect.Left,
                    innerRect.Top,
                    progressWidth,
                    innerRect.Height
                );

                using (SolidBrush progressBrush = new SolidBrush(_barColor))
                    e.Graphics.FillRectangle(progressBrush, progressRect);
            }
        }
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        base.SetBoundsCore(x, y, width, PROGRESS_HEIGHT, specified);
    }

    private double ApplyCurveToT(double t, ProgressBarCurve curve)
    {
        t = Math.Max(0, Math.Min(1, t));

        switch (curve)
        {
            case ProgressBarCurve.EaseIn:
                return t * t;

            case ProgressBarCurve.EaseOut:
                return 1 - (1 - t) * (1 - t);

            case ProgressBarCurve.EaseInOut:
                return t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;

            case ProgressBarCurve.SmootherStep:
                return t * t * t * (t * (t * 6 - 15) + 10);

            case ProgressBarCurve.Exponential:
                return t == 0 ? 0 : Math.Pow(2, 10 * (t - 1));

            case ProgressBarCurve.Elastic:
                if (t == 0 || t == 1) return t;
                double c4 = (2 * Math.PI) / 3;
                return -Math.Pow(2, 10 * t - 10) * Math.Sin((t * 10 - 10.75) * c4);

            case ProgressBarCurve.Bounce:
                if (t < 1 / 2.75) return 7.5625 * t * t;
                else if (t < 2 / 2.75) return 7.5625 * (t -= 1.5 / 2.75) * t + 0.75;
                else if (t < 2.5 / 2.75) return 7.5625 * (t -= 2.25 / 2.75) * t + 0.9375;
                else return 7.5625 * (t -= 2.625 / 2.75) * t + 0.984375;

            case ProgressBarCurve.BackIn:
                {
                    double s = 1.70158;
                    return t * t * ((s + 1) * t - s);
                }

            case ProgressBarCurve.BackOut:
                {
                    double s = 1.70158;
                    t -= 1;
                    return t * t * ((s + 1) * t + s) + 1;
                }

            case ProgressBarCurve.Spring:
                return Math.Sin(t * Math.PI * (0.2 + 2.5 * t * t * t)) * Math.Pow(1 - t, 2.2) + t;

            case ProgressBarCurve.Linear:
            default:
                return t;
        }
    }

    private double Lerp(double a, double b, double t)
    {
        return a + (b - a) * MathUtils.Clamp((float)t, 0f, 1f);
    }
}


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
        public bool _closedByScript = false;


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
            progressBar1.Maximum = 100 + checkedListBox1.Items.Count * 10;
            progressBar1.Value = 0;

            // Force window to appear in taskbar
            this.ShowInTaskbar = true;
            int currentStyle = GetWindowLong(this.Handle, GWL_EXSTYLE).ToInt32();
            SetWindowLong(this.Handle, GWL_EXSTYLE, currentStyle | WS_EX_APPWINDOW);
        }
        public static int SplashScreenExtraWaitTime => int.Parse(GetBepInExConfigValue("2. Window", "ExtraWaitTime", "0"));

        private void UpdateProgress()
        {
            int newValue = checkedListBox1.CheckedItems.Count * 10 + _pluginPercentDone;
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
                    SetStatusMain("BepInEx patchers are being applied...");
                    break;

                case LoadEvent.PreloaderFinish:
                    checkedListBox1.SetItemChecked(0, true);
                    SetStatusMain("Finished applying patchers.");
                    SetStatusDetail("Plugins should start loading soon.");//\nIn case loading is stuck, check your entry point.");
                    break;

                case LoadEvent.ChainloaderStart:
                    SetStatusMain("BepInEx plugins are being loaded...");
                    break;

                case LoadEvent.ChainloaderFinish:
                    _pluginPercentDone = 100;
                    SetStatusMain("Finished loading plugins.");
                    SetStatusDetail("Waiting for the game to start...");//\nSome plugins might need more time to finish loading.");
                                                                        //Game window starts?
                                                                        //this.ShowInTaskbar = false; //Moved to program.cs 315

                    break;

                case LoadEvent.LoadFinished:


                    checkedListBox1.SetItemChecked(1, true);
                    int newValue = checkedListBox1.CheckedItems.Count * 10 + _pluginPercentDone;


                    double minWait = 5;
                    double maxWait = 60;
                    double maxAddition = 25;

                    double addition = 0;

                    if (SplashScreenExtraWaitTime > minWait)
                    {
                        // Interpolación lineal del aumento
                        double t = (SplashScreenExtraWaitTime - minWait) / (maxWait - minWait);
                        t = Math.Min(t, 1); // Clamp a 1 para evitar superar los 10 segundos
                        addition = t * maxAddition;
                    }

                    newProgressBar.Smoothness = (int)((SplashScreenExtraWaitTime + addition) * 10);
                    _closedByScript = true; //In case someone does ALT F4 to the loading screen, not close the game


                    System.Threading.Thread.Sleep(10);

                    progressBar1.Value = newValue;

                    System.Threading.Thread.Sleep(SplashScreenExtraWaitTime * 1000 - 10);

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


        class DwmApi
        {
            public enum DWMWINDOWATTRIBUTE : uint
            {
                DWMWA_CAPTION_COLOR = 35,
                DWMWA_USE_IMMERSIVE_DARK_MODE = 20
            }

            [DllImport("dwmapi.dll")]
            public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref uint attrValue, int attrSize);

            [DllImport("dwmapi.dll")]
            public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);
        }

        // This works only on Windows 10 (build 1809) or later.
        public void SetTitleBarColor(Color color)
        {
            uint colorRef = (uint)((color.R) | (color.G << 8) | (color.B << 16));
            IntPtr hwnd = this.Handle;
            DwmApi.DwmSetWindowAttribute(hwnd, DwmApi.DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, ref colorRef, sizeof(uint));
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
        public static string SplashScreenWindowType => GetBepInExConfigValue("2. Window", "WindowType", "FakeGame");
        public static int SplashScreenWindowWidth => int.Parse(GetBepInExConfigValue("2. Window", "WindowWidth", "640"));

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


        private CustomProgressBar newProgressBar;

        private void ConfigureLayout(int fixedWidth, int scaledHeight, PictureBoxSizeMode sizeMode)
        {
            // Window Settings
            //string windowType = GetBepInExConfigValue("2. Window", "WindowType", "FakeGame");
            //int windowWidth = int.Parse(GetBepInExConfigValue("2. Window", "WindowWidth", "640"));
            //int extraWaitTime = int.Parse(GetBepInExConfigValue("2. Window", "ExtraWaitTime", "1"));
            string titleBarColorHex = GetBepInExConfigValue("2. Window", "TitleBarColor", "#FFFFFF");
            string backgroundColorHex = GetBepInExConfigValue("2. Window", "BackgroundColor", "#000000");

            // Text Settings
            string textColorHex = GetBepInExConfigValue("3. Text", "TextColor", "#FFFFFF");
            string textFontName = GetBepInExConfigValue("3. Text", "TextFont", "Segoe UI");
            string textBackgroundColorHex = GetBepInExConfigValue("3. Text", "TextBackgroundColor", "#595959");

            // Progress Bar Settings
            bool useCustomProgressBar = bool.Parse(GetBepInExConfigValue("4. ProgressBar", "UseCustomProgressBar", "true"));
            string progressBarColorHex = GetBepInExConfigValue("4. ProgressBar", "ProgressBarColor", "#34b233");
            string progressBarBackgroundColorHex = GetBepInExConfigValue("4. ProgressBar", "ProgressBarBackgroundColor", "#FFFFFF");
            int progressBarBorderSize = int.Parse(GetBepInExConfigValue("4. ProgressBar", "ProgressBarBorderSize", "0"));
            string progressBarBorderColorHex = GetBepInExConfigValue("4. ProgressBar", "ProgressBarBorderColor", "#FFFFFF");
            int progressBarSmoothness = int.Parse(GetBepInExConfigValue("4. ProgressBar", "ProgressBarSmoothness", "5"));
            string progressBarCurve = GetBepInExConfigValue("4. ProgressBar", "ProgressBarCurve", "Linear");

            // Convert colors
            Color backgroundColor = ParseColorHex(backgroundColorHex, Color.Black);
            Color textBackgroundColor = ParseColorHex(textBackgroundColorHex, Color.FromArgb(89, 89, 89));
            Color textColor = ParseColorHex(textColorHex, Color.White);
            Color titleBarColor = ParseColorHex(titleBarColorHex, Color.White);
            Color progressBarColor = ParseColorHex(progressBarColorHex, Color.LimeGreen);
            Color progressBarBackgroundColor = ParseColorHex(progressBarBackgroundColorHex, Color.White);
            Color progressBarBorderColor = ParseColorHex(progressBarBorderColorHex, Color.White);
            ProgressBarCurve progressBarCurveEnum;

            if (titleBarColor != Color.White)
            {
                SetTitleBarColor(titleBarColor);
            }


            try
            {
                progressBarCurveEnum = (ProgressBarCurve)Enum.Parse(typeof(ProgressBarCurve), progressBarCurve, true);
            }
            catch
            {
                progressBarCurveEnum = ProgressBarCurve.Linear;
            }

            if (useCustomProgressBar)
            {
                // Replace with custom progress bar for hex colors
                newProgressBar = new CustomProgressBar(
                progressBar1,
                progressBarColor,
                progressBarBackgroundColor,
                progressBarBorderColor,
                progressBarBorderSize,
                progressBarSmoothness,
                progressBarCurveEnum
                );

                // Copy properties from old progress bar
                newProgressBar.Name = "newProgressBar";
                newProgressBar.Minimum = progressBar1.Minimum;
                newProgressBar.Maximum = progressBar1.Maximum;

                // Create and add new progress bar
                Controls.Add(pictureBox1);
                Controls.Add(newProgressBar);
                Controls.Add(labelBot);

                // Remove and dispose old progress bar
                progressBar1.Visible = false;
                //Controls.Remove(progressBar1);
                //progressBar1.Dispose();
                //progressBar1 = newProgressBar;

            }

            int labelHeight = 30;
            int progressHeight = 10;
            int totalHeight = scaledHeight + labelHeight + progressHeight;

            this.ClientSize = new Size(fixedWidth, totalHeight);

            pictureBox1.Size = new Size(fixedWidth, scaledHeight);
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.SizeMode = sizeMode;
            pictureBox1.BackColor = backgroundColor;

            labelBot.Size = new Size(fixedWidth, labelHeight);
            labelBot.Location = new Point(0, scaledHeight);

            try
            {
                labelBot.Font = new Font(textFontName, 11, FontStyle.Bold);
            }
            catch
            {
                labelBot.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            }

            labelBot.BackColor = textBackgroundColor;
            labelBot.ForeColor = textColor;
            labelBot.TextAlign = ContentAlignment.MiddleCenter;

            progressBar1.Size = new Size(fixedWidth, progressHeight);
            progressBar1.Location = new Point(0, scaledHeight + labelHeight);

            CenterWindow();
        }

        private Color ParseColorHex(string hexColor, Color defaultColor)
        {
            try
            {
                if (!hexColor.StartsWith("#"))
                    hexColor = "#" + hexColor;

                return ColorTranslator.FromHtml(hexColor);
            }
            catch
            {
                return defaultColor;
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

        public void SetIconImage(string imagePath)
        {
            Image img = Image.FromFile(imagePath);

            int fixedWidth = SplashScreenWindowWidth;

            float scale = (float)fixedWidth / img.Width;
            int scaledHeight = (int)(img.Height * scale);

            ConfigureLayout(fixedWidth, scaledHeight, PictureBoxSizeMode.Zoom);

            pictureBox1.Image = img;
        }

        private void UseFallbackIcon(Image icon)
        {
            if (icon != null)
            {
                int fixedWidth = SplashScreenWindowWidth;

                // Hardcoded base resolution 640x360
                float scale = (float)fixedWidth / 640;
                int scaledHeight = (int)(360 * scale);

                //PictureBoxSizeMode mode = icon.Height < pictureBox1.Height ? PictureBoxSizeMode.CenterImage : PictureBoxSizeMode.Zoom;

                ConfigureLayout(fixedWidth, scaledHeight, PictureBoxSizeMode.CenterImage);

                pictureBox1.Image = icon;
            }
        }
        public void SetPluginProgress(int percentDone)
        {
            _pluginPercentDone = Math.Min(100, Math.Max(Math.Max(0, percentDone), _pluginPercentDone));
            UpdateProgress();
        }
    }
}


