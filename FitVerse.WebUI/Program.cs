using FitVerse.Core.Interfaces;
using FitVerse.Data.Repositories;
using FitVerse.Core.IService;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.MapperConfigs;
using FitVerse.Core.Models;
using FitVerse.Core.UnitOfWork;
using FitVerse.Data.Configurations;
using FitVerse.Data.Context;
using FitVerse.Data.Repositories;
using FitVerse.Data.Service;
using FitVerse.Data.Service.FitVerse.Data.Service;
using FitVerse.Data.Service.FitVerse.Data.Service;
using FitVerse.Data.UnitOfWork;
using FitVerse.Service.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using FitVerse.Data.Service.FitVerse.Data.Service;
using FitVerse.Core.IUnitOfWorkServices;
using FitVerse.Core.Models;
using FitVerse.Web.Hubs;
using FitVerse.Data.Seed;

namespace FitVerse.WebUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<FitVerseDbContext>(options =>
                options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddAutoMapper(op=>op.AddProfile(typeof(MapperConfig)));


            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.User.AllowedUserNameCharacters = null;
                options.Password.RequiredUniqueChars = 0;
                }).AddEntityFrameworkStores<FitVerseDbContext>()
                .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);
                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                  options =>
                  {
                  options.LoginPath = new PathString("/Account/Login");
                  options.AccessDeniedPath = new PathString("/Account/AccessDenied");
              });
            //Allow UserName Duplication


            builder.Services.AddControllers()//make json case sensitive
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

                });


            // Add session services
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                // Add global action filter to populate user info in all views
                options.Filters.Add<FitVerse.WebUI.Filters.UserInfoActionFilter>();
            });


            // Add SignalR with custom UserIdProvider for targeted notifications
            builder.Services.AddSignalR()
             .AddJsonProtocol(options =>
             {
                 options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                 options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
             });

            // Register custom UserIdProvider for SignalR (CRITICAL for notifications)
            builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, FitVerse.Web.Helpers.CustomUserIdProvider>();

            
            builder.Services.AddScoped<IExercisePlanDetailRepository, ExercisePlanDetailRepository>();
            builder.Services.AddScoped<IExercisePlanDetailService, ExercisePlanDetailService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            builder.Services.AddScoped<IAnatomyRepository, AnatomyRepository>();
            builder.Services.AddScoped<IMuscleRepository, MuscleRepository>();
            builder.Services.AddScoped<ICoachRepository, CoachRepository>();
            builder.Services.AddScoped<IClientRepository, ClientRepository>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUnitOFWorkService, UnitOfWorkService>();
            builder.Services.AddScoped<IMuscleService, MuscleService>();
            builder.Services.AddScoped<ISpecialtiesRepository, SpecialityRepository>();
            builder.Services.AddScoped<ISpecialtyService, SpecialtyService>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IMessageService, MessageService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<FitVerse.Web.Helpers.NotificationHelper>();
            builder.Services.AddScoped<ICoachService, CoachService>();
            builder.Services.AddScoped<IClientService, ClientService>();
            builder.Services.AddScoped<IImageHandleService, ImageHandleService>();
            builder.Services.AddScoped<IClientOnCoachesService, ClientOnCoachesService>();
            builder.Services.AddScoped<IExerciseService, ExerciseService>();
            builder.Services.AddScoped<IExercisePlanRepository, ExercisePlanRepository>();
            builder.Services.AddScoped<IExercisePlanService, FitVerse.Service.Service.ExercisePlanService>();



            builder.Services.AddAutoMapper(op=>op.AddProfile(typeof(MapperConfig)));


            var app = builder.Build();

        
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Map SignalR Hub
            app.MapHub<ChatHub>("/chatHub");

            // Seed roles and default admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    // Include sample data only in development environment
                    bool includeSampleData = app.Environment.IsDevelopment();
                    await DatabaseSeeder.SeedAsync(services, includeSampleData);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            app.Run();
        }
    }
}
