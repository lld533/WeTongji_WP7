﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ImageTools.IO.Png;
using ImageTools;
using ImageTools.IO.Bmp;
using ImageTools.IO.Gif;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Media.Imaging;


namespace WeTongji.Utility
{
    public static class ImagePathToImageSource
    {
        public static String GetImageFileExtension(this String url)
        {
            return url.Split('.').Last().Split('?').First().ToLower();
        }

        public static BitmapSource GetImageSource(this String fileName)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            var fileExt = fileName.Split('.').Last();

            if (!store.FileExists(fileName))
            {
                return null;
            }
            else
            {
                if (fileExt == "png")
                {
                    PngDecoder decoder = new PngDecoder();
                    var imgExt = new ExtendedImage();
                    using (var stream = store.OpenFile(fileName, FileMode.Open))
                    {
                        decoder.Decode(imgExt, stream);
                        var wb = imgExt.ToBitmap();
                        return wb;
                    }
                }
                else if (fileExt == "gif")
                {
                    GifDecoder decoder = new GifDecoder();
                    var imgExt = new ExtendedImage();
                    using (var stream = store.OpenFile(fileName, FileMode.Open))
                    {
                        decoder.Decode(imgExt, stream);
                        var wb = imgExt.ToBitmap();
                        return wb;
                    }
                }
                else if (fileExt == "bmp")
                {
                    BmpDecoder decoder = new BmpDecoder();
                    var imgExt = new ExtendedImage();
                    using (var stream = store.OpenFile(fileName, FileMode.Open))
                    {
                        decoder.Decode(imgExt, stream);
                        var wb = imgExt.ToBitmap();
                        return wb;
                    }
                }
                else
                {
                    using (var stream = store.OpenFile(fileName, FileMode.Open))
                    {
                        var bi = new BitmapImage();
                        bi.SetSource(stream);
                        return bi;
                    }
                }
            }
        }

        public static IEnumerable<String> GetImageFilesNames(this String imageExtList)
        {
            if (!String.IsNullOrEmpty(imageExtList))
            {
                var images = imageExtList.Split(';');
                int count = images.Count();
                String[] result = new String[count];

                for (int i = 0; i < count; ++i)
                {
                    var parts = images[i].Split(':');
                    result[i] = String.Format("{0}.{1}", parts[0].Trim('\"'), parts[1].Trim('\"'));
                }

                return result;
            }
            else
                return new String[0];
        }
    }
}
