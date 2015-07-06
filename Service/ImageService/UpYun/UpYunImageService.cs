using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Globalization;
using System.Web;
using System.Configuration;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using BeerBubbleUtility;

namespace BeerBubbleUtility.ImageService
{
    public class UpYunImageService : IImageService

    {
        private string username = UpYunConfig.UserName;
        private string password = UpYunConfig.Password;
        private string DL = "/";
        private Hashtable tmp_infos = new Hashtable();
        private string file_secret;
        private string content_md5;
        private bool auto_mkdir = false;

        public UpYunImageService()
        {

        }

        public string UploadImage(string bucketName, byte[] imageBytes)
        {
            string dateFileName = FileHelper.GenerateDateFilename();
            string fullFileName = bucketName + "/" + dateFileName + ImageCommonConfig.Postfix;
            string localFilePath = ImageService.UploadImageToLocal(bucketName,imageBytes);
            /// 设置待上传文件的 Content-MD5 值（如又拍云服务端收到的文件MD5值与用户设置的不一致，将回报 406 Not Acceptable 错误）
            this.setContentMD5(md5_file(localFilePath));
            bool result = writeFile("/" + fullFileName, imageBytes, true);
            return result ? fullFileName : string.Empty;
        }

        //public string UploadHeadImage(ImageType imageType, Stream imageStream)
        //{
        //    string dateFileName = UploadHelper.GenerateDateFilename();
        //    string fullFileName = imageType + "/" + dateFileName + ImageCommonConfig.PngPostfix;
        //    string fullOFileName = imageType + "/" + dateFileName + ImageCommonConfig.OPostfix;

        //    Bitmap image = new Bitmap(System.Drawing.Image.FromStream(imageStream, true));
        //    byte[] bytes = ConvertToCircle(image,200,200);//CircleConvert(image);

        //    if (image != null)
        //    {
        //        image.Dispose();
        //        image = null;
        //    }

        //    string localFilePath = base.UploadImageToLocal(imageType, fullFileName, bytes);

        //    /// 设置待上传文件的 Content-MD5 值（如又拍云服务端收到的文件MD5值与用户设置的不一致，将回报 406 Not Acceptable 错误）
        //    this.setContentMD5(md5_file(localFilePath));

        //    if (this.writeFile("/" + fullFileName, bytes, true))
        //    {
        //        return fullFileName;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        //public string UploadImage(ImageType imageType, Stream imageStream, bool addWaterMark)
        //{
        //    string dateFileName = UploadHelper.GenerateDateFilename();
        //    string fullFileName = imageType + "/" + dateFileName + ImageCommonConfig.Postfix;
        //    string fullOFileName = imageType + "/" + dateFileName + ImageCommonConfig.OPostfix;

        //    System.Drawing.Image image = System.Drawing.Image.FromStream(imageStream, true);

        //    string oLocalFilePath = base.UploadImageToLocal(imageType, fullOFileName, imageToByteArray(image));

        //    if (addWaterMark)
        //    {
        //        image = AddImageWaterMark(imageStream);
        //    }

        //    byte[] bytes = imageToByteArray(image);

        //    if (image != null)
        //    {
        //        image.Dispose();
        //        image = null;
        //    }

        //    string localFilePath = base.UploadImageToLocal(imageType, fullFileName, bytes);

        //     设置待上传文件的 Content-MD5 值（如又拍云服务端收到的文件MD5值与用户设置的不一致，将回报 406 Not Acceptable 错误）
        //    this.setContentMD5(md5_file(localFilePath));

        //    if (this.writeFile("/" + fullFileName, bytes, true))
        //    {
        //        return fullFileName;
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return string.Empty;
            int index = imagePath.IndexOf('/');
            if (index <= 0) return imagePath;
            var imageType = imagePath.Substring(0, index);
            var imageSubPath = imagePath.Substring(index);
            return string.Format(UpYunConfig.ViewDomainTemplate, imageType) + imageSubPath;
        }

