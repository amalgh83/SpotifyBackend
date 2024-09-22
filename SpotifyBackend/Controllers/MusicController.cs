using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using SpotifyBackend.Models;
using System.Web.Http;
using Newtonsoft.Json;

namespace SpotifyBackend.Controllers
{
    [RoutePrefix("api/music")]
    public class MusicController : ApiController
    {

        private readonly HttpClient _httpClient;
        public MusicController()
        {
            _httpClient = new HttpClient();
        }
        // GET: Music

        [HttpGet]
        [Route("albums")]
        public async Task<IHttpActionResult> FetchAlbumsFromSpotify()
        {
            List<Album> albums = new List<Album>();
            var authHeader = HttpContext.Current.Request.Headers["Authorization"];

            var albumsList = new List<Album>();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                var requestUrl = "https://api.spotify.com/v1/me/albums"; // Change this to the appropriate endpoint
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Add the Authorization header with the Bearer token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);

                // Try accessing the 'items' field
                var items = jsonObject["items"];
                List<Image> images = null;
                // Check if 'items' exists and is not null
                if (items != null)
                {
                    foreach (var item in items)
                    {

                        images = new List<Image>();
                        var album = item["album"];
                        var id = album["id"]?.ToString();
                        var name = album["name"]?.ToString();
                        List<string> artists = album["artists"].Select(a => a["name"]?.ToString()).ToList();
                        foreach(var img in album["images"])
                        {
                            var image = new Image();
                            image.url = img["url"].ToString();
                            image.height = Int64.Parse(img["height"].ToString());
                            image.width = Int64.Parse(img["width"].ToString());
                            images.Add(image);
                        }

                        foreach(var track in album["tracks"])
                        {

                        }

                        albumsList.Add(new Album
                        {
                            Id = id,
                            Name = name,
                            artistName = artists,
                            images = images 
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No items found in the response");
                }
            }
            return Ok(albumsList);
        }

        [HttpGet]
        [Route("album/{id}/songs")]
        public async Task<IHttpActionResult> FetchSongsByAlbumId(string id)
        {
            List<Song> songs = new List<Song>();
            var authHeader = HttpContext.Current.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                var requestUrl = "https://api.spotify.com/v1/albums/" + id + "/tracks"; // Change this to the appropriate endpoint
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Add the Authorization header with the Bearer token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);

                // Try accessing the 'items' field
                var items = jsonObject["items"];
                Song song = null;
                // Check if 'items' exists and is not null
                if (items != null)
                {
                    foreach (var item in items)
                    {

                        song = new Song();

                        song.Id = item["id"]?.ToString();
                        song.Name = item["name"]?.ToString();
                        song.Uri = item["href"]?.ToString();
                        songs.Add(song);
                        
                    }
                }
                else
                {
                    Console.WriteLine("No items found in the response");
                }
            }
            return Ok(songs);
        }
    }
}