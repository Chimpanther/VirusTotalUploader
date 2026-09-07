namespace uploader
{
    public class MainFormRouter
    {
        private readonly IMainFormView _view;

        public MainFormRouter(IMainFormView view)
        {
            _view = view;
        }

        public void HandleFilesDropped(Settings settings, string[] files)
        {
            if (files == null || files.Length == 0) return;

            foreach (var file in files)
            {
                _view.ShowUploadForm(settings, true, file);
                _view.HideForm();
            }
        }

        public void HandleCommandLineArgs(Settings settings, string[] args)
        {
            if (args != null && args.Length == 2)
            {
                var file = args[1];
                _view.ShowUploadForm(settings, false, file);
                _view.HideForm();
            }
        }
    }
}
