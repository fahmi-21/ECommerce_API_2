
using ECommerce_API_2.DataAccess;
using ECommerce_API_2.Utilities.DBInitilization;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace ECommerce_API_2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<AppDbContext>(
                options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                }

            );

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.User.RequireUniqueEmail = true;
                option.SignIn.RequireConfirmedEmail = true;
                option.Password.RequiredLength = 8;
                option.Lockout.MaxFailedAccessAttempts = 5;
                option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
                .AddEntityFrameworkStores<AppDbContext>()   
                .AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });
            builder.Services.AddScoped<IRepository<Brand> , Repositories<Brand>>();
            builder.Services.AddScoped<IRepository<Category> , Repositories<Category>>();
            builder.Services.AddScoped<IRepository<Models.Product> , Repositories<Models.Product>>();
            builder.Services.AddScoped<IRepository<ProductColor> , Repositories<ProductColor>>();
            builder.Services.AddScoped<IRepository<ProductSubImg> , Repositories<ProductSubImg>>();
            builder.Services.AddScoped<IRepository<Cart> , Repositories<Cart>>();
            builder.Services.AddScoped<IRepository<ApplicationUserOTP> , Repositories<ApplicationUserOTP>>();
            builder.Services.AddScoped<IRepository<ApplicationUser>, Repositories<ApplicationUser>>();
            builder.Services.AddScoped<IRepository<Promotion> , Repositories<Promotion>>();
            


            builder.Services.AddScoped<IAccountServices, AccountServices>();
            builder.Services.AddScoped(typeof(IDBInitilizer), typeof(DBInitilizer));
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetService<IDBInitilizer>();
            service.Initialize();

            app.MapControllers();

            app.Run();
        }
    }
}
