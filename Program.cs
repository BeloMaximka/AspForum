using AspForum.Data.Entities;
using AspForum.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspForum.Services.Email;

namespace AspForum
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
			builder.Services.AddSingleton<IEmailService, GmailService>();

			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			builder.Services.AddDbContext<ApplicationContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddAuthentication()
				.AddGoogle(options =>
				{
					IConfigurationSection googleAuthNSection =
					builder.Configuration.GetSection("Authentication:Google");
					options.ClientId = googleAuthNSection["ClientId"];
					options.ClientSecret = googleAuthNSection["ClientSecret"];
				});

			builder.Services.AddIdentity<User, Role>()
				.AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultUI()
                .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}