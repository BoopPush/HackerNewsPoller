using System.ComponentModel.DataAnnotations;

namespace HackerNewsPoller.Configuration
{
    public class HackerNewsConfiguration
    {
        [Required]
        public string? BaseUrl { get; set; }

        [Required]
        public string? BestStoriesEndpoint { get; set; }

        [Required]
        public string? ItemEndpoint { get; set; }

        public int? MaxStoriesLimit { get; set; } = 200;

        public int MaxRequestsLimit { get; set; } = 20;

        public bool AllowConcurrency { get; set; } = true;

        public string GetStoryUrl(int id)
        {
            return $"{BaseUrl}{string.Format(ItemEndpoint!, id)}";
        }

        public string BestStoriesUrl => $"{BaseUrl}{BestStoriesEndpoint}";
    }

}
