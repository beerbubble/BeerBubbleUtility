using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace BeerBubbleUtility.ImageService
{
    sealed class ImageCommonConfig
    {
        public static readonly string Postfix = ".jpg";
        public static readonly string OPostfix = "_o.jpg";
        public static readonly string PngPostfix = ".png";
        public static readonly string OPngPostfix = "_o.png";
        public readonly static string UploadImageLocalFolder = ConfigurationManager.AppSettings["ImageLocalFolder"];
    }
}
