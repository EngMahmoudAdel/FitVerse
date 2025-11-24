using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Core.ViewModels.Meuscle;
using FitVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FitVerse.Service.Service
{
    public class MuscleService : IMuscleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MuscleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public (bool Success, string Message) AddMuscle(AddMuscleVM model)
        {
           
            _unitOfWork.Muscles.Add(new Muscle
            {
                Name = model.Name,
                Description = model.Description,
                AnatomyId = model.AnatomyId
            });

            return _unitOfWork.Complete() > 0 ? (true, "Muscle added successfully") : (false, "Something went wrong!");
        }

        public List<MuscleVM> GetAllMuscles()
        {
            var muscles = _unitOfWork.Muscles.GetAllWithAnatomy();
            return muscles.Select(m => new MuscleVM
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                AnatomyId = m.Anatomy?.Id ?? 0,
                AnatomyName = m.Anatomy?.Name ?? "Unknown",
                ExerciseCount = m.Exercises?.Count() ?? 0
            }).ToList();
        }

        public MuscleVM? GetMuscleById(int id)
        {
            var muscle = _unitOfWork.Muscles.GetByIdWithAnatomy(id);
            if (muscle == null) return null;

            return new MuscleVM
            {
                Id = muscle.Id,
                Name = muscle.Name,
                Description = muscle.Description,
                AnatomyId = muscle.AnatomyId,
                AnatomyName = muscle.Anatomy?.Name ?? "Unknown",
                ExerciseCount = muscle.Exercises?.Count() ?? 0
            };
        }

        public (bool Success, string Message) UpdateMuscle(MuscleVM model)
        {
            var muscle = _unitOfWork.Muscles.GetById(model.Id);
            if (muscle == null) return (false, "Muscle not found!");

            muscle.Name = model.Name;
            muscle.Description = model.Description;
            muscle.AnatomyId = model.AnatomyId;

            _unitOfWork.Muscles.Update(muscle);
            return _unitOfWork.Complete() > 0 ? (true, "Muscle updated successfully") : (false, "Something went wrong!");
        }

        public (bool Success, string Message) DeleteMuscle(int id)
        {
            var muscle = _unitOfWork.Muscles.GetById(id);
            if (muscle == null) return (false, "Muscle not found!");

            _unitOfWork.Muscles.Delete(muscle);
            return _unitOfWork.Complete() > 0 ? (true, "Muscle deleted successfully") : (false, "Something went wrong!");
        }

        public (List<MuscleVM> Data, int TotalItems) GetPagedMuscles(int page, int pageSize, string? search, int? anatomyId)
        {
            var query = _unitOfWork.Muscles.GetAllWithAnatomy().AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.Name.ToLower().Contains(search.ToLower()));

            if (anatomyId.HasValue && anatomyId.Value > 0)
                query = query.Where(m => m.AnatomyId == anatomyId.Value);

            var totalItems = query.Count();
            var data = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            var mappedData = data.Select(m => new MuscleVM
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                AnatomyId = m.Anatomy != null ? m.Anatomy.Id : 0,
                AnatomyName = m.Anatomy != null ? m.Anatomy.Name : "Unknown",
                ExerciseCount = m.Exercises.Count()
            }).ToList();
            return (mappedData, totalItems);
        }

        public (int TotalMuscles, int TotalAnatomyGroups, int TotalExercises) GetStats()
        {
            var totalMuscles = _unitOfWork.Muscles.GetAll().Count();
            var totalAnatomyGroups = _unitOfWork.Anatomies.GetAll().Count();
            var totalExercises = _unitOfWork.Muscles.GetAll()
                                   .Sum(m => m.Exercises?.Count ?? 0); 

            return (totalMuscles, totalAnatomyGroups, totalExercises);
        }


    }
}