using HackerNewsPoller.Configuration;
using HackerNewsPoller.Dtos;
using Microsoft.Extensions.Options;


namespace HackerNewsPoller.Clients
{
    public class HackerNewsClient : IHackerNewsClient
    {
        private readonly HttpClient client_;
        private readonly HackerNewsConfiguration configuration_;
        private readonly ILogger<HackerNewsClient> logger_;

        private const string LogPrefix = "HackerNewsClient";
        public HackerNewsClient(HttpClient client, IOptions<AppConfiguration> configuration, ILogger<HackerNewsClient> logger)
        {
            client_ = client;
            configuration_ = configuration.Value.HackerNews ?? new HackerNewsConfiguration();
            logger_ = logger;
        }

        public async Task<int[]?> GetBestStoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = configuration_.BestStoriesUrl;
                var ids = await client_.GetFromJsonAsync<int[]>(requestUrl, cancellationToken);

                return ids ?? Array.Empty<int>();
            }
            catch (Exception ex)
            {
                logger_.LogError(ex, $"{LogPrefix} Error pulling best stories due to: {ex.Message}");
                
                return Array.Empty<int>();
            }
        }

        public async Task<HackerNewsItem> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl = configuration_.GetStoryUrl(id);
                var story = await client_.GetFromJsonAsync<HackerNewsItem>(requestUrl, cancellationToken);

                return story ?? new HackerNewsItem();
            }
            catch (Exception ex)
            {
                logger_.LogError(ex, $"{LogPrefix} Error getting story by id due to: {ex.Message}");

                return new HackerNewsItem();
            }
        }
    }
}
