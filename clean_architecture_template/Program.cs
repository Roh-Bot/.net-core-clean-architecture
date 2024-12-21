using Autofac;
using Autofac.Extensions.DependencyInjection;
using clean_architecture_template.Middlewares;
using clean_architecture_template.Models;
using Core.ServiceContracts;
using Core.Services;
using Data;
using Data.Repositories;
using Data.RepositoryContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Text.Json;

namespace clean_architecture_template
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuring DI Container
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add services to the container.
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterType<DatabaseConnectionFactory>().SingleInstance();
                containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
                containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            });

            Console.WriteLine(builder.Environment.ApplicationName);
            // Configure Serilog logging
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
                    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        path:
                        $"{builder.Configuration["LogFilePath"]}{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}/log-.txt",
                        rollingInterval: RollingInterval.Hour,
                        retainedFileCountLimit: 7
                    );
            });

            // Add services for controllers
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.AllowTrailingCommas = false;
                });

            // To handle invalid model state error from Required attribute
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    return new BadRequestObjectResult(new Response()
                    {
                        Status = -1,
                        Error = context.ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .FirstOrDefault()
                    });
                };
            });

            // Add HttpClient
            builder.Services.AddHttpClient();

            // Add Basic Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Basic";
            })
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", options => { });

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields =
                    HttpLoggingFields.RequestMethod |
                    HttpLoggingFields.RequestPath |
                    HttpLoggingFields.RequestHeaders |
                    HttpLoggingFields.RequestQuery |
                    HttpLoggingFields.RequestBody |
                    HttpLoggingFields.ResponseStatusCode;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseExceptionHandlingMiddleware();

            app.UseSerilogRequestLogging();

            app.UseHttpLogging();

            app.UseRouting();

            // Use Authentication and Authorization
            app.UseAuthentication();
            app.UseAuthorization();


            // Route configuration
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action}/{id?}");

            app.Run();
        }
    }
}
