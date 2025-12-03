using HackerNewsPoller.Dtos;

namespace HackerNewsPoller.Mappers
{
    public static class HackerNewsMapper
    {
        public static BestStoryItem MapToBestStoryItem(this HackerNewsItem item)
        {
            var time = DateTimeOffset.FromUnixTimeSeconds(item.Time ?? new long());

            var result = new BestStoryItem()
            {
                Title = item.Title,
                Uri = item.Url,
                PostedBy = item.By,
                Time = time,
                Score = item.Score,
                CommentCount = item.Descendants
            };

            return result;
        }
    }
}
