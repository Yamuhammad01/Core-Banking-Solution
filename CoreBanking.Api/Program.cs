using CoreBanking.Api.Extensions;
using CoreBanking.Api.Swagger;
using CoreBanking.Application.Common;
using CoreBanking.Application.Identity;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Application.Services;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs;
using CoreBanking.Infrastructure.Configuration;
using CoreBanking.Infrastructure.EmailServices;
using CoreBanking.Infrastructure.Identity;
using CoreBanking.Infrastructure.Messaging;
using CoreBanking.Infrastructure.Persistence;
using CoreBanking.Infrastructure.Repository;
using CoreBanking.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;



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
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<IEmailSenderr, EmailSender>();
builder.Services.AddScoped(sp =>
    new EmailTemplateService(builder.Environment.ContentRootPath));

var baseUrl = builder.Configuration["Monnify:BaseUrl"];
if (string.IsNullOrEmpty(baseUrl))
{
    throw new Exception("Monnify:BaseUrl is not configured in appsettings.json");
}

builder.Services.AddHttpClient<IMonnifyService, MonnifyService>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
    Console.WriteLine("HttpClient BaseAddress: " + client.BaseAddress);
});


builder.Services.AddScoped<IBankingDbContext>(provider => provider.GetRequiredService<CoreBankingDbContext>());
builder.Services.AddScoped<IEmailTemplateService>(provider => provider.GetRequiredService<EmailTemplateService>());
builder.Services.AddScoped<ICodeHasher>(provider => provider.GetRequiredService<CodeHasher>());

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionPinService, TransactionPinService>();
builder.Services.AddScoped<ITransactionEmailService, TransactionEmailService>();
builder.Services.AddScoped<ICodeHasher, CodeHasher>();
builder.Services.AddScoped<IPinValidationService, PinValidationService>();
builder.Services.AddHostedService<RegistrationConsumer>();

builder.Services.AddHttpClient<IMonnifyService, MonnifyService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Monnify:BaseUrl"]);
});




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

app.MapPost("/api/customerrs", async (CustomerDto dto) =>
{
    var factory = new ConnectionFactory()
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest",
        Port = 5672
    };

    // await async connection
    await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(
        exchange: "corebank.exchange",
        type: ExchangeType.Direct,
        durable: true
    );

    await channel.QueueDeclareAsync(
        queue: "registration.queue",
        durable: true,
        exclusive: false,
        autoDelete: false
    );

    await channel.QueueBindAsync(
        queue: "registration.queue",
        exchange: "corebank.exchange",
        routingKey: "registration.create"
    );

    var message = new CustomerCreatedMessage(
        dto.FirstName,
        dto.LastName,
        dto.Email,
        dto.Password,
        dto.ConfirmPassword,
        dto.PhoneNumber
    );

    var json = JsonSerializer.Serialize(message);
    var body = Encoding.UTF8.GetBytes(json);

    await channel.BasicPublishAsync(
         exchange: "corebank.exchange",
         routingKey: "registration.create",
         mandatory: false,
         body: body,
        cancellationToken: CancellationToken.None
    );

    return Results.Accepted();
});


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

var addresses = app.Urls;
foreach (var address in addresses)
{
    Console.WriteLine($"App listening on: {address}");
}

app.Run();
public record CustomerDto(string FirstName, string LastName, string Email, string Password, string ConfirmPassword, string PhoneNumber);
public record CustomerCreatedMessage(string FirstName, string LastName, string Email, string Password, string ConfirmPassword, string PhoneNumber);
