using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Reflection;


namespace System
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public EnumDescriptionAttribute(string description)
        {
            this.Description = description;
        }
    }
}

namespace BeerBubbleUtility
{
    public static class EnumHelper
    {
        private static Dictionary<Type, HybridDictionary> _Dict = new Dictionary<Type, HybridDictionary>();

        public static string GetDescription(Enum key)
        {

            FieldInfo fi = key.GetType().GetField(key.ToString());

            if (null != fi)
            {
                object[] attrs = fi.GetCustomAttributes(typeof(EnumDescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                    return ((EnumDescriptionAttribute)attrs[0]).Description;
            }

            return string.Empty;
        }
    }

}
