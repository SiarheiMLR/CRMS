using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows;
using System;
using System.Windows.Media;

namespace CRMS.Domain.Utilities
{
    public static class XamlImageProcessor
    {
        public static string ConvertImagesToBase64(string xamlContent)
        {
            try
            {
                var doc = XamlReader.Parse(xamlContent) as FlowDocument;
                if (doc == null) return xamlContent;

                foreach (var image in FindVisualChildren<Image>(doc))
                {
                    if (image.Source is BitmapSource bitmapSource &&
                        !bitmapSource.ToString().StartsWith("data:"))
                    {
                        // Конвертируем изображение в base64
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                        using (var stream = new MemoryStream())
                        {
                            encoder.Save(stream);
                            var base64 = Convert.ToBase64String(stream.ToArray());
                            image.Source = new BitmapImage(
                                new Uri($"data:image/png;base64,{base64}"));
                        }
                    }
                }

                return XamlWriter.Save(doc);
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем оригинальный контент
                return xamlContent;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}