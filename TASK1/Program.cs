using static System.Net.WebRequestMethods;
using System.IO;
using System;

namespace TASK1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string fileName = "image";
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            string imageUrl = "https://via.placeholder.com/150/92c952";
            await DownloadImage(imageUrl, filePath);  
        }

        public static async Task DownloadImage(string imageUrl, string filePath)
        {
            try
            {
                using(var httpClient = new HttpClient())
                {
                    HttpResponseMessage resposne = await httpClient.GetAsync(imageUrl);

                    resposne.EnsureSuccessStatusCode();

                    string contentType = resposne.Content.Headers.ContentType.MediaType;

                    filePath += contentType;

                    Console.Write(contentType);

                    byte[] imageData = await resposne.Content.ReadAsByteArrayAsync();

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        fs.Write(imageData, 0, imageData.Length);
                    }

                    Console.WriteLine($"Download complete!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while downloading image file: {ex.Message}");
            }
        }

        private static string GetExtensionFromMime(string mimeType)
        {
            switch (mimeType)
            {
                case "image/jpeg":
                    return ".jpg";
                case "image/png":
                    return ".png";
                case "image/gif":
                    return ".gif";
                default:
                    return ".unknown";
            }
        }
    }
}
