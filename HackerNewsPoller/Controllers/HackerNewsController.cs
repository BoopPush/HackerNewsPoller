using HackerNewsPoller.Configuration;
using HackerNewsPoller.Dtos;
using HackerNewsPoller.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HackerNewsPoller.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HackerNewsController : ControllerBase
    {
        public IBestStoriesService storiesService_;
        public HackerNewsConfiguration configuration_;
        public HackerNewsController(IBestStoriesService storiesService, IOptions<AppConfiguration> appConfiguration)
        {
            storiesService_ = storiesService;
            configuration_ = appConfiguration.Value.HackerNews ?? new HackerNewsConfiguration();
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<BestStoryItem>>> GetBestStories([FromQuery] int number, CancellationToken cancellationToken)
        {
            if (number <= 0 || number > configuration_.MaxStoriesLimit)
            {
                return BadRequest($"Parameter n must be greater than 0 and less than {configuration_.MaxStoriesLimit}");
            }

            var result = await storiesService_.GetBestStoriesAsync(number, cancellationToken);

            return Ok(result);
        }
    }
}
