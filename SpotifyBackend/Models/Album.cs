using SpotifyBackend.Areas.HelpPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpotifyBackend.Models
{
    public class Album
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> artistName { get; set; }
        public List<Image> images { get; set; }
        public List<Song> songs { get; set; }
    }
}