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
    public partial class SettingsForm : DarkForm
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            var settings = Settings.LoadSettings();

            apiTextbox.Text = settings.ApiKey;
            directCheckbox.Checked = settings.DirectUpload;

            var options = SettingsFormHelpers.GetLanguageOptions();
            languageCombo.Items.Clear();
            foreach (var option in options)
            {
                languageCombo.Items.Add(option);
            }

            languageCombo.SelectedIndex = SettingsFormHelpers.GetSelectedLanguageIndex(settings, options);

            generalGroupBox.Text = LocalizationHelper.Base.SettingsForm_General;
            apiLabel.Text = LocalizationHelper.Base.SettingsForm_Key;
            getApiButton.Text = LocalizationHelper.Base.SettingsForm_Get;
            languageLabel.Text = LocalizationHelper.Base.SettingsForm_Language;
            saveButton.Text = LocalizationHelper.Base.SettingsForm_Save;
            openButton.Text = LocalizationHelper.Base.SettingsForm_Open;
            this.Text = LocalizationHelper.Base.SettingsForm_Title;
            directCheckbox.Text = LocalizationHelper.Base.SettingsForm_DirectUpload;
        }

        private void darkButton1_Click(object sender, EventArgs e)
        {
            var file = Settings.GetSettingsFilename();
            if (!File.Exists(file))
            {
                statusLabel.Text = LocalizationHelper.Base.Message_NoSettings;
                return;
            }

            var safePath = file.Replace("\"", "\\\"");
            var args = $"/e, /select, \"{safePath}\"";

            var info = new ProcessStartInfo {FileName = "explorer", Arguments = args};
            Process.Start(info);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            SettingsFormHelpers.SaveSettings(apiTextbox.Text, languageCombo.Text, directCheckbox.Checked);

            using (var messageBox = new DarkMessageBox(LocalizationHelper.Base.Message_Saved, "Ok", DarkMessageBoxIcon.Information, DarkDialogButton.Ok))
            {
                messageBox.ShowDialog();
            }

            // Needs full restart to initialize main form strings again
            Application.Restart();
            Environment.Exit(0);
        }

        private void getApiButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://developers.virustotal.com/reference",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open URL: {ex.Message}");
                statusLabel.Text = "Failed to open URL. Please check your browser settings.";
            }
        }
    }
}
