using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Services;
using static QRCoder.PayloadGenerator;

namespace WebApp.Pages.Account
{
    public class LoginTwoFactorWithAuthenticatorModel : PageModel
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private bool useGoogleAuthenticatorCode = false; // I've used this variable to test default code for Authenticator authorization & Google's Nuget package for same

        [BindProperty]
        public LoginTwoFactorWithAuthenticatorViewModel LoginTwoFactorWithAuthenticator { get; set; }
        public LoginTwoFactorWithAuthenticatorModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.LoginTwoFactorWithAuthenticator = new LoginTwoFactorWithAuthenticatorViewModel();
        }
        public void OnGet(string Email, bool RememberMe)
        {
            this.LoginTwoFactorWithAuthenticator.Email = Email;
            this.LoginTwoFactorWithAuthenticator.RememberMe = RememberMe;
            this.LoginTwoFactorWithAuthenticator.SecurityCode = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();


            if (useGoogleAuthenticatorCode)
            {
                #region Google TwoFactor Authenticator Code..
                var twoFactor = new TwoFactorAuthenticator();
                var user = await this.userManager.GetUserAsync(base.User);
                if (user != null)
                {
                    string customerSecretKey = await userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty;
                    string validationKey = this.LoginTwoFactorWithAuthenticator.SecurityCode;
                    if (twoFactor.ValidateTwoFactorPIN(customerSecretKey, validationKey))
                    {
                        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(this.LoginTwoFactorWithAuthenticator.SecurityCode, true, this.LoginTwoFactorWithAuthenticator.RememberMe);
                        if (result.Succeeded)
                        {
                            return RedirectToPage("/Index");
                        }
                        else if (result.IsLockedOut)
                        {
                            ModelState.AddModelError("Authenticator2FA", "Account locked out.");
                        }
                        else
                        {
                            ModelState.AddModelError("Authenticator2FA", "Invalid security code.");
                        }
                    }
                }
                #endregion Google TwoFactor Authenticator Code..
            }
            else
            {
                #region Default TwoFactor Authenticator Code..
                var result = await signInManager.TwoFactorAuthenticatorSignInAsync(this.LoginTwoFactorWithAuthenticator.SecurityCode, true, this.LoginTwoFactorWithAuthenticator.RememberMe);
                if (result.Succeeded)
                {
                    return RedirectToPage("/Index");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Authenticator2FA", "Account locked out.");
                }
                else
                {
                    ModelState.AddModelError("Authenticator2FA", "Invalid security code.");
                }
                #endregion Default TwoFactor Authenticator Code..
            }

            return Page();
        }
    }


    public class LoginTwoFactorWithAuthenticatorViewModel
    {
        public string Email { get; set; } = string.Empty;

        [Required]
        public string SecurityCode { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
