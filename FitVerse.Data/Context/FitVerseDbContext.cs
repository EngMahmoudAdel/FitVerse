using FitVerse.Core.Models;
using FitVerse.Data.Configurations;
using FitVerse.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.IO;
using FitVerse.Data.Configurations;


namespace FitVerse.Data.Context
{
    public class FitVerseDbContext : IdentityDbContext<ApplicationUser>
    {
        public FitVerseDbContext(DbContextOptions<FitVerseDbContext> options)
            : base(options)
        {

        }

        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Anatomy> Anatomies { get; set; }
        public DbSet<CoachSpecialties> CoachSpecialties { get; set; }
        public DbSet<CoachFeedback> CoachFeedbacks { get; set; }
        public DbSet<DietPlan> DietPlans { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ExercisePlan> ExercisePlans { get; set; }
        public DbSet<ExercisePlanDetail> ExercisePlanDetails { get; set; }
        public DbSet<Muscle> Muscles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Specialty> Specialties { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
      
        }
       
    }
    
}
