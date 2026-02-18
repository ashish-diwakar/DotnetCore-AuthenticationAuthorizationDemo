using System.ComponentModel.DataAnnotations;

namespace SecurityDemo.Authorization
{
    public class Credential
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "User Name")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
