namespace HackerNewsPoller.Configuration
{
    public class CacheConfiguration
    {
        //value in seconds
        public double BestStoriesDuration = 60;

        //value in seconds
        public double ItemDuration = 120;

        public string DefaultCacheKey = "best_stories_ids";
    }
}
