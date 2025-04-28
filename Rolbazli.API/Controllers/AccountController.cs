using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Rolbazli.API.DTOs;
using RoleBazli.Model.Models;

namespace Rolbazli.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AccountController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        //post isteği: api/account/register
        public async Task<ActionResult<string>> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);//geçersiz 400 hatası döner.
            }

            var user = new AppUser
            {
                Email = registerDTO.Email,
                FullName = registerDTO.FullName,
                UserName = registerDTO.Email //genelde kullanıcı adı e-posta atanır.
            };

            //UserManger yardımıyla kullanıcı oluşturulur ve şifre atanır.
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            //eğer kullanıcı oluşturulmadıysa hata dönsün
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            //kullanıcı kayıt oluştururken eğer role seçmezse varsayılan olarak "User" atansın.
            if (registerDTO.Roles is null)
            {
                await _userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                //eğer roller varsa her biri kullanıcıya atanabilir. kullanıcı bu durumda istediği rolü semesine izin vermemiz gerekir.
                foreach (var role in registerDTO.Roles)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            //başarılı bir şekilde kayıt oluştuysa cevap olarak bir DTO döner.
            return Ok(new AuthResponseDTO
            {
                IsSuccess = true,
                Message = "Hesabınız başarıyla oluşturuldu."
            });
        }

        [HttpPost("login")]
        //post isteği: api/account/login
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            //Eğer kullanıcı bulunamadıysa
            if (user is null)
            {
                return Unauthorized(new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Bu e-posta ile kayıt bulunamadı."
                });
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
            //eğer şifre hatalıysa
            if (!result)
            {
                return Unauthorized(new AuthResponseDTO
                {
                    IsSuccess = false,
                    Message = "Şifre hatalı!"
                });
            }
            var token = GenerateToken(user);

            return Ok(new AuthResponseDTO
            {
                Token = token,
                IsSuccess = true,
                Message = "Giriş başarılı"
            });
        }

        private string GenerateToken(AppUser appUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JWTSetting").GetSection("secretKey").Value!);
            var roles = _userManager.GetRolesAsync(appUser).Result;

            List<Claim> claims =
            [
                new Claim(JwtRegisteredClaimNames.Email, appUser.Email!),
                new Claim(JwtRegisteredClaimNames.Name, appUser.FullName!),
                new Claim(JwtRegisteredClaimNames.NameId, appUser.Id.ToString()!),
                new Claim(JwtRegisteredClaimNames.Aud, _configuration.GetSection("JWTSetting").GetSection("validAudience").Value!),
                new Claim(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWTSetting").GetSection("validIssuer").Value!),
            ];

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),// token 7 gün geçerli olacak.
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}