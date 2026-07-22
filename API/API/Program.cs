 

using System;
using System.Text;
using API.Application.Repository;
using API.Application.Repository.IRepository;
using API.Application.Service.ProfilService;
using API.Application.Services;
using API.Application.Services.IServices;
using Auth.Application.Interfaces;
using Auth.Application.Services;
using Auth.DependencyInjection;
using Authorization.Application.Interfaces;
using Authorization.DependencyInjection;
using Authorization.Domain.Entities;
using Infrastructure;
using Infrastructure.Application.Interfaces;
using Infrastructure.Application.Services;
using Infrastructure.DependencyInjection;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using Settings.Application.Interfaces;
using SYSGES_MAGs.Helpers;
using Users.DependencyInjection;
using Users.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

#region Services

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

#endregion

#region Dependency Injection

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IPermissionGenerator, PermissionGenerator>();

builder.Services.AddScoped<IdentitySeeder, IdentitySeeder>();
builder.Services.AddScoped<SettingSeeder, SettingSeeder>();

builder.Services.AddScoped<ILoginTrackingService, LoginTrackingService>();
builder.Services.AddScoped<ILdapService, LdapService>();

builder.Services.AddScoped<ISettingService, SettingService>();

builder.Services.AddScoped<IProfilService, ProfilService>();
builder.Services.AddScoped<IProfilRepository, ProfilRepository>();

builder.Services.AddScoped<IMagProcessingService, MagProcessingService>();

builder.Services.AddScoped<IBkmvtiRepository, BkmvtiRepository>();
builder.Services.AddScoped<ITypeMagRepository, TypeMagRepository>();
builder.Services.AddScoped<IBkmvtiService, BkmvtiService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IComptesOuvertRepository, ComptesOuvertRepository>();
builder.Services.AddScoped<IComptesOuvertService, ComptesOuvertService>();

builder.Services.AddScoped<ICompteDebiteRedevCarteService, CompteDebiteRedevCarteService>();
builder.Services.AddScoped<IComptesDebiteRedevCarteRepository, ComptesDebiteRedevCarteRepository>();

builder.Services.AddScoped<MagProcessingHelper>();

builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 Mo
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(15);
});

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowUse_Case_Carte",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5239",
                    "https://localhost:5229",
                    "http://localhost:5074",
                    "https://localhost:7014"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

#endregion

#region Identity

builder
    .Services.AddIdentity<ApplicationUser, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

#endregion

#region Database

var connectionStringSql = builder.Configuration.GetConnectionString("SqlServerConnection");

ExcelPackage.License.SetNonCommercialPersonal("Ton Nom");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionStringSql)
);

#endregion

#region Authentication

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
            ),
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"Jwt OnAuthenticationFailed: {ctx.Exception?.Message}");

                return Task.CompletedTask;
            },

            OnTokenValidated = ctx =>
            {
                Console.WriteLine("Jwt OnTokenValidated: token valid");

                return Task.CompletedTask;
            },

            OnChallenge = ctx =>
            {
                Console.WriteLine(
                    $"Jwt OnChallenge: Error={ctx.Error} Description={ctx.ErrorDescription}"
                );

                return Task.CompletedTask;
            },
        };
    });

#endregion

#region Modules

builder.Services.AddInfrastructure().AddUsersModule().AddAuthorizationModule().AddAuthModule();

#endregion

var app = builder.Build();

#region Database Initialization

using (var scope = app.Services.CreateScope())
{
    // Génération automatique des permissions
    var generator = scope.ServiceProvider.GetRequiredService<IPermissionGenerator>();

    await generator.GenerateAsync();

    // Création du rôle et de l'utilisateur administrateur
    var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();

    await seeder.SeedAsync();

    // Initialisation des paramètres par défaut
    var settingSeeder = scope.ServiceProvider.GetRequiredService<SettingSeeder>();

    await settingSeeder.SeedAsync();
}

#endregion

#region Development

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

#endregion

#region Middlewares

app.Use(
    async (context, next) =>
    {
        Console.WriteLine($"{context.Request.Method} {context.Request.Path}");

        await next();
    }
);

app.UseCors("AllowUse_Case_Carte");

app.UseHttpsRedirection();

app.UseAuthentication();

app.Use(
    async (context, next) =>
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        var isAuth = context.User?.Identity?.IsAuthenticated ?? false;

        Console.WriteLine($"Incoming Authorization: {authHeader}");
        Console.WriteLine($"HttpContext.User.Identity.IsAuthenticated: {isAuth}");

        await next();
    }
);

app.UseAuthorization();

#endregion

#region Endpoints

app.MapControllers();

#endregion

app.Run();
