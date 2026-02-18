using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Security.Claims;

namespace Web_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private IConfiguration configuration;

        public AuthController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody] Credential credential)
        {
            if (credential.Username == "admin" && credential.Password == "password")
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, credential.Username) ,
                    new Claim(ClaimTypes.Email, "admin@mysite.com"),
                    new Claim(ClaimTypes.Role, "admin"),
                    new Claim("Department", "HR"),
                    new Claim("Admin", "true"),
                    new Claim("Manager", "true"),
                    new Claim("EmploymentDate", DateTime.UtcNow.AddMonths(-9).ToString("yyyy-MM-dd"))
                };
                var expiresAt = DateTime.UtcNow.AddMinutes(5);
                return Ok(new { 
                    access_token = CreateToken(claims, expiresAt), 
                    expires_at = expiresAt
                });

            }

            ModelState.AddModelError("Unauthorized", "You do not have access to this endpoint.");
            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Unauthorized",
                Detail = "You do not have access to this endpoint."
            };

            return Unauthorized(problemDetails);
        }

        // Fix for CS8604: Ensure configuration["Jwt:SecretKey"] is not null before using it.
        private string CreateToken(List<Claim> claims, DateTime expiresAt)
        {
            var claimsDic = new Dictionary<string, object>();
            if (claims is not null && claims.Count > 0)
            {
                foreach (var claim in claims)
                {
                    claimsDic.Add(claim.Type, claim.Value);
                }
            }

            var secretKey = configuration["SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT secret key is not configured.");
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
                    SecurityAlgorithms.HmacSha256Signature),
                Claims = claimsDic,
                Expires = expiresAt,
                NotBefore = DateTime.UtcNow,
            };
            var tokenHandler = new JsonWebTokenHandler();
            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }

    public class Credential
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
