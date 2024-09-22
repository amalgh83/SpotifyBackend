using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Windows;

namespace SpotifyBackend.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {

        private readonly string clientId = ConfigurationManager.AppSettings["SpotifyClientId"];
        private readonly string clientSecret = ConfigurationManager.AppSettings["SpotifyClientSecret"];
        private readonly string redirectUri = ConfigurationManager.AppSettings["redirectUrl"]; 
        private readonly HttpClient _httpClient;
        public AuthController()
        {
            _httpClient = new HttpClient();
        }
      

        [HttpPost]
        [Route("callback")]
        public async Task<IHttpActionResult> Callback()
        {
            var content = await Request.Content.ReadAsStringAsync();

            // Assuming the content is JSON
            var jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(content);

            string code = jsonObject.code;
            string codeVerifier = jsonObject.code_verifier;
            var tokenResponse = await ExchangeCodeForToken(code, codeVerifier);
            //var token = Newtonsoft.Json.JsonConvert.SerializeObject<dynamic>(tokenResponse);
            return Ok(tokenResponse);
        }


        private async Task<string> ExchangeCodeForToken(string code, string codeVerifier)
        {
            
            using (var httpClient = new HttpClient())
            {
                var payload = new Dictionary<string, string>
                {
                    { "client_id", clientId },
                    { "grant_type", "authorization_code" },
                    { "code", code },
                    { "redirect_uri", redirectUri }, // Ensure this matches your registered redirect URI
                    { "code_verifier", codeVerifier }
                };

                var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(payload));

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    // Log the error response for debugging
                    Console.WriteLine("Error response from Spotify: " + errorResponse);
                    //return InternalServerError(new Exception("Failed to exchange code for token."));
                }

                var tokenData = await response.Content.ReadAsStringAsync();
                return tokenData.ToString(); // Return token data to the client
            }

        }
        [HttpGet]
        [Route("spotify/albums")]
        public async Task<IHttpActionResult> GetAlbums()
        {
            var accessToken = Request.Headers.Authorization.Parameter;
            var albums = await FetchAlbumsFromSpotify(accessToken);
            return Ok(albums);
        }

        private async Task<IHttpActionResult> FetchAlbumsFromSpotify(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/me/albums");

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Failed to fetch albums.");
            }

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}