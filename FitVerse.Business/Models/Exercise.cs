using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Data.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? VideoLink { get; set; }
        public string? Description { get; set; }
        public int MuscleId { get; set; }
        public virtual Muscle? Muscle { get; set; }
        public int EquipmentId { get; set; }
        public virtual  Equipment? Equipment { get; set; }
        public virtual ICollection<ExercisePlanDetail>? ExercisePlanDetails { get; set; } = new HashSet<ExercisePlanDetail>();

    }

}
