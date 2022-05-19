using EzAspDotNet.Models;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using FeedCrawler.Crawler;
using FeedCrawler.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Services
{
    public class CrawlingService
    {
        private readonly MongoDbService _mongoDbService;

        private readonly RssService _rssService;

        private readonly WebHookService _webHookService;

        private readonly MongoDbUtil<FeedData> _mongoFeedData;

        public CrawlingService(MongoDbService mongoDbService,
            RssService rssService,
            WebHookService webHookService)
        {
            _mongoDbService = mongoDbService;
            _rssService = rssService;
            _webHookService = webHookService;
            _mongoFeedData = new MongoDbUtil<FeedData>(mongoDbService.Database);

            _mongoFeedData.Collection.Indexes.CreateMany(new List<CreateIndexModel<FeedData>>
            {
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.DateTime)),
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.ItemTitle)),
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.FeedTitle)),
                new CreateIndexModel<FeedData>(Builders<FeedData>.IndexKeys.Ascending(x => x.Url).Ascending(x => x.Href))
            });
        }

        public async Task<Protocols.Response.FeedList> Get(Protocols.Request.FeedList feedList)
        {
            var builder = Builders<FeedData>.Filter;
            var filter = FilterDefinition<FeedData>.Empty;
            if (!string.IsNullOrEmpty(feedList.Keyword))
            {
                filter &= builder.Regex(x => x.FeedTitle, "^" + feedList.Keyword + ".*");
            }

            return new Protocols.Response.FeedList
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Limit = feedList.Limit,
                Offset = feedList.Offset,
                Sort = feedList.Sort,
                Asc = feedList.Asc,
                Datas = MapperUtil.Map<List<FeedData>, List<Protocols.Common.FeedData>>(await _mongoFeedData.Page(filter, feedList.Limit, feedList.Offset, feedList.Sort, feedList.Asc)),
                Total = await _mongoFeedData.CountAsync(filter)
            };
        }

        public async Task<Protocols.Response.Feed> Execute(Protocols.Request.Feed feed)
        {
            var onCrawlDataDelegate = new RssCrawler.CrawlDataDelegate(OnNewCrawlData);
            var rssList = feed.All ?
                MapperUtil.Map<List<Rss>, List<Protocols.Common.Rss>>(await _rssService.All()) :
                feed.RssList;

            Parallel.ForEach(rssList, new ParallelOptions { MaxDegreeOfParallelism = 16 },
                async rss =>
                {
                    var update = await new RssCrawler(onCrawlDataDelegate, _mongoDbService.Database, MapperUtil.Map<Rss>(rss)).RunAsync();
                    if (update != null)
                    {
                        await _rssService.Update(update);
                    }
                }
            );

            return new Protocols.Response.Feed
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success
            };
        }

        public async Task OnNewCrawlData(FeedData feedData)
        {
            if (DateTime.Now.Subtract(feedData.DateTime).TotalDays > 7)
            {
                return;
            }

            await _webHookService.Execute(Builders<Notification>.Filter.Empty,
                new EzAspDotNet.Notification.Data.WebHook
                {
                    Title = $"{feedData.ItemTitle} - {feedData.FeedTitle}",
                    Footer = feedData.ItemAuthor,
                    TitleLink = feedData.Href,
                    TimeStamp = feedData.DateTime.ToTimeStamp()
                });
        }
    }
}
