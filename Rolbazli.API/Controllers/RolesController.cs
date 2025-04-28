using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Rolbazli.API.DTOs;
using RoleBazli.Model.Models;

namespace Rolbazli.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody]CreateRoleDTO createRoleDTO) 
        {
            if (string.IsNullOrEmpty(createRoleDTO.RoleName))
            {
                return BadRequest("Rol boş bırakılamaz");
            }
            
            var roleExists = await _roleManager.RoleExistsAsync(createRoleDTO.RoleName);
            if (roleExists)
            {
                return BadRequest("Rol zaten mevcut");
            }
            
            var RoleResult = await _roleManager.CreateAsync(new IdentityRole(createRoleDTO.RoleName));
            if (RoleResult.Succeeded)
            {
                return Ok("Rol başarıyla oluşturuldu");
            }
                return BadRequest("Rol oluşturulurken hata oluştu");
        }
    }
}
