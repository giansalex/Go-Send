using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GoSend
{
    class constants
    {
        public static string NameDB = "DbManager.sqlite";
        public static string ServerUri = "http://127.0.0.1/Project/GoSend/";

        public static string GetFolderByExt(string ext)
        {
            string[] Images = new[] { ".jpg", ".png", ".jpeg", ".bmp", ".gif" };
            string[] Video = new[] { ".mp4", ".wmv", ".avi"};
            string[] Audio = new[] { ".mp3", ".wav", ".acc", ".ogg", ".m4a"};

            ext = ext.ToLower();
            if (Images.Contains(ext))
                return "Imagenes";
            else if (Audio.Contains(ext))
                return "Audios";
            else if (Video.Contains(ext))
                return "Videos";
            else
             return "Files";
        }
    }
}
