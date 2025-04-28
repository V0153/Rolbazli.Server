using System.ComponentModel.DataAnnotations;

namespace Rolbazli.API.DTOs
{
    public class RegisterDTO //Kullanıcının kayıt(register) sırasında göndereceği verilleri tutan DTO (Data Transfer Object) sınıfı.
    {
        [Required,EmailAddress] //Bu alanın zorunlu olduğunu belirtir.
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; //FullName alanı varsayılan olarak boş bir string ile başlar.
        public string Password { get; set; } = string.Empty;
        public List<string>? Roles { get; set; }
    }
}