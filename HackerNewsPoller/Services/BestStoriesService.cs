using HackerNewsPoller.Clients;
using HackerNewsPoller.Configuration;
using HackerNewsPoller.Dtos;
using HackerNewsPoller.Mappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Threading;

namespace HackerNewsPoller.Services
{
    public class BestStoriesService : IBestStoriesService
    {
        private readonly IHackerNewsClient client_;
        private readonly HackerNewsConfiguration configuration_;
        private readonly ILogger<BestStoriesService> logger_;
        private readonly IMemoryCache cache_;
        private readonly SemaphoreSlim semaphore_;

        private readonly TimeSpan BestStoriesDuration;
        private readonly TimeSpan ItemDuration;

        //todo move to config
        private readonly string DefaultCacheKey;

        public BestStoriesService(
            IHackerNewsClient client, 
            IOptions<AppConfiguration> configuration, 
            ILogger<BestStoriesService> logger, 
            IMemoryCache cache)
        {
            client_ = client;
            configuration_ = configuration.Value.HackerNews ?? new HackerNewsConfiguration();
            logger_ = logger;
            cache_ = cache;
            semaphore_ = new SemaphoreSlim(configuration_.MaxRequestsLimit, configuration_.MaxRequestsLimit);

            BestStoriesDuration = TimeSpan.FromSeconds(configuration.Value.Cache?.BestStoriesDuration ?? 60);
            ItemDuration = TimeSpan.FromSeconds(configuration.Value.Cache?.ItemDuration ?? 120);
            DefaultCacheKey = configuration.Value.Cache?.DefaultCacheKey ?? "best_stories_ids";
        }

        public async Task<IReadOnlyList<BestStoryItem?>> GetBestStoriesAsync(int number, CancellationToken cancellationToken)
        {
            if (number <= 0 || number > configuration_.MaxStoriesLimit)
            {
                return Array.Empty<BestStoryItem>();
            }

            var storyIds = await client_.GetBestStoriesAsync(cancellationToken);

            var ids = storyIds?.Take(number).ToList();

            var tasks = new List<Task<BestStoryItem?>>();

            if (configuration_.AllowConcurrency)
            {
                tasks = ids?.Select(id => ExecuteWithConcurrencyLimit(() => GetItemAsync(id, cancellationToken), cancellationToken)).ToList();
            }
            else
            {
                tasks = ids?.Select(id => GetItemAsync(id, cancellationToken)).ToList();
            }

            var tasksResult = await Task.WhenAll(tasks!);

            return tasksResult
                .Where(i  => i != null)
                .OrderByDescending(i => i!.Score)
                .ToList();
        }

        private async Task<int[]> GetBestStoriesIdsAsync(CancellationToken cancellationToken)
        {
            var cacheStories = cache_.Get<int[]>(DefaultCacheKey);

            if (cacheStories != null)
            {
                return cacheStories;
            }

            var updatedStories = await client_.GetBestStoriesAsync(cancellationToken);

            if (updatedStories?.Length > 0)
            {
                cache_.Set(DefaultCacheKey, updatedStories, BestStoriesDuration);
            }

            return updatedStories ?? Array.Empty<int>();
        }

        private async Task<BestStoryItem?> GetItemAsync(int id, CancellationToken cancellationToken)
        {
            var cacheKey = $"item_{id}";
            var cacheItem = cache_.Get<HackerNewsItem>(cacheKey);

            if (cacheItem != null) 
            {
                return cacheItem.MapToBestStoryItem();
            }

            var updatedItem = await client_.GetStoryByIdAsync(id, cancellationToken);

            if (updatedItem is { Type: "story"})
            {
                cache_.Set(cacheKey, updatedItem, ItemDuration);
            }

            return updatedItem?.MapToBestStoryItem();
        }

        private async Task<T> ExecuteWithConcurrencyLimit<T>(Func<Task<T>> action, CancellationToken ct)
        {
            await semaphore_.WaitAsync(ct);

            try
            {
                return await action();
            }
            finally
            {
                semaphore_.Release();
            }
        }

    }
}
