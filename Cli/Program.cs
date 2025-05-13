using FeedCrawler.Crawler;
using MongoDB.Driver;
using Serilog;
using System.Text;
using System.Threading.Tasks;

namespace Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var client = new MongoClient("mongodb://localhost:27017/?maxPoolSize=200");
            var database = client.GetDatabase("cli-feed-crawler");


            await new RssCrawler(null, database, new FeedCrawler.Models.Rss
            {
                Url = "https://medium.com/feed/hgmin",
                Name = "테스트"
            }).RunAsync();

            await new RssCrawler(null, database, new FeedCrawler.Models.Rss
            {
                Url = "https://elky84.github.io/feed.xml",
                Name = "Elky Essay"
            }).RunAsync();
        }
    }
}