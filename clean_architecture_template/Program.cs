using System.Net;
using System.Text.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using clean_architecture_template.Middlewares;
using clean_architecture_template.Models;
using Core.ServiceContracts;
using Core.Services;
using Domain.RepositoryContracts;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Timeout;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

#region Autofac DI container

// Configuring DI Container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container.
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().SingleInstance();

    containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

    containerBuilder.RegisterType<MiscService>().As<IMiscService>().InstancePerLifetimeScope();

    containerBuilder.RegisterType<JwtService>().As<IJwtService>().InstancePerLifetimeScope();
});

#endregion

#region Serilog logger

// Configure Serilog logging
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("ApplicationName", builder.Environment.ApplicationName)
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            restrictedToMinimumLevel: LogEventLevel.Information
        )
        .WriteTo.File(
            $"{builder.Configuration["LogFilePath"]}/{builder.Environment.ApplicationName}/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}/log-.txt",
            rollingInterval: RollingInterval.Hour,
            retainedFileCountLimit: 7,
            restrictedToMinimumLevel: LogEventLevel.Information
        )
        .WriteTo.Seq(
            serverUrl: "http://localhost:5341/",
            restrictedToMinimumLevel: LogEventLevel.Information
        );
});

#endregion

#region Controller config

// Add services for controllers
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add(new ConsumesAttribute("application/json"));
        options.Filters.Add(new ProducesAttribute("application/json"));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.AllowTrailingCommas = false;
    });

builder.Services.Configure<MvcOptions>(options => options.AllowEmptyInputInBodyModelBinding = true);

// To handle invalid model state error from Required attribute
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(new Response().BadRequest(context.ModelState));
});

#endregion

#region HttpClient

// Add HttpClient
builder.Services.AddHttpClient<IHttpClientService, HttpClientService>()
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .OrResult(response =>
            response.StatusCode
                is HttpStatusCode.InternalServerError
                or HttpStatusCode.GatewayTimeout
                or HttpStatusCode.ServiceUnavailable
        )
        .WaitAndRetryAsync(
            Convert.ToInt32(builder.Configuration["Http:RetryCount"]!),
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (response, timespan, retryCount, _) =>
            {
                Log.Warning(
                    "Retrying {RetryCount}/{MaxRetries} after {Delay} due to {Reason}",
                    retryCount,
                    builder.Configuration["Http:RetryCount"],
                    timespan,
                    response.Exception?.Message ?? response.Result?.StatusCode.ToString()
                );
            })
    )
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
        TimeSpan.FromSeconds(Convert.ToInt32(builder.Configuration["Http:RetryTimeout"]!)),
        TimeoutStrategy.Optimistic
    ));

#endregion

#region Authentication

//Add Jwt Authentication
builder.Services.AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme,
        options => { });


#endregion

#region Swagger

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        BearerFormat = "JWT",
        Scheme = "bearer",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Description =
            "Enter 'Bearer' [space] and then your valid token in the text input below.\n\nExample: \"Bearer eyJhb...\""
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region HTTP Logging

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.RequestQuery |
        HttpLoggingFields.RequestBody |
        HttpLoggingFields.ResponseBody;
});

#endregion

#region Entrypoint

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandlingMiddleware();

app.UseRequestBufferingMiddleware();

app.UseSerilogRequestLogging();

app.UseHttpLogging();

app.UseRouting();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();


// Route configuration
app.MapControllerRoute(
    "default",
    "{controller}/{action}/{id?}");

app.Run();

#endregion