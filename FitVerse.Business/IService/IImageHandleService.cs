using FitVerse.Core.UnitOfWork;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Core.IService
{
    public interface IImageHandleService : IService
    {
        string? SaveImage(IFormFile? file);
        bool DeleteImage(string? imagePath);
    }
}
