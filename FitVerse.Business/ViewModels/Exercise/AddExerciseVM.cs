using FitVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Core.ViewModels.ExerciseVM
{
    public class AddExerciseVM
    {
        public string Name { get; set; }
        public string? VideoLink { get; set; }
        public string Description { get; set; }

        public  int MuscleId { get; set; }

        public int EquipmentId { get; set; }
    }
}
