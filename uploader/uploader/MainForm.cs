using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Forms;

namespace uploader
{
    public partial class MainForm : DarkForm, IMainFormView
    {
        private SettingsForm _settingsForm = new SettingsForm();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set working directory to exe location because of language files
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            LocalizationHelper.Update();

            dragLabel.Text = LocalizationHelper.Base.MainForm_DragFile;
            moreLabel.Text = LocalizationHelper.Base.MainForm_More;
        }

        private void moreLabel_Click(object sender, EventArgs e)
        {
            if (_settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm();
            }
            _settingsForm.Show();
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var settings = SettingsManager.LoadSettings();
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var router = new MainFormRouter(this);
            router.HandleFilesDropped(settings, files);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var settings = SettingsManager.LoadSettings();
            var args = Environment.GetCommandLineArgs();

            var router = new MainFormRouter(this);
            router.HandleCommandLineArgs(settings, args);
        }

        public void ShowUploadForm(Settings settings, bool reopen, string file)
        {
            var uploadForm = new UploadForm(this, settings, reopen, file);
            uploadForm.Show();
        }

        public void HideForm()
        {
            this.Hide();
        }
    }
}