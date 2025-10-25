using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CoreBanking.Domain.Entities;
using CoreBanking.Application.Services;
using Microsoft.EntityFrameworkCore;
using CoreBanking.Infrastructure.Persistence;
using CoreBanking.Infrastructure.EmailServices;
using CoreBanking.DTOs.AccountDto;
using CoreBanking.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Identity.UI.Services;
using CoreBanking.Application.Common;
using CoreBanking.Application.CommandHandlers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CoreBanking.Application.Command;


namespace CoreBanking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Customer> _userManager;
        private readonly SignInManager<Customer> _signInManager;
        private readonly JwtService _jwtService;
        private readonly CoreBankingDbContext _context;
        private readonly IEmailSenderr _emailSender;
        private readonly EmailTemplateService _emailTemplateService;
        private readonly IMediator _mediator;
        public AuthController(
            UserManager<Customer> userManager, 
            SignInManager<Customer> signInManager, 
            JwtService jwtService, 
            CoreBankingDbContext coreBankingDbContext,
            IEmailSenderr emailSender,
            EmailTemplateService emailTemplateService,
            IMediator mediator
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = coreBankingDbContext;
            _emailSender = emailSender;
            _emailTemplateService = emailTemplateService;
            _mediator = mediator;
        }
        [HttpPost("customer/register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto register)
        {
            if (register.Password != register.ConfirmPassword)
                return BadRequest(new { message = "Passwords do not match." });

            var userExists = await _userManager.FindByEmailAsync(register.Email);
            if (userExists != null)
                return BadRequest(new { message = "User with this email already exists." });

            var user = new Customer
            {
                UserName = register.Email,
                Email = register.Email,
                FirstName = register.FirstName,
                LastName = register.LastName,
                PhoneNumber = register.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });



            // Create default BankAccount after user registration
            var bankAccount = new BankAccount
            {
                CustomerId = user.Id,
                AccountNumber = await GenerateUniqueAccountNumberAsync(),
                Balance = 0m, // default balance
                AccountType = "Savings",
                Currency = "NGN",
                Status = "Active"
            };

            _context.BankAccounts.Add(bankAccount);
            await _context.SaveChangesAsync();

            var emailBody = await _emailTemplateService.GetWelcomeTemplateAsync(
              user.FirstName, user.LastName, bankAccount.AccountNumber, bankAccount.Currency); // get the placeholder to use in the template

            var message = new Message(
                new string[] { register.Email },
                "Welcome to CoreBanking",
                emailBody
            );


            await _emailSender.SendEmailAsync(message);

            return Ok(new
            {
                message = "User registered successfully, welcome email has been sent",
                email = user.Email
                
            });
        }
        [HttpPost("customer/reg/free/registerrs")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("customer/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            if (!user.EmailConfirmed)
            {
                return BadRequest("Comfirm your email before login please");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");
            var roles = await _userManager.GetRolesAsync(user);

            var token = await _jwtService.GenerateTokenAsync(user);

            return Ok(new
            {
                access_token = token,
                expires_in = 3600
            });
        }
        [Authorize]
        [HttpPost("send-emailconfirmation-code")]
        public async Task<IActionResult> SendEmailCode()
        {
            var result = await _mediator.Send(new SendEmailConfirmationCommand());

            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailConfirmationCommand command)
        {
            await _mediator.Send(command);
            return Ok(new { message = "Email confirmed successfully" });
        }



        private async Task<string> GenerateUniqueAccountNumberAsync()
        {
            const string bankCode = "811";
            string accountNumber;
            bool exists;

            do
            {
                // Generate 7 random digits
                var random = new Random();
                var randomDigits = random.Next(0, 9999999).ToString("D7"); // pad with zeros
                accountNumber = bankCode + randomDigits;

                // Ensure it's unique b4 creating new account
                exists = await _context.BankAccounts
                    .AnyAsync(b => b.AccountNumber == accountNumber);

            } while (exists);

            return accountNumber;
        }


    }
    public record LoginRequestDto(string Email, string Password);
}
