﻿using CodeHollow.FeedReader;
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
    public class RssCrawler
    {
        protected Rss Rss { get; set; }

        protected MongoDbUtil<FeedData> MongoDbFeedData;

        public delegate Task CrawlDataDelegate(FeedData data);

        public CrawlDataDelegate OnCrawlDataDelegate { get; set; }

        public RssCrawler(CrawlDataDelegate onCrawlDataDelegate, IMongoDatabase mongoDb, Rss rss)
        {
            if (mongoDb != null)
            {
                MongoDbFeedData = new MongoDbUtil<FeedData>(mongoDb);
            }

            OnCrawlDataDelegate = onCrawlDataDelegate;
            Rss = rss;
        }

        public virtual async Task<Rss> RunAsync()
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
                        link = atomFeedItem.Links.LastOrDefault().Href;
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
                        Log.Error($"Not found Link. <Item:{item}> <Url:{Rss.Url}>");
                    }

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
                if (!Rss.ErrorTime.HasValue)
                {
                    Rss.ErrorTime = DateTime.Now;
                }
                return Rss;
            }
            return null;
        }

        protected async Task OnCrawlData(FeedData feedData)
        {
            if (MongoDbFeedData == null)
            {
                return;
            }

            await MongoDbFeedData.UpsertAsync(Builders<FeedData>.Filter.Eq(x => x.Url, feedData.Url) &
                    Builders<FeedData>.Filter.Eq(x => x.Href, feedData.Href),
                    feedData,
                    async (feedData) =>
                    {
                        if (OnCrawlDataDelegate != null)
                        {
                            await OnCrawlDataDelegate.Invoke(feedData);
                        }
                    });
        }
    }
}
