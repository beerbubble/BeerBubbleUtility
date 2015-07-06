using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeerBubbleUtility.ImageService
{
    static class ApiDomain
    {
        public const string Default = "v0.api.upyun.com"; //默认 自动识别
        public const string Telecom = "v1.api.upyun.com"; //电信
        public const string Unicom = "v2.api.upyun.com"; //联通
        public const string Mobile = "v3.api.upyun.com"; //移动
    }

    sealed class UpYunConfig
    {
        public static readonly string UserName = "uteamuser";
        public static readonly string Password = "uteam!@#456";

        public static readonly bool UpAuth = false;
        public static readonly string Api_Domain = ApiDomain.Default;

        public static readonly string ViewDomainTemplate = "http://{0}.b0.upaiyun.com";
    }
}