        #region PrivateMethod
        private void upyunAuth(Hashtable headers, string method, string uri, HttpWebRequest request)
        {
            DateTime dt = DateTime.UtcNow;
            string date = dt.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", CultureInfo.CreateSpecificCulture("en-US"));
            request.Date = dt;
            //headers.Add("Date", date);
            string auth;
            if (request.ContentLength != -1)
                auth = md5(method + '&' + uri + '&' + date + '&' + request.ContentLength + '&' + md5(this.password));
            else
                auth = md5(method + '&' + uri + '&' + date + '&' + 0 + '&' + md5(this.password));
            headers.Add("Authorization", "UpYun " + this.username + ':' + auth);
        }

        private string md5(string str)
        {
            MD5 m = new MD5CryptoServiceProvider();
            byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            string resule = BitConverter.ToString(s);
            resule = resule.Replace("-", "");
            return resule.ToLower();
        }

        private bool delete(string bucketname, string path, Hashtable headers)
        {
            HttpWebResponse resp;
            byte[] a = null;
            resp = newWorker("DELETE", DL + bucketname + path, a, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        private System.Drawing.Image AddImageWaterMark(Stream imageStream)
        {
            System.Drawing.Image initImage = System.Drawing.Image.FromStream(imageStream, true);

            //获取水印图片
            using (System.Drawing.Image wrImage = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath(@"\Image\watermark.png")))
            {
                //水印绘制条件：原始图片宽高均大于或等于水印图片
                if (initImage.Width >= wrImage.Width && initImage.Height >= wrImage.Height)
                {
                    Graphics gWater = Graphics.FromImage(initImage);

                    //透明属性
                    ImageAttributes imgAttributes = new ImageAttributes();
                    ColorMap colorMap = new ColorMap();
                    colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                    colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                    ColorMap[] remapTable = { colorMap };
                    imgAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                    float[][] colorMatrixElements = { 
                                   new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                   new float[] {0.0f,  0.0f,  0.0f,  1.0f, 0.0f},//透明度:1.0
                                   new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                };

                    ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);
                    imgAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    gWater.DrawImage(wrImage, new Rectangle(initImage.Width - wrImage.Width - 30, initImage.Height - wrImage.Height - 30, wrImage.Width, wrImage.Height), 0, 0, wrImage.Width, wrImage.Height, GraphicsUnit.Pixel, imgAttributes);

                    gWater.Dispose();
                }
                wrImage.Dispose();
            }

            return initImage;
        }

