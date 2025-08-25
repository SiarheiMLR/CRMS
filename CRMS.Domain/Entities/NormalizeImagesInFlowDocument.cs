using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace CRMS.Domain.Entities
{
    public partial class Ticket
    {
        public static void NormalizeImagesInFlowDocument(FlowDocument document)
        {
            if (document == null) return;

            var images = FindImagesInFlowDocument(document);

            foreach (var image in images)
            {
                try
                {
                    if (image.Source is BitmapImage bmp)
                    {
                        // Пропускаем уже нормализованные изображения
                        if (bmp.UriSource != null && bmp.UriSource.IsAbsoluteUri && bmp.UriSource.Scheme == "data")
                            continue;

                        byte[] bytes = null;

                        // 1. Если картинка ссылается на файл
                        if (bmp.UriSource != null && bmp.UriSource.IsFile)
                        {
                            var path = bmp.UriSource.LocalPath;
                            if (File.Exists(path))
                                bytes = File.ReadAllBytes(path);
                        }

                        // 2. Если картинка вставлена из буфера
                        if (bytes == null && bmp.StreamSource != null)
                        {
                            using var ms = new MemoryStream();
                            bmp.StreamSource.Position = 0;
                            bmp.StreamSource.CopyTo(ms);
                            bytes = ms.ToArray();
                        }

                        // 3. Если байты удалось получить → конвертируем в base64
                        if (bytes != null)
                        {
                            string base64 = Convert.ToBase64String(bytes);
                            string mimeType = GetMimeTypeFromBytes(bytes);
                            string dataUri = $"data:{mimeType};base64,{base64}";

                            var newBmp = new BitmapImage();
                            newBmp.BeginInit();
                            newBmp.CacheOption = BitmapCacheOption.OnLoad;
                            newBmp.UriSource = new Uri(dataUri);
                            newBmp.EndInit();

                            image.Source = newBmp;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка нормализации изображения: {ex.Message}");
                    // Заменяем на плейсхолдер в случае ошибки
                    ReplaceWithPlaceholder(image);
                }
            }
        }

        private static string GetMimeTypeFromBytes(byte[] bytes)
        {
            // Простая проверка сигнатур файлов
            if (bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";
            if (bytes.Length > 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";
            if (bytes.Length > 3 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
                return "image/gif";

            return "image/png"; // По умолчанию
        }

    }
}
