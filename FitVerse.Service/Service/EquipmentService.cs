using AutoMapper;
using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.viewModels;
using FitVerse.Core.ViewModels.Coach;
using FitVerse.Core.ViewModels.Equipment;
using FitVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Service.Service
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IImageHandleService imageHandleService;

        public EquipmentService(IUnitOfWork unitOfWork, IMapper mapper, IImageHandleService imageHandleService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.imageHandleService = imageHandleService;

        }


        public (bool Success, string Message) AddEquipment(AddEquipmentVM model)
        {

            try
            {
                string? imagePath = imageHandleService.SaveImage(model.EquipmentImageFile);

                var equipment = mapper.Map<Equipment>(model);

                equipment.Image = imagePath ?? "/Images/default.jpg";

                unitOfWork.Equipments.Add(equipment);
                unitOfWork.Complete();

                return (true, "Equipment added successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public List<AddEquipmentVM> GetAll(string? search)
        {
            var equipments = unitOfWork.Equipments.GetAll();
            if (!string.IsNullOrEmpty(search))
            {
                equipments = equipments.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            
            return mapper.Map<List<AddEquipmentVM>>(equipments);
        }

        public AddEquipmentVM GetById(int id)
        {
            var equipment = unitOfWork.Equipments.GetById(id);
            return mapper.Map<AddEquipmentVM>(equipment);
        }


        public (bool Success, string Message) Update(AddEquipmentVM model)
        {
            try
            {
                var equipment = unitOfWork.Equipments.GetById(model.Id);
                if (equipment == null)
                    return (false, "Not Found!");

                equipment.Name = model.Name;

                // ✅ تحديث الصورة لو تم رفع واحدة جديدة
                if (model.EquipmentImageFile != null)
                {
                    // Delete old image before saving new one
                    if (!string.IsNullOrEmpty(equipment.Image))
                    {
                        imageHandleService.DeleteImage(equipment.Image);
                    }
                    
                    string? imagePath = imageHandleService.SaveImage(model.EquipmentImageFile);
                    equipment.Image = imagePath ?? equipment.Image;
                }

                unitOfWork.Equipments.Update(equipment);
                unitOfWork.Complete();

                return (true, "Equipment updated successfully!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool Success, string Message) Delete(int id)
        {
            var equipment = unitOfWork.Equipments.GetById(id);
            if (equipment == null)
                return (false, "Not Found!");

            // Delete image from wwwroot before deleting the entity
            if (!string.IsNullOrEmpty(equipment.Image))
            {
                imageHandleService.DeleteImage(equipment.Image);
            }

            unitOfWork.Equipments.Delete(equipment);
            if (unitOfWork.Complete() > 0)
                return (true, "Equipment deleted successfully");

            return (false, "Something went wrong!");
        }

        public (List<AddEquipmentVM> Data, int TotalItems) GetPagedEquipments(int page, int pageSize, string? search)
        {
            var query = unitOfWork.Equipments.GetAll().AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                string lowerSearch = search.ToLower();
                query = query.Where(e => e.Name.ToLower().Contains(lowerSearch));
            }

            var totalItems = query.Count();

            var pagedData = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var mappedData = mapper.Map<List<AddEquipmentVM>>(pagedData);

            return (mappedData, totalItems);
        }

        public int GetTotalEquipmentCount()
        {
            return unitOfWork.Equipments.GetAll().Count();

        }
        public int GetTotalExerciseCount()
        {
            return unitOfWork.Exercises.GetAll().Count();
        }

    }
}



