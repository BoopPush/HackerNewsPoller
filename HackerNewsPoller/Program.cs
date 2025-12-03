
using HackerNewsPoller.Clients;
using HackerNewsPoller.Configuration;
using HackerNewsPoller.Services;

namespace HackerNewsPoller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMemoryCache();

            builder.Services
                .AddOptions<AppConfiguration>()
                .Bind(builder.Configuration.GetSection("AppConfiguration"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddHttpClient<IHackerNewsClient, HackerNewsClient>();

            builder.Services
                .AddSingleton<IBestStoriesService, BestStoriesService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
