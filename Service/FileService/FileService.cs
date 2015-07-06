using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleUtility
{
    public class FileService
    {
        public virtual string UploadFileToLocal(byte[] fileBytes, string extension)
        {
            string filePath = GenerateDateFilename() + extension;
            FileHelper.SaveFileBinary(fileBytes, FileCommonConfig.UploadFileLocalFolder + filePath);
            return filePath;
        }

        public virtual string GetImageFullPath(string fliePath)
        {
            return "/File/" + fliePath;
        }

        private static string GenerateFilename()
        {
            return DateTime.Now.ToString("HHmmss") + "." + new Random().Next(10000000, 99999999);
        }

        private static string GenerateFilepath()
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString("0000") + "/" + now.Month.ToString("00") + "/" + now.Day.ToString("00") + "/";
        }

        private static string GenerateDateFilename()
        {
            return GenerateFilepath() + GenerateFilename();
        }
    }
}
