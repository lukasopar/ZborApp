using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using ZborMob.Droid;

[assembly: Dependency(typeof(FileService))]
namespace ZborMob.Droid
{
    public class FileService : IFileService
    {
        public void Save(string name,  byte[] data)
        {
            var documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);

            string filePath = Path.Combine(documentsPath.AbsolutePath, name);

            try
            {
                File.WriteAllBytes(filePath, data);
            }catch(Exception e)
            {
                int g = 0;
            }
        }
    }
}