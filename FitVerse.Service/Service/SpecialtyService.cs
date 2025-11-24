using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Specialist;
using FitVerse.Core.ViewModels.Specialty;
using FitVerse.Data.Models;
using FitVerse.Data.Service;
using FitVerse.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Service.Service
{
    public class SpecialtyService : ISpecialtyService
    {
       private readonly IUnitOfWork _unitOfWork;
       private readonly IImageHandleService _imageHandleService = new ImageHandleService();
        public  SpecialtyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public (bool Success, string Message) AddSpecialty(AddSpecialtyVM model)
        {
            try
            {
                // استخدمي الـ ImageHandleService لحفظ الصورة
                string? imagePath = _imageHandleService.SaveImage(model.Image);

                _unitOfWork.Specialties.Add(new Specialty
                {
                    Name = model.Name,
                    Description = model.Description,
                    Image = imagePath
                });

                var result = _unitOfWork.Complete() > 0;
                return result ? (true, "Specialty Added Successfully") : (false, "Something Went Wrong");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        public(bool Success, string Message) DeleteSpecialty(int id)
        {
            var specialty = _unitOfWork.Specialties.GetById(id);
            if (specialty == null) return (false, "Specialty Not Found");
            
            // Delete image from wwwroot before deleting the entity
            if (!string.IsNullOrEmpty(specialty.Image))
            {
                _imageHandleService.DeleteImage(specialty.Image);
            }
            
            _unitOfWork.Specialties.Delete(specialty);
            return _unitOfWork.Complete() > 0 ? (true, "Specialty deleted successfully") : (false, "Something went wrong!");
        }
        
        public List<SpecialtyVM> GetAllSpecialties()
        {
            var allSpecialties = _unitOfWork.Specialties.GetAll().ToList();
            var allCoachSpecialties = _unitOfWork.CoachSpecialties.GetAll().ToList();

            return allSpecialties.Select(s => new SpecialtyVM
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
               Image = s.Image,
                CoachesCount = allCoachSpecialties.Count(cs => cs.SpecialtyId == s.Id)
            }).ToList();

           
        }

       public SpecialtyVM? GetSpecialtyById(int id)
        {
            var specialty = _unitOfWork.Specialties.GetById(id);
            var totalSpecialties = _unitOfWork.Specialties.GetAll().Count(); 

            if (specialty == null) return null;
            return new SpecialtyVM
            {
                Id = specialty.Id,
                Name = specialty.Name,
                Description = specialty.Description,
               Image =specialty.Image,
                CoachesCount = specialty.CoachSpecialties.Count(),
                TotalSpecialties = totalSpecialties

            };
        }

        public (int TotalSpecialties, int TotalCoaches) GetStats()
        {
            var totalSpecialties = _unitOfWork.Specialties.GetAll().Count();
            var totalCoaches = _unitOfWork.CoachSpecialties.GetAll().Count();

            return (totalSpecialties, totalCoaches);
        }

        public (bool Success, string Message) UpdateSpecialty(UpdateSpecialtyVM model)
        {
            var specialty = _unitOfWork.Specialties.GetById(model.Id);
            if (specialty == null)
                return (false, "Specialty not found");

            specialty.Name = model.Name;
            specialty.Description = model.Description;

            if (model.Image != null)
            {
                // Delete old image before saving new one
                if (!string.IsNullOrEmpty(specialty.Image))
                {
                    _imageHandleService.DeleteImage(specialty.Image);
                }
                
                var imagePath = _imageHandleService.SaveImage(model.Image);
                specialty.Image = imagePath;
            }

            _unitOfWork.Specialties.Update(specialty);
            return _unitOfWork.Complete() > 0
                ? (true, "Specialty updated successfully")
                : (false, "Something went wrong!");
        }

      
    }
}
