using HackerNewsPoller.Clients;
using HackerNewsPoller.Configuration;
using HackerNewsPoller.Dtos;
using HackerNewsPoller.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace HackerNewsPoller.Tests
{
    public class BestStoriesServiceTests
    {
        private BestStoriesService CreateService(
             IHackerNewsClient? clientMock = null,
             IMemoryCache? memoryCache = null,
             AppConfiguration? appConfig = null)
        {
            var client = clientMock ?? Substitute.For<IHackerNewsClient>();
            var logger = Substitute.For<ILogger<BestStoriesService>>();
            var cache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());

            var config = appConfig ?? new AppConfiguration
            {
                HackerNews = new HackerNewsConfiguration
                {
                    BaseUrl = "https://hacker-news.firebaseio.com/v0",
                    BestStoriesEndpoint = "/beststories.json",
                    ItemEndpoint = "/item/{0}.json"
                },
                Cache = new CacheConfiguration
                {
                    BestStoriesDuration = 60,
                    ItemDuration = 120,
                }
            };

            var options = Options.Create(config);

            return new BestStoriesService(
                client,
                options,
                logger,
                cache
            );
        }


        [Fact]
        public async Task GetBestStoriesAsync_ReturnsEmpty_WhenCountIsLessOrEqualZero()
        {
            // Arrange
            var client = Substitute.For<IHackerNewsClient>();
            var service = CreateService(client);

            // Act
            var resultZero = await service.GetBestStoriesAsync(0, CancellationToken.None);
            var resultNegative = await service.GetBestStoriesAsync(-5, CancellationToken.None);

            // Assert
            Assert.Empty(resultZero);
            Assert.Empty(resultNegative);
            await client.DidNotReceiveWithAnyArgs().GetBestStoriesAsync(default);
        }

        [Fact]
        public async Task GetBestStoriesAsync_ReturnsStoriesSortedByScore()
        {
            // Arrange
            var client = Substitute.For<IHackerNewsClient>();

            client.GetBestStoriesAsync(Arg.Any<CancellationToken>())
                  .Returns(new[] { 1, 2, 3 });

            client.GetStoryByIdAsync(1, Arg.Any<CancellationToken>())
                  .Returns(new HackerNewsItem
                  {
                      Id = 1,
                      Title = "Story 1",
                      Score = 10,
                      Type = "story",
                      Descendants = 5,
                      By = "user1",
                      Time = 1,
                      Url = "http://story1"
                  });

            client.GetStoryByIdAsync(2, Arg.Any<CancellationToken>())
                  .Returns(new HackerNewsItem
                  {
                      Id = 2,
                      Title = "Story 2",
                      Score = 30,
                      Type = "story",
                      Descendants = 3,
                      By = "user2",
                      Time = 2,
                      Url = "http://story2"
                  });

            client.GetStoryByIdAsync(3, Arg.Any<CancellationToken>())
                  .Returns(new HackerNewsItem
                  {
                      Id = 3,
                      Title = "Story 3",
                      Score = 20,
                      Type = "story",
                      Descendants = 7,
                      By = "user3",
                      Time = 3,
                      Url = "http://story3"
                  });

            var service = CreateService(client);

            // Act
            var result = await service.GetBestStoriesAsync(3, CancellationToken.None);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Collection(
                result,
                s => Assert.Equal("Story 2", s!.Title),
                s => Assert.Equal("Story 3", s!.Title),
                s => Assert.Equal("Story 1", s!.Title)
            );
        }

        [Fact]
        public async Task GetBestStoriesAsync_RespectsAvailableItems_WhenLessThanRequested()
        {
            // Arrange
            var client = Substitute.For<IHackerNewsClient>();

            client.GetBestStoriesAsync(Arg.Any<CancellationToken>())
                  .Returns([1, 2]);

            client.GetStoryByIdAsync(1, Arg.Any<CancellationToken>())
                  .Returns(new HackerNewsItem
                  {
                      Id = 1,
                      Title = "Story 1",
                      Score = 10,
                      Type = "story",
                      Descendants = 0,
                      By = "user1",
                      Time = 1,
                      Url = "http://story1"
                  });

            client.GetStoryByIdAsync(2, Arg.Any<CancellationToken>())
                  .Returns((HackerNewsItem?)null);

            var service = CreateService(client);

            // Act
            var result = await service.GetBestStoriesAsync(10, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Story 1", result[0]!.Title);
        }
    }
}
