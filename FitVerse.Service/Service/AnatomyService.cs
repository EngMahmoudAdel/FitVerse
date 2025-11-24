using AutoMapper;
using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Anatomy;
using FitVerse.Core.ViewModels.Anatomy;
using FitVerse.Core.ViewModels.Equipment;
using FitVerse.Core.ViewModels.Meuscle;
using FitVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitVerse.Service.Service
{
    public class AnatomyService : IAnatomyService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IImageHandleService imageHandleService;

        public AnatomyService(IUnitOfWork unitOfWork, IMapper mapper, IImageHandleService imageHandleService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageHandleService = imageHandleService;

        }


        public (bool Success, string Message) AddAnatomy(AddAnatomyVM model)
        {

            try
            {
                string? imagePath = imageHandleService.SaveImage(model.ImageFile);

                var Anatomy = mapper.Map<Anatomy>(model);

                Anatomy.Image = imagePath ?? "/Images/default.jpg";

                unitOfWork.Anatomies.Add(Anatomy);
                unitOfWork.Complete();

                return (true, "Anatomy added successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public List<AddAnatomyVM> GetAll(string? search)
        {
            var anatomies = unitOfWork.Anatomies.GetAll();
            if (!string.IsNullOrEmpty(search))
            {
                anatomies = anatomies.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }


            return mapper.Map<List<AddAnatomyVM>>(anatomies);
        }

        public AddAnatomyVM? GetById(int id)
        {
            var Anatomy = unitOfWork.Anatomies.GetById(id);
            return mapper.Map<AddAnatomyVM>(Anatomy);
        }


        public (bool Success, string Message) Update(AddAnatomyVM model)
        {
            try
            {
                var anatomy = unitOfWork.Anatomies.GetById(model.Id);
                if (anatomy == null)
                    return (false, "Not Found!");

                anatomy.Name = model.Name;

                // ✅ تحديث الصورة لو تم رفع واحدة جديدة
                if (model.ImageFile != null)
                {
                    // Delete old image before saving new one
                    if (!string.IsNullOrEmpty(anatomy.Image))
                    {
                        imageHandleService.DeleteImage(anatomy.Image);
                    }
                    
                    string? imagePath = imageHandleService.SaveImage(model.ImageFile);
                    anatomy.Image = imagePath ?? anatomy.Image;
                }

                unitOfWork.Anatomies.Update(anatomy);
                unitOfWork.Complete();

                return (true, "Anatomy updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) Delete(int id)
        {
            var Anatomy = unitOfWork.Anatomies.GetById(id);
            if (Anatomy == null)
                return (false, "Not Found!");

            // Delete image from wwwroot before deleting the entity
            if (!string.IsNullOrEmpty(Anatomy.Image))
            {
                imageHandleService.DeleteImage(Anatomy.Image);
            }

            unitOfWork.Anatomies.Delete(Anatomy);
            if (unitOfWork.Complete() > 0)
                return (true, "Anatomy deleted successfully");

            return (false, "Something went wrong!");
        }

     


        // ✅ Count Helpers
        public int GetAllCount() => unitOfWork.Anatomies.GetAll().Count();
        public int GetMuscleCount() => unitOfWork.Muscles.GetAll().Count();
        public int GetExerciseCount() => unitOfWork.Exercises.GetAll().Count();

  

       
    }
}

