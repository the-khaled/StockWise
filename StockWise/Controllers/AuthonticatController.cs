using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using StockWise.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthonticatController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        public AuthonticatController
            (UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailService emailService, IJwtService jwtService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;   
            _jwtService = jwtService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto UserformRequest)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = UserformRequest.UserName,
                    Email = UserformRequest.Email
                };

                IdentityResult result = await _userManager.CreateAsync(user, UserformRequest.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return Ok(new { message = "User registered successfully. Please login." });
                }
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("AddToRole")]
        [Authorize(Roles = "Admin")] // بس الـ Admin يقدر يستخدمه
        public async Task<IActionResult> AddToRole([FromBody] AddToRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound("User not found.");

            var result = await _userManager.AddToRoleAsync(user, dto.Role);
            return result.Succeeded ? Ok($"User is now {dto.Role}") : BadRequest(result.Errors);
        }

        public class AddToRoleDto
        {
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto Userfromrequest)
        {
            if (ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    var UserFromDB = await _userManager.FindByNameAsync(Userfromrequest.UserName);
                    if (UserFromDB != null)
                    {
                        bool Found = await _userManager.CheckPasswordAsync(UserFromDB, Userfromrequest.Password);
                        if (Found)
                        {
                            // 1. جيب الـ Roles
                            var userRole = await _userManager.GetRolesAsync(UserFromDB);

                            // 2. استخدم IJwtService لتوليد Access Token
                            var accessToken = _jwtService.GenerateAccessToken(UserFromDB, userRole);

                            // 3. استخدم IJwtService لتوليد Refresh Token
                            var refreshToken = _jwtService.GenerateRefreshToken();

                            // 4. احفظ الـ Refresh Token في الـ DB
                            UserFromDB.RefreshToken = refreshToken;
                            UserFromDB.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
                            await _userManager.UpdateAsync(UserFromDB);

                            // 5. رجّع Access + Refresh
                            return Ok(new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                                refreshToken = refreshToken,
                                expiration = accessToken.ValidTo
                            });
                        }
                    }
                    ModelState.AddModelError("Username", "Username or password invalid");
                }
            }
                return BadRequest(ModelState);
        }
        
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Ok("If the email exists, a password reset link has been sent.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Authonticat",
                new { email = model.Email, token = encodedToken },
                Request.Scheme);

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #007bff;'>Reset Your Password</h2>
                    <p>Click the button below to reset your password:</p>
                    <a href='{callbackUrl}' 
                       style='background: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>
                       Reset Password
                    </a>
                    <p style='margin-top: 20px; color: #666; font-size: 12px;'>
                        This link will expire in 1 hour.
                    </p>
                </div>";

            await _emailService.SendEmailAsync(user.Email!, "Reset Your Password", emailBody);

            return Ok("Password reset email sent successfully.");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid request.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
                return Ok("Password has been reset successfully. You can now login.");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return BadRequest(ModelState);
        }
        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest("Refresh token is required");

            // 1. جيب المستخدم من الـ Refresh Token
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return BadRequest("Invalid or expired refresh token");

            // 2. استخدم IJwtService لتوليد Access Token جديد
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtService.GenerateAccessToken(user, roles);

            // 3. استخدم IJwtService لتوليد Refresh Token جديد
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // 4. احفظ الـ Refresh الجديد
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            // 5. رجّع الـ Tokens الجديدة
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken,
                expiration = newAccessToken.ValidTo
            });
        }

    }
}