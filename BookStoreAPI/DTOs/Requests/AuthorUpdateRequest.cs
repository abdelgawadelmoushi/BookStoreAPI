using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace BookStoreAPI.DTOs.Requests
{
    public class AuthorUpdateRequest
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public IFormFile Img { get; set; }

        public List<int> CategoryIds { get; set; } = new List<int>();
        public List<int> BookIds { get; set; } = new List<int>();
        public List<string> Skills { get; set; } = new List<string>();
    }
}
