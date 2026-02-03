using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Task4.Data;
using Task4.Services;

namespace Task4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("DATABASE_URL environment variable is not set");

            if (connectionString.StartsWith("mysql://"))
                connectionString = ConvertRailwayUrlToConnectionString(connectionString);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

            builder.Services.AddScoped<IEmailService, ResendEmailService>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
            }

            var port = Environment.GetEnvironmentVariable("PORT") ?? "3000";
            app.Run($"http://0.0.0.0:{port}");
        }

        private static string ConvertRailwayUrlToConnectionString(string railwayUrl)
        {
            try
            {
                var uri = new Uri(railwayUrl);
                var userInfo = uri.UserInfo.Split(':');

                return userInfo.Length != 2
                    ? railwayUrl
                    : $"Server={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
                      $"User={userInfo[0]};Password={userInfo[1]};";
            }
            catch
            {
                return railwayUrl;
            }
        }
    }
}
