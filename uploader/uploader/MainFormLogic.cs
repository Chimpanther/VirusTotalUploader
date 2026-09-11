using System;

namespace uploader
{
    public static class MainFormLogic
    {
        public static string[] GetDroppedFiles(object data)
        {
            var files = data as string[];
            if (files == null || files.Length == 0)
                return null;

            return files;
        }

        public static string TryGetFileFromArgs(string[] args)
        {
            if (args != null && args.Length == 2)
            {
                return args[1]; // Second argument because .NET puts program filename to the first
            }

            return null;
        }
    }
}
