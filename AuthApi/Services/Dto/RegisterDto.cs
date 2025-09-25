using System.ComponentModel.DataAnnotations;

namespace AuthApi.Services.Dto
{
    public class RegisterDto
    {

        [Required(ErrorMessage = "Fullname is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
