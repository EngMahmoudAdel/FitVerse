using FitVerse.Core.IService;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Data.Service
{
    //SERVICE THAT RETURN IMAGE PATH AFTER SAVING IT IN wwwroot/Images FOLDER 
    public class ImageHandleService : IImageHandleService
    {
        public string? SaveImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return $"/Images/{fileName}";
        }

        public bool DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                // Remove leading slash if present
                string cleanPath = imagePath.TrimStart('/');
                
                // Build full path to the image file
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleanPath);

                // Check if file exists and delete it
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Log the exception if you have a logging service
                return false;
            }
        }
    }
}
