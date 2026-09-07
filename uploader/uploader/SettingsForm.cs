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
    public class MessageBoxOptions
    {
        public string Message { get; set; }
        public string Caption { get; set; }
    }

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

        public Settings CurrentSettings
        {
            get => new Settings
            {
                ApiKey = apiTextbox.Text,
                Language = languageCombo.Text,
                DirectUpload = directCheckbox.Checked
            };
            set
            {
                apiTextbox.Text = value.ApiKey;
                directCheckbox.Checked = value.DirectUpload;
            }
        }

        public void LoadLanguages(string[] languages)
        {
            languageCombo.Items.Clear();
            foreach (var language in languages)
            {
                languageCombo.Items.Add(language);
            }
        }

        public void SelectLanguageOrDefault(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                var defaultLanguage = languageCombo.Items.Add("Default (Build-in English)");
                languageCombo.SelectedIndex = defaultLanguage;
            }
            else
            {
                var index = languageCombo.Items.IndexOf(language);
                if (index != -1)
                {
                    languageCombo.SelectedIndex = index;
                }
            }
        }

        public void SetLocalization(SettingsFormLocalization loc)
        {
            generalGroupBox.Text = loc.General;
            apiLabel.Text = loc.Key;
            getApiButton.Text = loc.Get;
            languageLabel.Text = loc.Language;
            saveButton.Text = loc.Save;
            openButton.Text = loc.Open;
            this.Text = loc.Title;
            directCheckbox.Text = loc.DirectUpload;
        }

        public void ShowStatusMessage(MessageBoxOptions options)
        {
            statusLabel.Text = options.Message;
        }

        public void ShowSuccessMessage(MessageBoxOptions options)
        {
            using (var messageBox = new DarkMessageBox(options.Message, options.Caption, DarkMessageBoxIcon.Information, DarkDialogButton.Ok))
            {
                messageBox.ShowDialog();
            }
        }

        public void RevealInExplorer(MessageBoxOptions options)
        {
            Utils.RevealInExplorer(options.Message);
        }

        public void OpenUrl(MessageBoxOptions options)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = options.Message,
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
