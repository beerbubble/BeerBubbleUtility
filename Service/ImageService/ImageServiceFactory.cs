using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleUtility.ImageService
{
    public class ImageServiceFactory
    {
        public static IImageService Create()
        {
            return new UpYunImageService();
        }
    }
}
