namespace HackerNewsPoller.Configuration
{
    public class AppConfiguration
    {
        public CacheConfiguration? Cache { get; set; }

        public HackerNewsConfiguration? HackerNews { get; set; }
    }
}
