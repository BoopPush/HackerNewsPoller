using HackerNewsPoller.Dtos;

namespace HackerNewsPoller.Clients
{
    public interface IHackerNewsClient
    {
        Task<int[]?> GetBestStoriesAsync (CancellationToken cancellationToken = default);
        Task<HackerNewsItem> GetStoryByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
