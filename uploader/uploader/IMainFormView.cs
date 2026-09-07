namespace uploader
{
    public interface IMainFormView
    {
        void ShowUploadForm(Settings settings, bool reopen, string file);
        void HideForm();
    }
}
