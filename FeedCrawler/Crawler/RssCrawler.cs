using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using EzMongoDb.Util;
using FeedCrawler.Models;
using MongoDB.Driver;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FeedCrawler.Crawler
{
    public sealed class RssCrawler
    {
        private Rss Rss { get; set; }

        private readonly MongoDbUtil<FeedData> _mongoDbFeedData;

        public delegate Task CrawlDataDelegate(FeedData data);

        private CrawlDataDelegate OnCrawlDataDelegate { get; set; }

        public RssCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Rss rss)
        {
            if (mongoDb != null)
            {
                _mongoDbFeedData = new MongoDbUtil<FeedData>(mongoDb);
            }

            OnCrawlDataDelegate = onCrawlDataDelegate;
            Rss = rss;
        }
        
        public static string EnsureHttpsUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }

            return url;
        }

        public async Task<Rss> RunAsync()
        {
            try
            {
                var feed = await FeedReader.ReadAsync(Rss.Url);
                foreach (var item in feed.Items)
                {
                    var link = item.Link;
                    if (item.SpecificItem is AtomFeedItem)
                    {
                        var atomFeedItem = item.SpecificItem as AtomFeedItem;
                        link = atomFeedItem?.Links.LastOrDefault()?.Href;
                    }

                    if (link != null)
                    {
                        link = link.StartsWith("http") ? link : feed.Link + link;
                    }
                    else if (Uri.IsWellFormedUriString(item.Id, UriKind.Absolute))
                    {
                        link = item.Id;
                    }
                    else
                    {
                        Log.Error("Not found Link. <Item:{FeedItem}> <Url:{RssUrl}>", item, Rss.Url);
                    }

                    link = EnsureHttpsUrl(link);

                    var feedData = new FeedData
                    {
                        Url = Rss.Url,
                        Description = feed.Description,
                        ItemTitle = item.Title,
                        ItemAuthor = item.Author,
                        ItemContent = item.Content,
                        FeedTitle = feed.Title,
                        Href = link,
                        DateTime = item.PublishingDate.GetValueOrDefault(DateTime.Now)
                    };

                    await OnCrawlData(feedData);
                }

                if (!string.IsNullOrEmpty(Rss.Error))
                {
                    Rss.ErrorTime = null;
                    Rss.Error = string.Empty;
                    return Rss;
                }
            }
            catch (Exception e)
            {
                Rss.Error = e.Message;
                Rss.ErrorTime ??= DateTime.Now;
                return Rss;
            }
            return null;
        }

        private async Task OnCrawlData(FeedData feedData)
        {
            if (_mongoDbFeedData == null)
            {
                return;
            }
            
            await _mongoDbFeedData.UpsertAsync(Builders<FeedData>.Filter.Eq(x => x.Url, feedData.Url) &
                    Builders<FeedData>.Filter.Eq(x => x.Href, feedData.Href),
                    feedData,
                    async void (data) =>
                    {
                        try
                        {
                            if (OnCrawlDataDelegate != null)
                            {
                                await OnCrawlDataDelegate.Invoke(data);
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, $"Title:{data.ItemTitle} Href:{feedData.Href}");
                        }
                    });
        }
    }
}
