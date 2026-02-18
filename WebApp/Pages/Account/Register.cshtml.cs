using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Account;
using WebApp.Services;

namespace WebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IEmailService emailService;

        public RegisterModel(Microsoft.AspNetCore.Identity.UserManager<User> userManager, IEmailService emailService)
        {
            UserManager = userManager;
            this.emailService = emailService;
        }

        [BindProperty]
        public RegisterViewModel _RegisterViewModel { get; set; } = new RegisterViewModel
        {
            Email = string.Empty,
            Password = string.Empty,
            ConfirmPassword = string.Empty
        };
        public UserManager<User> UserManager { get; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate Email address (for unique email address), will be done later
            //if (!new EmailAddressAttribute().IsValid(_RegisterViewModel.Email))
            //{
            //    ModelState.AddModelError("Email", "Invalid email address!");
            //    return Page();
            //}

            // This code is related to extending IdentityUser Class to User Class with added columns: Department & Position
            /*
            var user = new User
            {
                UserName = _RegisterViewModel.Email,
                Email = _RegisterViewModel.Email,
                Department = _RegisterViewModel.Department,
                Position = _RegisterViewModel.Position
            };
            */

            // This code is related to using Claims for additional information: Department & Position - Part 1
            var user = new User
            {
                UserName = _RegisterViewModel.Email,
                Email = _RegisterViewModel.Email
            };
            var claim_Department = new Claim("Department", _RegisterViewModel.Department);
            var claim_Position = new Claim("Position", _RegisterViewModel.Position);



            var result = await this.UserManager.CreateAsync(user, _RegisterViewModel.Password);

            if (result.Succeeded)
            {
                // This code is related to using Claims for additional information: Department & Position - Part 2
                await this.UserManager.AddClaimAsync(user, claim_Department);
                await this.UserManager.AddClaimAsync(user, claim_Position);

                var confirmationToken = await this.UserManager.GenerateEmailConfirmationTokenAsync(user);

                // Commented to avoid sending email during development, you can uncomment it when you have an email service set up.
                /*
                await emailService.SendEmailAsync(user.Email, "Please confirm your email", $"Please confirm your email by clicking <a href='{Url.PageLink("/Account/ConfirmEmail", values: new { userId = user.Id, token = confirmationToken })}'>here</a>.");
                return RedirectToPage("/Account/Login");
                */

                // For development purposes, we will just redirect with the confirmation URL by default.
                return Redirect(Url.PageLink("/Account/ConfirmEmail", values: new { userId = user.Id, token = confirmationToken }) ?? "");

            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
        }


        public class RegisterViewModel
        {
            [Required]
            [EmailAddress(ErrorMessage = "Invalid email address!")]
            public required string Email { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public required string Password { get; set; }
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public required string ConfirmPassword { get; set; }

            [Required]
            public string Department { get; set; } = string.Empty;

            [Required]
            public string Position { get; set; } = string.Empty;
        }
    }
}
