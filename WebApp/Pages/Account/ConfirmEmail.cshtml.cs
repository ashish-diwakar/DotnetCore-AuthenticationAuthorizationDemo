using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class ConfirmEmailModel(Microsoft.AspNetCore.Identity.UserManager<User> userManager) : PageModel
    {
        private readonly Microsoft.AspNetCore.Identity.UserManager<User> userManager = userManager;

        [BindProperty]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            var user = await this.userManager.FindByIdAsync(userId);
            if (user == null)
            {
                this.StatusMessage = $"Failed to validate email.";
            }
            else
            {
                var result = await this.userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    this.StatusMessage = $"Thank you for confirming your email. You can now try to login.";
                }
                else
                {
                    this.StatusMessage = $"Failed to validate email.";
                }
            }
            return Page();
        }
    }
}
