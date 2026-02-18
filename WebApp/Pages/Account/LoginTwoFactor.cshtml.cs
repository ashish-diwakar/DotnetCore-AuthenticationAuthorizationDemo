using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    public class LoginTwoFactorModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;
        private readonly SignInManager<User> signInManager;

        [BindProperty]
        public LoginTwoFactorViewModel loginTwoFactorViewModel { get; set; }

        public LoginTwoFactorModel(UserManager<User> userManager, IEmailService emailService, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.signInManager = signInManager;
            this.loginTwoFactorViewModel = new LoginTwoFactorViewModel
            {
                Email = string.Empty,
                SecurityCode = string.Empty,
                RememberMe = false
            };
        }


        public async Task OnGetAsync(string Email, bool RememberMe)
        {
            this.loginTwoFactorViewModel.Email= Email;
            this.loginTwoFactorViewModel.RememberMe = RememberMe;
            this.loginTwoFactorViewModel.SecurityCode = string.Empty;
            var user = await userManager.FindByEmailAsync(Email);
            if (user is not null)
            {
                var securityCode = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
                await emailService.SendEmailAsync(Email, "Your security code", $"Please use this code as your OTP/Security code : {securityCode}");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await userManager.FindByEmailAsync(loginTwoFactorViewModel.Email);
            if (user is not null)
            {
                var result = await signInManager.TwoFactorSignInAsync("Email", loginTwoFactorViewModel.SecurityCode, loginTwoFactorViewModel.RememberMe, rememberClient: false);
                if (result.Succeeded)
                {
                    return RedirectToPage("/Index");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Login2FA", "Account locked out.");
                    return Page();
                }
                else
                {
                    ModelState.AddModelError("Login2FA", "Invalid security code.");
                    return Page();
                }
            }
            else
            {
                ModelState.AddModelError("Login2FA", "User not found.");
                return Page();
            }
        }
    }

    public class LoginTwoFactorViewModel
    {
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SecurityCode { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
