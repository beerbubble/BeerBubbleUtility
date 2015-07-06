using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleUtility.ImageService
{
    public interface IImageService
    {
        string UploadImage(string bucketName, byte[] imageBytes);

        string GetImageUrl(string imagePath);
    }
}
