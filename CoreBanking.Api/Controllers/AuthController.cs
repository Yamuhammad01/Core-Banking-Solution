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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CoreBanking.Application.Command.PasswordResetCommand;
using CoreBanking.Application.Command.EmailConfirmationCommand;
using CoreBanking.Application.Command.RegisterCommand;
using CoreBanking.Application.Command.TransactionPinCommand;


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
                return BadRequest("Verify your email before login please");
            }
            //check if account is active
            if (!user.IsActive)
            {
                return BadRequest("Your account is not active");
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

        [HttpPost("confirm-email")]
        public async Task<IActionResult> SendConfirmationCode([FromBody] SendEmailCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailCodeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendPasswordResetCode([FromBody] ResendEmailCodeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> SendPasswordResetCode([FromBody] SendPasswordResetCodeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("verify-password")]
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

        [HttpPost("reset-transaction-pin")]
        public async Task<IActionResult> SendPinReset([FromBody] SendPinResetCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }

        [HttpPost("verify-transaction-pin")]
        public async Task<IActionResult> VerifyPinReset([FromBody] VerifyPinResetCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }

        [HttpPost("resend-transaction-pin-code")]
        public async Task<IActionResult> ResendPinReset([FromBody] ResendPinResetCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
             var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }

        //change transaction pin 
        [HttpPost("change-pin")]
        public async Task<IActionResult> ChangePin([FromBody] ChangePinCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }
    }
    public record LoginRequestDto(string Email, string Password);
}
