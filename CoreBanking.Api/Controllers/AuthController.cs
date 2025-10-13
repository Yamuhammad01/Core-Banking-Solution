using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CoreBanking.Domain.Entities;
using CoreBanking.Application.Services;
using Microsoft.EntityFrameworkCore;
using CoreBanking.Infrastructure.Persistence;
using CoreBanking.Infrastructure.Services;
using CoreBanking.DTOs.AccountDto;


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
        public AuthController(UserManager<Customer> userManager, SignInManager<Customer> signInManager, JwtService jwtService, CoreBankingDbContext coreBankingDbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = coreBankingDbContext;
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


            return Ok(new
            {
                message = "User registered successfully",
                email = user.Email
            });
        }


        [HttpPost("customer/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return Unauthorized("Invalid credentials");

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
