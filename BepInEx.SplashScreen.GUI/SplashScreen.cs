using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace BepInEx.SplashScreen
{
    public partial class SplashScreen : Form
    {
        private const string WorkingStr = "...";
        private const string DoneStr = "...Done";
        private string _gameLocation;
        private int _pluginPercentDone;



        public SplashScreen()
        {
            InitializeComponent();

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100 + checkedListBox1.Items.Count * 15;
            progressBar1.Value = 0;

            AppendToItem(0, WorkingStr);
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
                    break;

                case LoadEvent.LoadFinished:
                    //AppendToItem(3, "Done");
                    //checkedListBox1.SetItemCheckState(3, CheckState.Checked);
                    Environment.Exit(0);
                    return;

                default:
                    return;
            }

            UpdateProgress();
            checkedListBox1.Invalidate();
        }

        private void AppendToItem(int index, string str)
        {
            var current = checkedListBox1.Items[index].ToString();
            checkedListBox1.Items[index] = current + str;
        }

        private void UpdateProgress()
        {
            progressBar1.Value = checkedListBox1.CheckedItems.Count * 15 + _pluginPercentDone;
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

        public void SetIcon(Image icon)
        {

        }



        private void Button1_Click(object sender, EventArgs e)
        {
            if (Program.isGameLoaded == false)
            {
                Process.Start(_gameLocation);
            }
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (Program.isGameLoaded == false)
            {
                dragging = true;
                dragCursorPoint = Cursor.Position;
                dragFormPoint = this.Location;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (Program.isGameLoaded == false)
            {
                if (dragging)
                {
                    Point diff = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                    this.Location = Point.Add(dragFormPoint, new Size(diff));
                }
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        public void SetPluginProgress(int percentDone)
        {
            _pluginPercentDone = Math.Min(100, Math.Max(Math.Max(0, percentDone), _pluginPercentDone));
            UpdateProgress();
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            this.TopMost = true;
        }

    }
}
