using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Encodings.Web;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    [Authorize]
    public class AuthenticatorMFASetupModel : PageModel
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> userManager;
        private readonly IMyAppService myAppService;
        private bool useGoogleAuthenticatorCode = false; // I've used this variable to test default code for Authenticator authorization & Google's Nuget package for same
        private UrlEncoder _urlEncoder;

        [BindProperty]
        public AuthenticatorMFASetupViewModel authenticatorMFASetupViewModel { get; set; } = new AuthenticatorMFASetupViewModel();

        public AuthenticatorMFASetupModel(UserManager<User> userManager, IMyAppService myAppService, UrlEncoder urlEncoder)
        {
            this.userManager = userManager;
            this.myAppService = myAppService;
            this._urlEncoder = urlEncoder;
            this.authenticatorMFASetupViewModel.IsSucceed = false;
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode(myAppService.GetMyApplicationName()?? "ASP.NET Core Identity"), //_urlEncoder.Encode("ASP.NET Core Identity"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        public async Task OnGetAsync()
        {
            var user = await this.userManager.GetUserAsync(base.User);
            if (user != null)
            {
                var key = await userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrEmpty(key))
                {
                    await userManager.ResetAuthenticatorKeyAsync(user);
                    key = await userManager.GetAuthenticatorKeyAsync(user);
                }
                this.authenticatorMFASetupViewModel.SharedKey = key ?? string.Empty;


                if (useGoogleAuthenticatorCode)
                {
                    #region Google TwoFactor Authenticator Code..
                    var twoFactor = new TwoFactorAuthenticator();

                    // We need a unique PER USER key to identify this Setup
                    // must be saved: you need this value later to verify a validation code
                    var customerSecretKey = this.authenticatorMFASetupViewModel.SharedKey; // Guid.NewGuid.ToString();
                    string applicationName = this.myAppService.GetMyApplicationName();

                    var setupInfo = twoFactor.GenerateSetupCode(
                        // name of the application - the name shown in the Authenticator
                        applicationName,
                        // an account identifier - shouldn't have spaces
                        user.Email,
                        // the secret key that also is used to validate later
                        customerSecretKey,
                        // Base32 Encoding (odd this was left in)
                        false,
                        // resolution for the QR Code - larger number means bigger image
                        10);

                    // a string key
                    var TwoFactorSetupKey = setupInfo.ManualEntryKey;

                    // a base64 formatted string that can be directly assigned to an img src
                    this.authenticatorMFASetupViewModel.QrCodeImageData = setupInfo.QrCodeSetupImageUrl;
                    #endregion Google TwoFactor Authenticator Code..
                }
                else
                {
                    #region Default TwoFactor Authenticator Code..

                    this.authenticatorMFASetupViewModel.QrCodeImageData = GenerateQrCodeUri(user.Email ?? string.Empty, this.authenticatorMFASetupViewModel.SharedKey);

                    #endregion Default TwoFactor Authenticator Code..
                }
            }
        }

        public async Task<PageResult> OnPostAsync()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            var user = await this.userManager.GetUserAsync(base.User);
            if (user != null)
            {

                if (useGoogleAuthenticatorCode)
                {
                    #region Google TwoFactor Authenticator Code..

                    var customerSecretKey = this.authenticatorMFASetupViewModel.SharedKey;
                    string validationKey = authenticatorMFASetupViewModel.SecurityCode;
                    var twoFactor = new TwoFactorAuthenticator();
                    if (twoFactor.ValidateTwoFactorPIN(customerSecretKey, validationKey))
                    {
                        await userManager.SetTwoFactorEnabledAsync(user, true);
                        this.authenticatorMFASetupViewModel.IsSucceed = true;
                        return Page();
                    }
                    else
                        ModelState.AddModelError("AuthenticatorSetup", "Invalid Validation code.");

                    #endregion Google TwoFactor Authenticator Code..
                }
                else
                {
                    #region Default TwoFactor Authenticator Code..

                    if(await userManager.VerifyTwoFactorTokenAsync(
                        user, 
                        userManager.Options.Tokens.AuthenticatorTokenProvider, 
                        this.authenticatorMFASetupViewModel.SecurityCode))
                    {
                        await userManager.SetTwoFactorEnabledAsync(user, true);
                        this.authenticatorMFASetupViewModel.IsSucceed = true;
                    }
                    else
                    {
                        ModelState.AddModelError("AuthenticatorSetup", "Something went wrong during Authenticator setup.");
                        this.authenticatorMFASetupViewModel.IsSucceed = false;
                    }

                    #endregion Default TwoFactor Authenticator Code..
                }
            }
            return Page();
        }
    }

    public class AuthenticatorMFASetupViewModel
    {
        public string? SharedKey { get; set; } = string.Empty;

        [Required]
        public string SecurityCode { get; set; } = string.Empty;

        public bool IsSucceed { get; set; }

        public string? QrCodeImageData { get; set; }
    }
}
