using HackerNewsPoller.Mappers;
using HackerNewsPoller.Dtos;

namespace HackerNewsPoller.Tests
{
    public class HackerNewsMapperTests
    {
        [Fact]
        public void MapToBestStoryItem_Maps_All_Fields()
        {
            var unixTime = 1_700_000_000L;

            var source = new HackerNewsItem
            {
                Title = "Story title",
                Url = "https://example.com",
                By = "author",
                Time = unixTime,
                Score = 123,
                Descendants = 42
            };

            var result = source.MapToBestStoryItem();

            Assert.Equal(source.Title, result.Title);
            Assert.Equal(source.Url, result.Uri);
            Assert.Equal(source.By, result.PostedBy);
            Assert.Equal(source.Score, result.Score);
            Assert.Equal(source.Descendants, result.CommentCount);
            Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(unixTime), result.Time);
        }

        [Fact]
        public void MapToBestStoryItem_Uses_Epoch_When_Time_Is_Null()
        {
            var source = new HackerNewsItem
            {
                Title = "No time",
                Url = "https://example.com",
                By = "author",
                Time = null,
                Score = 1,
                Descendants = 0
            };

            var result = source.MapToBestStoryItem();

            var expected = DateTimeOffset.FromUnixTimeSeconds(0);
            Assert.Equal(expected, result.Time);
        }
    }
}
