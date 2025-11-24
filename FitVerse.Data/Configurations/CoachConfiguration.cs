using FitVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace FitVerse.Data.Configurations
{
    public class CoachConfiguration : IEntityTypeConfiguration<Coach>
    {
        public void Configure(EntityTypeBuilder<Coach> builder)
        {

            builder.HasKey(c => c.Id);

        

            builder.Property(c => c.About)
                   .HasMaxLength(1000);

            builder.HasMany(c => c.CoachSpecialties)
                   .WithOne(cs => cs.Coach)
                   .HasForeignKey(cs => cs.CoachId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Chats)
                   .WithOne(ch => ch.Coach)
                   .HasForeignKey(ch => ch.CoachId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.ClientSubscriptions)
                   .WithOne(cs => cs.Coach)
                   .HasForeignKey(cs => cs.CoachId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.CoachPackages)
                   .WithOne(cp => cp.Coach)
                   .HasForeignKey(cp => cp.CoachId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
    new Coach
    {
        Id = "C1",
        //Name = "John Smith",
        ExperienceYears = 8,
        About = "Expert in Strength and Conditioning",
        ////ImagePath = "/images/coaches/john.jpg",
        ////IsActive = true
    },
    new Coach
    {
        Id = "C2",
        //Name = "Sarah Johnson",
        ExperienceYears = 6,
        About = "Cardio and endurance specialist with personalized HIIT plans.",
        //ImagePath = "/images/coaches/sarah.jpg",
        //IsActive = true
    },
    new Coach
    {
        Id = "C3",
        ////Name = "Michael Lee",
        ExperienceYears = 7,
        About = "Yoga and mobility instructor focused on flexibility and wellness.",
        ////ImagePath = "/images/coaches/michael.jpg",
        ////IsActive = true
    },
    new Coach
    {
        Id = "C4",
        //Name = "Chris Evans",
        ExperienceYears = 5,
        About = "CrossFit certified coach delivering high-intensity programs.",
        //ImagePath = "/images/coaches/chris.jpg",
        //IsActive = true
    },
    new Coach
    {
        Id = "C5",
        //Name = "Amanda Davis",
        ExperienceYears = 4,
        About = "Boxing and MMA trainer with focus on endurance and strength.",
        //ImagePath = "/images/coaches/amanda.jpg",
        //IsActive = true
    },
    new Coach
    {
        Id = "C6",
        //Name = "Robert Wilson",
        ExperienceYears = 10,
        About = "Professional bodybuilder and muscle growth expert.",
        //ImagePath = "/images/coaches/robert.jpg",
        //IsActive = true
    },
    new Coach
    {
        Id = "C7",
        //Name = "Emily Clark",
        ExperienceYears = 5,
        About = "Running and endurance coach with marathon training expertise.",
        //ImagePath = "/images/coaches/emily.jpg",
        //IsActive = true
    },
    new Coach
    {
        Id = "C8",
        //Name = "David Harris",
        ExperienceYears = 6,
        About = "Nutrition and weight loss expert with balanced diet programs.",
        //ImagePath = "/images/coaches/david.jpg",
        //IsActive = true
    }
);
        }
    }
}
