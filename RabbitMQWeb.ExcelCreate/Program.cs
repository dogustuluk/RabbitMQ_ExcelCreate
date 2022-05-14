using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQWeb.ExcelCreate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQWeb.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                //kullan�c� kaydetmek i�in UserManager almam�z laz�m
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                appDbContext.Database.Migrate(); //database-update yazmay� unutursak uygulama aya�a kalkarken ilgili komutlar� veritaban�na i�lemesini sa�layan kod.

                if (!appDbContext.Users.Any())
                {
                    userManager.CreateAsync(new IdentityUser { UserName = "dogus", Email = "dogus@gmail.com" }, "Password12*").Wait();
                    userManager.CreateAsync(new IdentityUser { UserName = "Dogus2", Email = "Dogus2Tuluk@gmail.com" }, "Password12*").Wait();
                }
            }
            
                
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
