using HackerNewsPoller.Dtos;

namespace HackerNewsPoller.Services
{
    public interface IBestStoriesService
    {
        Task<IReadOnlyList<BestStoryItem?>> GetBestStoriesAsync(int count, CancellationToken cancellationToken);
    }
}
