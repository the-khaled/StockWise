using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StockWise.Domain.Models;
using StockWise.Services.DTOS;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StockWise.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthonticatController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthonticatController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
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
                    return Ok(new { message = "User registered successfully. Please login." });

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("Password", item.Description);
                }
            }
            return BadRequest(ModelState);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto Userfromrequest)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser UserFromDB = await _userManager.FindByNameAsync(Userfromrequest.UserName);
                if (UserFromDB != null)
                {
                    bool Found = await _userManager.CheckPasswordAsync(UserFromDB, Userfromrequest.Password);
                    if (Found)
                    {
                        List<Claim> Userclaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, UserFromDB.Id),
                            new Claim(ClaimTypes.Name, UserFromDB.UserName!)
                        };

                        var userRole = await _userManager.GetRolesAsync(UserFromDB);
                        foreach (var Rolename in userRole)
                        {
                            Userclaims.Add(new Claim(ClaimTypes.Role, Rolename));
                        }

                        var SignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
                        var signingCre = new SigningCredentials(SignInKey, SecurityAlgorithms.HmacSha256);

                        JwtSecurityToken Mytoken = new JwtSecurityToken(
                            audience: _configuration["JWT:Audience"],
                            issuer: _configuration["JWT:Issuer"],
                            expires: DateTime.Now.AddHours(1),
                            claims: Userclaims,
                            signingCredentials: signingCre
                        );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(Mytoken),
                            expiration = Mytoken.ValidTo
                        });
                    }
                }
                ModelState.AddModelError("Username", "Username or password invalid");
            }
            return BadRequest(ModelState);
        }
    }
}