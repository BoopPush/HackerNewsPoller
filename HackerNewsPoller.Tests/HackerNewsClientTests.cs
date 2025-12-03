using System.Net;
using System.Text;
using HackerNewsPoller.Clients;           
using HackerNewsPoller.Configuration;      
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace HackerNewsPoller.Tests
{
    public class HackerNewsClientTests
    {
        private HackerNewsClient CreateClient(string responseJson, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new TestHttpMessageHandler
            {
                Handler = (_, _) =>
                {
                    var message = new HttpResponseMessage(statusCode)
                    {
                        Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                    };
                    return Task.FromResult(message);
                }
            };

            var httpClient = new HttpClient(handler);

            var config = new AppConfiguration
            {
                HackerNews = new HackerNewsConfiguration
                {
                    BaseUrl = "https://example.com",
                    BestStoriesEndpoint = "/beststories.json",
                    ItemEndpoint = "/item/{0}.json"
                }
            };

            var options = Options.Create(config);
            var logger = Substitute.For<ILogger<HackerNewsClient>>();

            return new HackerNewsClient(httpClient, options, logger);
        }

        [Fact]
        public async Task GetBestStoriesAsync_Returns_Ids_From_Api()
        {
            var json = "[1,2,3]";
            var client = CreateClient(json);

            var result = await client.GetBestStoriesAsync(CancellationToken.None);

            Assert.Equal([1, 2, 3], result);
        }

        [Fact]
        public async Task GetBestStoriesAsync_Returns_Empty_On_Exception()
        {
            var handler = new TestHttpMessageHandler
            {
                Handler = (_, _) => throw new HttpRequestException("boom")
            };

            var httpClient = new HttpClient(handler);

            var config = new AppConfiguration
            {
                HackerNews = new HackerNewsConfiguration
                {
                    BaseUrl = "https://example.com",
                    BestStoriesEndpoint = "/beststories.json",
                    ItemEndpoint = "/item/{0}.json"
                }
            };

            var options = Options.Create(config);
            var logger = Substitute.For<ILogger<HackerNewsClient>>();

            var client = new HackerNewsClient(httpClient, options, logger);

            var result = await client.GetBestStoriesAsync(CancellationToken.None);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetStoryByIdAsync_Returns_Item_From_Api()
        {
            var json = @"{
                ""id"": 1,
                ""by"": ""user1"",
                ""score"": 42,
                ""time"": 1700000000,
                ""title"": ""Story title"",
                ""url"": ""https://example.com"",
                ""descendants"": 7,
                ""type"": ""story""
            }";

            var client = CreateClient(json);

            var result = await client.GetStoryByIdAsync(1, CancellationToken.None);

            Assert.Equal(1, result.Id);
            Assert.Equal("user1", result.By);
            Assert.Equal(42, result.Score);
            Assert.Equal("Story title", result.Title);
            Assert.Equal("https://example.com", result.Url);
        }

        [Fact]
        public async Task GetStoryByIdAsync_Returns_New_Item_On_Exception()
        {
            var handler = new TestHttpMessageHandler
            {
                Handler = (_, _) => throw new HttpRequestException("boom")
            };

            var httpClient = new HttpClient(handler);

            var config = new AppConfiguration
            {
                HackerNews = new HackerNewsConfiguration
                {
                    BaseUrl = "https://example.com",
                    BestStoriesEndpoint = "/beststories.json",
                    ItemEndpoint = "/item/{0}.json"
                }
            };

            var options = Options.Create(config);
            var logger = Substitute.For<ILogger<HackerNewsClient>>();

            var client = new HackerNewsClient(httpClient, options, logger);

            var result = await client.GetStoryByIdAsync(1, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Null(result.Id);
        }
    }

    internal class TestHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Handler { get; set; }
            = (_, _) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Handler(request, cancellationToken);
        }
    }
}
