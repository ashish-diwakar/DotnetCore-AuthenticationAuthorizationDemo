using Newtonsoft.Json;

namespace SecurityDemo.Authorization
{
    public class JwtToken
    {
        [JsonProperty("access_token")] 
        public string Token { get; set; } = string.Empty;

        [JsonProperty("expires_at")]
        public DateTime ExipresAt { get; set; }
    }
}
