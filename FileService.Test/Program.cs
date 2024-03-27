using FileService.AWS;
using FileService.Azure;
using Microsoft.Extensions.Configuration;

namespace FileService.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string fileServiceType = builder.Configuration["FileServiceType"];

            builder.Services.AddControllersWithViews();

            if (fileServiceType == "Azure")
            {
                builder.Services.AddAzureFileService(options =>
                {
                    options.ConnectionString = builder.Configuration.GetSection("Azure:ConnectionString").Value;
                });
            }
            else if (fileServiceType == "AWS")
            {
                builder.Services.AddAWSFileService(options =>
                {
                    options.AccessKey = builder.Configuration.GetSection("AWS:AccessKeyId").Value;
                    options.SecretKey = builder.Configuration.GetSection("AWS:SecretAccessKey").Value;
                    options.Region = builder.Configuration.GetSection("AWS:Region").Value;
                });
            }
            else
            {
                throw new InvalidOperationException("Invalid file service type specified in configuration.");
            }

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}