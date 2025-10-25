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
using CoreBanking.Application.Command.PasswordResetCommand;


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

        [HttpPost("send-confirmation-code")]
        public async Task<IActionResult> SendConfirmationCode([FromBody] SendEmailConfirmationCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailConfirmationCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        [HttpPost("send-password-reset-code")]
        public async Task<IActionResult> SendPasswordResetCode([FromBody] SendPasswordResetCodeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("verify-password-reset-code")]
        public async Task<IActionResult> VerifyPasswordResetCode([FromBody] VerifyPasswordResetCodeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("resend-password-reset-code")]
        public async Task<IActionResult> ResendPasswordResetCode([FromBody] ResendPasswordResetCodeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
    public record LoginRequestDto(string Email, string Password);
}
