using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        public LoginModel(SignInManager<User> signInManager)
        {
            SignInManager = signInManager;
        }

        [BindProperty]
        public CredentialViewModel Credential { get; set; } = new CredentialViewModel();
        public SignInManager<User> SignInManager { get; }

        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var result = await SignInManager.PasswordSignInAsync(Credential.Email, Credential.Password, Credential.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                if(result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginTwoFactor", new { RememberMe = Credential.RememberMe, Email = Credential.Email });    
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login", "Account locked out.");
                }
                else
                {
                    ModelState.AddModelError("Login", "Invalid username or password.");
                }
                return Page();
            }
        }
    }

    public class CredentialViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
