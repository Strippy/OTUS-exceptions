using System;
using System.IO;

namespace ToDoText
{ public static class FileUtils
    {
        public static bool IsFilePathValid(string a_path)
        {
            if (a_path.Trim() == string.Empty)
            {
                return false;
            }

            string pathname;
            string filename;
            try
            {
                pathname = Path.GetPathRoot(a_path);
                filename = Path.GetFileName(a_path);
            }
            catch (ArgumentException)
            {
                return false;
            }

            if (filename.Trim() == string.Empty)
            {
                return false;
            }

            if (pathname.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }

            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return false;
            }

            return true;
        }
    }
}