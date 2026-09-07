using System;

namespace uploader
{
    public class Settings
    {
        public string ApiKey = "";
        public string Language = "";
        public bool DirectUpload = false;

        public Settings Clone()
        {
            return new Settings
            {
                ApiKey = this.ApiKey,
                Language = this.Language,
                DirectUpload = this.DirectUpload
            };
        }
    }
}
