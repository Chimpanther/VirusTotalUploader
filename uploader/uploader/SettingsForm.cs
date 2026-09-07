using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Forms;

namespace uploader
{
    public partial class SettingsForm : DarkForm, ISettingsView
    {
        private readonly SettingsPresenter _presenter;

        public SettingsForm()
        {
            InitializeComponent();
            _presenter = new SettingsPresenter(this);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            _presenter.Load();
        }

        private void darkButton1_Click(object sender, EventArgs e)
        {
            _presenter.OpenSettingsFile();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            _presenter.SaveSettings();
        }

        private void getApiButton_Click(object sender, EventArgs e)
        {
            _presenter.GetApiKey();
        }

        public string ApiKey
        {
            get => apiTextbox.Text;
            set => apiTextbox.Text = value;
        }

        public bool DirectUpload
        {
            get => directCheckbox.Checked;
            set => directCheckbox.Checked = value;
        }

        public string Language
        {
            get => languageCombo.Text;
            set => languageCombo.Text = value;
        }

        public void ClearLanguages() => languageCombo.Items.Clear();

        public void AddLanguage(string language) => languageCombo.Items.Add(language);

        public int AddLanguageAndGetIndex(string language) => languageCombo.Items.Add(language);

        public void SetLanguageSelectedIndex(int index) => languageCombo.SelectedIndex = index;

        public int GetLanguageIndexOf(string language) => languageCombo.Items.IndexOf(language);

        public void SetLocalization(LocalizationBase loc)
        {
            generalGroupBox.Text = loc.SettingsForm_General;
            apiLabel.Text = loc.SettingsForm_Key;
            getApiButton.Text = loc.SettingsForm_Get;
            languageLabel.Text = loc.SettingsForm_Language;
            saveButton.Text = loc.SettingsForm_Save;
            openButton.Text = loc.SettingsForm_Open;
            this.Text = loc.SettingsForm_Title;
            directCheckbox.Text = loc.SettingsForm_DirectUpload;
        }

        public void ShowStatusMessage(string message)
        {
            statusLabel.Text = message;
        }

        public void ShowMessageBox(string message, string caption)
        {
            using (var messageBox = new DarkMessageBox(message, caption, DarkMessageBoxIcon.Information, DarkDialogButton.Ok))
            {
                messageBox.ShowDialog();
            }
        }

        public void RevealInExplorer(string path)
        {
            Utils.RevealInExplorer(path);
        }

        public void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public void RestartApplication()
        {
            Application.Restart();
            Environment.Exit(0);
        }
    }
}
