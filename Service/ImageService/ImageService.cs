using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleUtility.ImageService
{
    public class ImageService
    {

        public static string UploadImageToLocal(string bucketName,byte[] imageBytes)
        {

            string filePath = GenerateDateFilename() + ImageCommonConfig.Postfix;

            var localFilePath = ImageCommonConfig.UploadImageLocalFolder + bucketName + '/' + filePath;
            FileHelper.SaveFileBinary(imageBytes, localFilePath);
            return localFilePath;
        }

        private void SaveImage(byte[] content, string file)
        {
            string[] filepaths = file.IndexOf('|') > 0 ? file.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries) : new[] { file };
            
            foreach (string filepath in filepaths)
            {
                FileHelper.CheckDirectory(filepath);
                using (FileStream fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(content);
                    }
                }
            }
        }

        public virtual string GetImageFullPath(string imagePath)
        {
            return "/Image/" + imagePath;
        }

        private static string GenerateFilename()
        {
            return DateTime.Now.ToString("HHmmssffff") + "." + new Random().Next(10000000, 99999999);
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
