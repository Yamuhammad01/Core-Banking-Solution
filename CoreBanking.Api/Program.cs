using CoreBanking.Api.Extensions;
using CoreBanking.Api.Swagger;
using CoreBanking.Application.Command.RegisterCommand;
using CoreBanking.Application.Common;
using CoreBanking.Application.Identity;
using CoreBanking.Application.Interfaces.IMailServices;
using CoreBanking.Application.Interfaces.IRepository;
using CoreBanking.Application.Interfaces.IServices;
using CoreBanking.Application.Security;
using CoreBanking.Application.Services;
using CoreBanking.Application.Shared;
using CoreBanking.Domain.Entities;
using CoreBanking.DTOs;
using CoreBanking.Infrastructure.Configuration;
using CoreBanking.Infrastructure.EmailServices;
using CoreBanking.Infrastructure.Identity;
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
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreBanking.Infrastructure.Messaging.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CoreBankingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Email goes through Brevo's HTTPS API (port 443). Render's free tier blocks
// outbound SMTP ports 25/465/587, so no SMTP transport is used.
builder.Services.AddHttpClient<IEmailSenderr, EmailSender>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
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

var paystackBaseUrl = builder.Configuration["Paystack:BaseUrl"];
Console.WriteLine("Paystack HttpClient BaseAddress: " + paystackBaseUrl);

// Register PaystackService with HttpClient and BaseAddress
builder.Services.AddHttpClient<IPayStackService, PaystackService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<PaystackSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", settings.SecretKey);
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
// ---- RabbitMQ (optional, env-var friendly) --------------------------------------
// Local: appsettings.json "RabbitMq" section.
// Render: environment variables, e.g. RabbitMq__Url=amqps://user:pass@host/vhost (CloudAMQP)
//         or RabbitMq__Host / RabbitMq__Port / RabbitMq__User / RabbitMq__Password.
//         RabbitMq__Enabled=false runs the API without a broker.
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqSettings>()
    ?? new RabbitMqSettings();

builder.Services.AddSingleton(rabbitMqSettings);

// RabbitMQ outages must never take the API down: ignore background service exceptions instead of throwing.
// BrokerUnreachableException, so worker failures must never stop the host.
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// The MassTransit bus starts in the background; host startup must not depend on the broker.
builder.Services.Configure<MassTransit.MassTransitHostOptions>(options => options.WaitUntilStarted = false);

builder.Services.AddMassTransitServices(rabbitMqSettings); // MassTransit config
//builder.Services.AddHttpClient<IVirtualAccountService, PaystackService>();
builder.Services.Configure<PaystackSettings>(
    builder.Configuration.GetSection("Paystack"));





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

app.MapPost("/api/customerrs", async (
    CustomerDto dto,
    RabbitMqSettings rabbitMq,
    IMediator mediator,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("CustomerRegistration");

    if (rabbitMq.IsBrokerConfigured)
    {
        try
        {
            await PublishRegistrationMessageAsync(dto, rabbitMq);
            return Results.Accepted();
        }
        catch (Exception ex)
        {
            // Broker configured but unreachable: degrade gracefully instead of failing the request.
            logger.LogError(ex, "Failed to publish the registration message to RabbitMQ. Running the registration in-process instead.");
        }
    }
    else
    {
        logger.LogWarning("RabbitMQ is not configured; running the registration in-process.");
    }

    // Same flow the queue consumer uses (CoreBanking.Application RegisterCommandHandler):
    // Identity user + bank account, so registration works even without a broker.
    var result = await mediator.Send(new RegisterCommand
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Password = dto.Password,
        ConfirmPassword = dto.ConfirmPassword,
        PhoneNumber = dto.PhoneNumber
    });

    if (!result.Succeeded)
    {
        logger.LogWarning("In-process registration failed for {Email}: {Reason}", dto.Email, result.Message);
        return Results.BadRequest(new { message = result.Message });
    }

    return Results.Accepted(value: new
    {
        message = result.Message,
        note = "Processed in-process because no message broker is available."
    });
});

static async Task PublishRegistrationMessageAsync(CustomerDto dto, RabbitMqSettings rabbitMq)
{
    var factory = rabbitMq.CreateConnectionFactory();

    await using var connection = await factory.CreateConnectionAsync("CoreBanking.Api:CustomerPublish");
    await using var channel = await connection.CreateChannelAsync();

    await channel.ExchangeDeclareAsync(
        exchange: rabbitMq.Exchange,
        type: ExchangeType.Direct,
        durable: true);

    await channel.QueueDeclareAsync(
        queue: rabbitMq.Queue,
        durable: rabbitMq.DurableQueue,
        exclusive: false,
        autoDelete: false);

    await channel.QueueBindAsync(
        queue: rabbitMq.Queue,
        exchange: rabbitMq.Exchange,
        routingKey: rabbitMq.RoutingKey);

    var message = new CustomerCreatedMessage(
        dto.FirstName,
        dto.LastName,
        dto.Email,
        dto.Password,
        dto.ConfirmPassword,
        dto.PhoneNumber);

    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

    await channel.BasicPublishAsync(
        exchange: rabbitMq.Exchange,
        routingKey: rabbitMq.RoutingKey,
        mandatory: false,
        body: body,
        cancellationToken: CancellationToken.None);
}


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
