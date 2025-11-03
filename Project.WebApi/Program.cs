using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Timeout;
using Project.Core.Application;
using Project.Core.Interfaces;
using Project.Infrastructure;
using Project.Infrastructure.Clients;
using Project.Infrastructure.Repositories;
using Project.WebApi.Authentication;
using Project.WebApi.Middlewares;
using Project.WebApi.Models;
using Serilog;
using Serilog.Events;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

#region Autofac DI container

// Configuring DI Container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container.
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<DatabaseFactory>().SingleInstance();
    containerBuilder.RegisterType<HttpPollyClient>()
        .SingleInstance();

    containerBuilder.RegisterType<UserUseCase>().InstancePerLifetimeScope();
    containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();

    containerBuilder.RegisterType<JwtAuth>().InstancePerLifetimeScope();
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

// Add Default HttpClient
builder.Services.AddHttpClient(nameof(HttpExternalClients.Default), client =>
    {
        client.Timeout = TimeSpan.Parse(builder.Configuration["Http:Default:RetryTimeout"]!);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .OrResult(response =>
            response.StatusCode
                is HttpStatusCode.InternalServerError
                or HttpStatusCode.GatewayTimeout
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.NotFound
        )
        .WaitAndRetryAsync(
            Convert.ToInt32(builder.Configuration["Http:Default:RetryCount"]!),
            retryAttempt => TimeSpan.Parse(builder.Configuration["Http:Default:RetryAfter"]!),
            onRetry: (response, timespan, retryCount, _) =>
            {
                Log.Warning(
                    "Retrying {RetryCount}/{MaxRetries} after {Delay} due to {Reason}",
                    retryCount,
                    builder.Configuration["Http:Default:RetryCount"],
                    timespan,
                    response.Exception?.Message ?? response.Result?.StatusCode.ToString()
                );
            })
    )
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.Parse(builder.Configuration["Http:Default:RetryTimeout"]!),
        TimeoutStrategy.Optimistic
    ));

// Add Default HttpClient
builder.Services.AddHttpClient(nameof(HttpExternalClients.Weather), client =>
    {
        client.Timeout = TimeSpan.Parse(builder.Configuration["Http:Weather:RetryTimeout"]!);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .OrResult(response =>
            response.StatusCode
                is HttpStatusCode.InternalServerError
                or HttpStatusCode.GatewayTimeout
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.NotFound
        )
        .WaitAndRetryAsync(
            Convert.ToInt32(builder.Configuration["Http:Weather:RetryCount"]!),
            retryAttempt => TimeSpan.Parse(builder.Configuration["Http:Weather:RetryAfter"]!),
            onRetry: (response, timespan, retryCount, _) =>
            {
                Log.Warning(
                    "Retrying {RetryCount}/{MaxRetries} after {Delay} due to {Reason}",
                    retryCount,
                    builder.Configuration["Http:Weather:RetryCount"],
                    timespan,
                    response.Exception?.Message ?? response.Result?.StatusCode.ToString()
                );
            })
    )
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.Parse(builder.Configuration["Http:Weather:RetryTimeout"]!),
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

app.UseHttpTraceIdentifierLoggingMiddleware();

app.UseSerilogRequestLogging();

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