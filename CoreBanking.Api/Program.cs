using Microsoft.EntityFrameworkCore;
using CoreBanking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using CoreBanking.DTOs;
using CoreBanking.Api.Swagger;
using CoreBanking.Domain.Entities;
using CoreBanking.Infrastructure.Repository;
using CoreBanking.Application.Services;
using CoreBanking.Infrastructure.Identity;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Security.Claims;
using CoreBanking.Application.Identity;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using System.Net.Mail;
using System.Net;
using CoreBanking.Api.Extensions;
using CoreBanking.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using CoreBanking.Infrastructure.EmailServices;
using CoreBanking.Infrastructure.Services;
using Microsoft.Extensions.Options;
using MediatR;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Security;
using CoreBanking.Application.Common;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CoreBankingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IEmailSenderr, EmailSender>();
// port configuration for Render Deployment

//var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddIdentityApiEndpoints<Customer>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<CoreBankingDbContext>()
    .AddDefaultTokenProviders(); 
/*builder.Services.AddIdentity<Customer, IdentityRole>()
    .AddEntityFrameworkStores<CoreBankingDbContext>()
    .AddDefaultTokenProviders();  */

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Converts enums to strings in JSON 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<TransactionPinService>();
builder.Services.AddScoped<IEmailSenderr, EmailSender>();
builder.Services.AddScoped(sp =>
    new EmailTemplateService(builder.Environment.ContentRootPath));

builder.Services.AddScoped<IBankingDbContext>(provider => provider.GetRequiredService<CoreBankingDbContext>());
builder.Services.AddScoped<IEmailTemplateService>(provider => provider.GetRequiredService<EmailTemplateService>());
builder.Services.AddScoped<ICodeHasher>(provider => provider.GetRequiredService<CodeHasher>());

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionPinService, TransactionPinService>();
builder.Services.AddScoped<ITransactionEmailService, TransactionEmailService>();
builder.Services.AddScoped<ICodeHasher, CodeHasher>();
builder.Services.AddScoped<IPinValidationService, PinValidationService>();
builder.Services.AddScoped<IAdminService, AdminService>();


//builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddHttpContextAccessor();


builder.Services.Configure<AdminSettings>(
    builder.Configuration.GetSection("Admin"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

builder.Services.AddFluentEmailConfiguration(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CoreBanking.Application.AssemblyMarker).Assembly));


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };
});


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter token: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    c.DocumentFilter<RemoveIdentityRegisterDocumentFilter>();
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<Customer>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var adminConfig = services.GetRequiredService<IOptions<AdminSettings>>();

    await RoleIdentity.SeedAsync(userManager, roleManager, adminConfig);
}

var authGroup = app.MapGroup("/api/auth");
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


//app.MapIdentityApi<IdentityUser>();

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => "Core Banking API is running");// simple health check endpoint 
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });


authGroup.MapIdentityApi<Customer>();


app.MapControllers();

app.Run();
