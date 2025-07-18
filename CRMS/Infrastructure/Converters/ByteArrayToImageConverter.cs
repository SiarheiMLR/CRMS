﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CRMS.Infrastructure.Converters
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public ImageSource? DefaultAvatar { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                using var stream = new MemoryStream(bytes);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze(); // для потокобезопасности
                return image;
            }
            return DefaultAvatar ?? new BitmapImage(new Uri("pack://application:,,,/Resources/Images/no-avatar.jpg"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack не реализован");
        }
    }
}

