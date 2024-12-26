using System.Net;
using System.Text;
using System.Text.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using clean_architecture_template.Middlewares;
using clean_architecture_template.Models;
using Core.ServiceContracts;
using Core.Services;
using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Timeout;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Autofac DI container

// Configuring DI Container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services to the container.
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<DatabaseFactory>().SingleInstance();

    containerBuilder.RegisterType<MiscService>().As<IMiscService>().InstancePerLifetimeScope();
    //containerBuilder.RegisterType<JwtService>().As<IJwtService>().InstancePerLifetimeScope();
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
        .WriteTo.File(
            $"{builder.Configuration["LogFilePath"]}{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}/log-.txt",
            rollingInterval: RollingInterval.Hour,
            retainedFileCountLimit: 7
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

// To handle invalid model state error from Required attribute
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(new Response
        {
            Status = -1,
            Error = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault()
        });
    };
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

#region Basic Auth

// Add Basic Authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = "Basic";
//})
//.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", options => { });

#endregion

#region JWT

// Add Jwt Authentication
//builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateAudience = true,
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            ValidateIssuer = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
//        };
//        options.Events = new JwtBearerEvents
//        {
//            OnAuthenticationFailed = context =>
//            {
//                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
//                logger.LogError("Authentication failed: {Error}", context.Exception.Message);
//                context.Response.StatusCode = 401;
//                return Task.FromResult(context.Response.WriteAsJsonAsync(new Response().Unauthorized()));
//            }
//        };
//    });

#endregion

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
        HttpLoggingFields.RequestHeaders |
        HttpLoggingFields.RequestQuery |
        HttpLoggingFields.RequestBody |
        HttpLoggingFields.ResponseStatusCode;
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