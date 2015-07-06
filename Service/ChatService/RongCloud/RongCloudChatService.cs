using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeerBubbleUtility
{
    public class RongCloudChatService
    {
        public string GetToken(string userID,string userName,string headImgPath)
        {
            return RongCloudServer.GetToken(RongCloudConfig.AppKey, RongCloudConfig.AppSecret, userID, userName, headImgPath);
        }
    }
}
