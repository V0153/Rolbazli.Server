using System.ComponentModel.DataAnnotations;

namespace Rolbazli.API.DTOs
{
    public class CreateRoleDTO
    {
        [Required(ErrorMessage ="Rol ismi zorunlu")]
        public string RoleName { get; set; } = null!;
    }
}
