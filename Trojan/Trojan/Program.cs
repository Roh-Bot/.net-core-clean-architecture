using Autofac;
using Autofac.Extensions.DependencyInjection;
using Domain.ServiceContracts;
using Microsoft.AspNetCore.Mvc;
using Services;
using System.Text.Json;
using Trojan.Models;

namespace Trojan
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();


            // Add services to the container.
            // Adding Autofac to ServiceProviderFactory 
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Registering services into the Autofac Dependency Injection Container
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            });

            builder.Configuration.AddJsonFile("parameters.json", optional: true, reloadOnChange: true);

            // Getting configuration from appsettings.json
            builder.Services.Configure<Database>(builder.Configuration.GetSection("database"));

            // Getting configuration from a custom json file

            // Setting a custom invalid model state response 
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var response = new Response()
                    {
                        StatusCode = -1,
                        ErrorMessage = context.ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .FirstOrDefault()
                    };
                    return new BadRequestObjectResult(response);
                };
            });

            // Adding Controller with views with json naming policy set to camel case
            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddHttpClient();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
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