        public byte[] ConvertToCircle(Bitmap bmp, int width, int height)
        {
            if (bmp == null) return null;
            using (bmp)
            using (Bitmap newImg = new Bitmap(width, height))
            using (Graphics g = Graphics.FromImage(newImg))
            {
                g.DrawImage(bmp, 0, 0, width, height);
                using (TextureBrush texture = new TextureBrush(newImg))
                {
                    texture.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.Clear(Color.Transparent);
                    g.FillEllipse(texture, new RectangleF(2, 2, width - 4, height - 4));
                    using (MemoryStream ms = new MemoryStream())
                    {
                        newImg.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }

        private byte[] CircleConvert(Bitmap bmp)
        {
            const int imgW = 120;
            const int imgH = 120;
            //Bitmap bmp = bmp1;
            Bitmap smallImg = new Bitmap(imgW, imgH);
            if (bmp == null)
            {
                return null;
            }

            int w = bmp.Width;
            int h = bmp.Height;
            int r = w > h ? (h >> 1) : (w >> 1);
            Bitmap canvas = new Bitmap(r * 2, r * 2);
            Graphics g = Graphics.FromImage(canvas);
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            TextureBrush texture = new TextureBrush(bmp);
            texture.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
            g.FillEllipse(texture, new Rectangle(0, 0, r * 2, r * 2));

            if (g != null)
            {
                g.Dispose();
                g = null;
            }

            g = Graphics.FromImage(smallImg);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(canvas, new Rectangle(0, 0, imgW, imgH), new Rectangle(0, 0, canvas.Width, canvas.Height), GraphicsUnit.Pixel);

            if (g != null)
            {
                g.Dispose();
                g = null;
            }

            //smallImg.Save(saveTo, System.Drawing.Imaging.ImageFormat.Png);//保存
            if (bmp != null)
            {
                bmp.Dispose();
                bmp = null;
            }

            if (canvas != null)
            {
                canvas.Dispose();
                canvas = null;
            }

            byte[] bytes = pngImageToByteArray(smallImg);

            if (smallImg != null)
            {
                smallImg.Dispose();
                smallImg = null;
            }
            return bytes;
        }

        private HttpWebResponse newWorker(string method, string Url, byte[] postData, Hashtable headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + UpYunConfig.Api_Domain + Url);


            request.Method = method;

            if (this.auto_mkdir == true)
            {
                headers.Add("mkdir", "true");
                this.auto_mkdir = false;
            }

            if (postData != null)
            {
                request.ContentLength = postData.Length;
                request.KeepAlive = true;
                if (this.content_md5 != null)
                {
                    request.Headers.Add("Content-MD5", this.content_md5);
                    this.content_md5 = null;
                }
                if (this.file_secret != null)
                {
                    request.Headers.Add("Content-Secret", this.file_secret);
                    this.file_secret = null;
                }
            }

            if (UpYunConfig.UpAuth)
            {
                upyunAuth(headers, method, Url, request);
            }
            else
            {
                request.Headers.Add("Authorization", "Basic " +
                    Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(this.username + ":" + this.password)));
            }
            foreach (DictionaryEntry var in headers)
            {
                request.Headers.Add(var.Key.ToString(), var.Value.ToString());
            }

            if (postData != null)
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(postData, 0, postData.Length);
                dataStream.Close();
            }
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                this.tmp_infos = new Hashtable();
                foreach (var hl in response.Headers)
                {
                    string name = (string)hl;
                    if (name.Length > 7 && name.Substring(0, 7) == "x-upyun")
                    {
                        this.tmp_infos.Add(name, response.Headers[name]);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return response;
        }

        public byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        public byte[] pngImageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }

        /**
        * 获取总体空间的占用信息
        * return 空间占用量，失败返回 null
        */
        private int getFolderUsage(string bucketname, string url)
        {
            Hashtable headers = new Hashtable();
            int size;
            byte[] a = null;
            HttpWebResponse resp = newWorker("GET", DL + bucketname + url + "?usage", a, headers);
            try
            {
                StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                string strhtml = sr.ReadToEnd();
                resp.Close();
                size = int.Parse(strhtml);
            }
            catch (Exception)
            {
                size = 0;
            }
            return size;
        }

        /**
           * 获取某个子目录的占用信息
           * @param $path 目标路径
           * return 空间占用量，失败返回 null
           */
        private int getBucketUsage(string bucketname)
        {
            return getFolderUsage(bucketname, "");
        }

        /**
        * 创建目录
        * @param $path 目录路径
        * return true or false
        */
        private bool mkDir(string bucketname, string path, bool auto_mkdir)
        {
            this.auto_mkdir = auto_mkdir;
            Hashtable headers = new Hashtable();
            headers.Add("folder", "create");
            HttpWebResponse resp;
            byte[] a = new byte[0];
            resp = newWorker("POST", DL + bucketname + path, a, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        /**
        * 删除目录
        * @param $path 目录路径
        * return true or false
        */
        public bool rmDir(string bucketname, string path)
        {
            Hashtable headers = new Hashtable();
            return delete(bucketname, path, headers);
        }

        /**
        * 读取目录列表
        * @param $path 目录路径
        * return array 数组 或 null
        */
        private ArrayList readDir(string bucketname, string url)
        {
            Hashtable headers = new Hashtable();
            byte[] a = null;
            HttpWebResponse resp = newWorker("GET", DL + bucketname + url, a, headers);
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string strhtml = sr.ReadToEnd();
            resp.Close();
            strhtml = strhtml.Replace("\t", "\\");
            strhtml = strhtml.Replace("\n", "\\");
            string[] ss = strhtml.Split('\\');
            int i = 0;
            ArrayList AL = new ArrayList();
            while (i < ss.Length)
            {
                FolderItem fi = new FolderItem(ss[i], ss[i + 1], int.Parse(ss[i + 2]), int.Parse(ss[i + 3]));
                AL.Add(fi);
                i += 4;
            }
            return AL;
        }

        /**
        * 上传文件
        * @param $file 文件路径（包含文件名）
        * @param $datas 文件内容 或 文件IO数据流
        * return true or false
        */
        private bool writeFile(string bucketname, string path, byte[] data, bool auto_mkdir)
        {
            Hashtable headers = new Hashtable();
            this.auto_mkdir = auto_mkdir;
            HttpWebResponse resp;
            resp = newWorker("POST", DL + bucketname + path, data, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        private bool writeFile(string path, byte[] data, bool auto_mkdir)
        {
            Hashtable headers = new Hashtable();
            this.auto_mkdir = auto_mkdir;
            HttpWebResponse resp;
            resp = newWorker("POST", path, data, headers);
            if ((int)resp.StatusCode == 200)
            {
                resp.Close();
                return true;
            }
            else
            {
                resp.Close();
                return false;
            }
        }

        /**
        * 删除文件
        * @param $file 文件路径（包含文件名）
        * return true or false
        */
        private bool deleteFile(string bucketname, string path)
        {
            Hashtable headers = new Hashtable();
            return delete(bucketname, path, headers);
        }

        /**
        * 读取文件
        * @param $file 文件路径（包含文件名）
        * @param $output_file 可传递文件IO数据流（默认为 null，结果返回文件内容，如设置文件数据流，将返回 true or false）
        * return 文件内容 或 null
        */
        private byte[] readFile(string bucketname, string path)
        {
            Hashtable headers = new Hashtable();
            byte[] a = null;

            HttpWebResponse resp = newWorker("GET", DL + bucketname + path, a, headers);
            StreamReader sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            BinaryReader br = new BinaryReader(sr.BaseStream);
            byte[] by = br.ReadBytes(1024 * 1024 * 100); /// 又拍云存储最大文件限制 100Mb，对于普通用户可以改写该值，以减少内存消耗
            resp.Close();
            return by;
        }

        /**
        * 设置待上传文件的 Content-MD5 值（如又拍云服务端收到的文件MD5值与用户设置的不一致，将回报 406 Not Acceptable 错误）
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        private void setContentMD5(string str)
        {
            this.content_md5 = str;
        }

        /**
        * 设置待上传文件的 访问密钥（注意：仅支持图片空！，设置密钥后，无法根据原文件URL直接访问，需带 URL 后面加上 （缩略图间隔标志符+密钥） 进行访问）
        * 如缩略图间隔标志符为 ! ，密钥为 bac，上传文件路径为 /folder/test.jpg ，那么该图片的对外访问地址为： http://空间域名/folder/test.jpg!bac
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        private void setFileSecret(string str)
        {
            this.file_secret = str;
        }

        /**
        * 获取文件信息
        * @param $file 文件路径（包含文件名）
        * return array('type'=> file | folder, 'size'=> file size, 'date'=> unix time) 或 null
        */
        private Hashtable getFileInfo(string bucketname, string file)
        {
            Hashtable headers = new Hashtable();
            byte[] a = null;
            HttpWebResponse resp = newWorker("HEAD", DL + bucketname + file, a, headers);
            resp.Close();
            Hashtable ht;
            try
            {
                ht = new Hashtable();
                ht.Add("type", this.tmp_infos["x-upyun-file-type"]);
                ht.Add("size", this.tmp_infos["x-upyun-file-size"]);
                ht.Add("date", this.tmp_infos["x-upyun-file-date"]);
            }
            catch (Exception)
            {
                ht = new Hashtable();
            }
            return ht;
        }

        //获取上传后的图片信息（仅图片空间有返回数据）
        private object getWritedFileInfo(string key)
        {
            if (this.tmp_infos == new Hashtable()) return "";
            return this.tmp_infos[key];
        }

        //计算文件的MD5码
        private static string md5_file(string pathName)
        {
            string strResult = "";
            string strHashData = "";

            byte[] arrbytHashValue;
            System.IO.FileStream oFileStream = null;

            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher =
                       new System.Security.Cryptography.MD5CryptoServiceProvider();

            try
            {
                oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open,
                      System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = System.BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return strResult.ToLower();
        }
        #endregion
    }
}

//目录条目类
public class FolderItem
{
    public string filename;
    public string filetype;
    public int size;
    public int number;
    public FolderItem(string filename, string filetype, int size, int number)
    {
        this.filename = filename;
        this.filetype = filetype;
        this.size = size;
        this.number = number;
    }
}

