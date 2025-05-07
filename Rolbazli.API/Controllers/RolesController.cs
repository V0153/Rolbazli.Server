using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rolbazli.API.DTOs;
using RoleBazli.Model.Models;

namespace Rolbazli.API.Controllers
{
    [Authorize(Roles = "Admin")]
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
        [HttpGet("get-roles")]
        public async Task<ActionResult<IEnumerable<RoleResponseDTO>>> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDtos = new List<RoleResponseDTO>();

            foreach (var role in roles)
            {
                var userInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
                roleDtos.Add(new RoleResponseDTO
                {
                    Id = role.Id,
                    Name = role.Name,
                    TotalUsers = userInRole.Count
                });
            }
            return Ok(roleDtos);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoleById(string id)
        {
            //kullanıcının girdiği id'yi bul
            var roleId = await _roleManager.FindByIdAsync(id);
            if (roleId is null)
            {
                return NotFound("Role not found");
            }
            var result = await _roleManager.DeleteAsync(roleId);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role deleted successfully." });
            }
            return BadRequest("Role deletion failed!..");
        }

        [HttpPost("role-assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDTO assignRoleDTO)
        {
            var user = await _userManager.FindByIdAsync(assignRoleDTO.UserId);
            if (user is null)
            {
                return NotFound("User not found!..");
            }
            var role = await _roleManager.FindByIdAsync(assignRoleDTO.RoleId);
            if (role is null)
            {
                return NotFound("Role not found!..");
            }
            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Role assign successfullyy" });
            }
            var error = result.Errors.FirstOrDefault();
            return BadRequest(error!.Description);
        }
    }
}
