using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SecurityDemo.Authorization;
using SecurityDemo.DTO;
using SecurityDemo.Pages.Account;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecurityDemo.Pages
{
    [Authorize(Policy = "HRManagerOnly")]
    public class HrManagementModel : PageModel
    {
        private IHttpClientFactory httpClientFactory;

        [BindProperty]
        public List<WeatherForcastDTO>? weatherForcastItems { get; set; }

        public HrManagementModel(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task OnGetAsync()
        {
            var token = new JwtToken();
            string? strTokenObj = HttpContext.Session.GetString("access_token");
            if (!string.IsNullOrEmpty(strTokenObj))
            {
                token = Newtonsoft.Json.JsonConvert.DeserializeObject<JwtToken>(strTokenObj) ?? new JwtToken();
            }
            else
            {
                token = await GetJwtTokenAsync();
            }
            if (token == null || string.IsNullOrEmpty(token.Token) || token.ExipresAt <= DateTime.UtcNow)
            {
                token = await GetJwtTokenAsync();
            }
            var client = httpClientFactory.CreateClient("OurWebAPI");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token?.Token ?? string.Empty);
            weatherForcastItems = await client.GetFromJsonAsync<List<WeatherForcastDTO>>("WeatherForecast");
        }

        private async Task<JwtToken?> GetJwtTokenAsync()
        {
            var client = httpClientFactory.CreateClient("OurWebAPI");
            var credential = new Credential { Username = "admin", Password = "password" };
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(credential), System.Text.Encoding.UTF8, "application/json");
            var res = await client.PostAsync("auth", content);
            res.EnsureSuccessStatusCode();
            string strJwt = await res.Content.ReadAsStringAsync();
            HttpContext.Session.SetString("access_token", strJwt);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JwtToken>(strJwt);
        }
    }
}
