using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
    public class UserProfileModel : PageModel
    {
        private UserManager<User> userManager;

        [BindProperty]
        public UserProfileViewModel UserProfile { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        public UserProfileModel(UserManager<User> userManager)
        {
            this.userManager = userManager;
            this.UserProfile = new UserProfileViewModel
            {
                Email = string.Empty,
                Department = string.Empty,
                Position = string.Empty
            };
        }

        private async Task<(User? user, Claim? departmentClaim, Claim? positionClaim)> GetUserInfoAsync()
        {
            var user = await userManager.FindByNameAsync(User?.Identity?.Name ?? string.Empty);
            if (user != null)
            {
                var claims = await userManager.GetClaimsAsync(user);
                var departmentClaim = claims.FirstOrDefault(c => c.Type == "Department");
                var positionClaim = claims.FirstOrDefault(c => c.Type == "Position");
                
                return (user, departmentClaim, positionClaim);
            }
            return (null, null, null);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            SuccessMessage = string.Empty;
            var (user, departmentClaim, positionClaim) = await GetUserInfoAsync();
            if (user == null)
            {
                return NotFound("User not found.");
            }
            UserProfile.Email = User.Identity?.Name ?? string.Empty;
            UserProfile.Department = departmentClaim?.Value ?? string.Empty;
            UserProfile.Position = positionClaim?.Value ?? string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                var (user, departmentClaim, positionClaim) = await GetUserInfoAsync();
                if(user != null)
                {
                    if (departmentClaim != null)
                    {
                        await userManager.ReplaceClaimAsync(user, departmentClaim, new Claim(departmentClaim.Type, UserProfile.Department));
                        //await userManager.RemoveClaimAsync(user, departmentClaim);
                    }
                    else if (user != null && departmentClaim == null)
                    {
                        await userManager.AddClaimAsync(user, new Claim("Department", UserProfile.Department));
                    }
                    //await userManager.AddClaimAsync(user, new Claim("Department", UserProfile.Department));

                    if (positionClaim != null)
                    {
                        await userManager.ReplaceClaimAsync(user, positionClaim, new Claim(positionClaim.Type, UserProfile.Position));
                        //await userManager.RemoveClaimAsync(user, positionClaim);
                    }
                    else if (user != null && positionClaim == null)
                    {
                        await userManager.AddClaimAsync(user, new Claim("Position", UserProfile.Position));
                    }
                    //await userManager.AddClaimAsync(user, new Claim("Position", UserProfile.Position));

                }

                SuccessMessage = "Profile updated successfully.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while updating the profile: {ex.Message}");                
            }

            return Page();
        }
    }

    public class UserProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}
