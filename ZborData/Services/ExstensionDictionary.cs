using System;
using System.Collections.Generic;
using System.Text;

namespace ZborData.Services
{
    public static class ExstensionDictionary
    {
        private static Dictionary<string, string> _dict = new Dictionary<string, string>
        {
            {"pdf", "https://image.flaticon.com/icons/svg/136/136522.svg"},
            {"doc", "https://image.flaticon.com/icons/svg/136/136521.svg"},
            {"docx", "https://image.flaticon.com/icons/svg/136/136521.svg"},
            {"jpg", "https://image.flaticon.com/icons/svg/136/136524.svg"},
            {"jpeg", "https://image.flaticon.com/icons/svg/136/136524.svg"},
            {"xls", "https://image.flaticon.com/icons/svg/136/136532.svg"},
            {"xlsx", "https://image.flaticon.com/icons/svg/136/136532.svg"},
            {"png", "https://image.flaticon.com/icons/svg/136/136523.svg"},
            {"zip", "https://image.flaticon.com/icons/svg/136/136544.svg"},
            {"mp3", "https://image.flaticon.com/icons/svg/136/136548.svg"}
        };
        public static string GetLink(string ekstenzija)
        {
            // Try to get the result in the static Dictionary
            string result;
            if (_dict.TryGetValue(ekstenzija, out result))
            {
                return result;
            }
            else
            {
                return "https://image.flaticon.com/icons/svg/136/136549.svg";
            }
        }
    }
   
}
