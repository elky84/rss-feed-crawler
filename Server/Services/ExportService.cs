using EzAspDotNet.Services;
using EzMongoDb.Util;
using FeedCrawler.Models;
using MongoDB.Driver;
using Server.Protocols.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server.Services
{
    public class ExportService
    {
        private readonly MongoDbUtil<FeedData> _mongoFeedData;

        private readonly IMongoCollection<WebHookHistory> _mongoWebHookHistory;

        public ExportService(MongoDbService mongoDbService)
        {
            _mongoFeedData = new MongoDbUtil<FeedData>(mongoDbService.Database);
            _mongoWebHookHistory = mongoDbService.Database.GetCollection<WebHookHistory>(nameof(WebHookHistory));
        }

        public async Task<List<FeedData>> Rss(ExportRss request)
        {
            var b = Builders<FeedData>.Filter;
            var filter = b.Gte(x => x.DateTime, request.From) & b.Lte(x => x.DateTime, request.To);
            if (!string.IsNullOrEmpty(request.SiteType))
            {
                filter &= b.Eq(x => x.SiteType, request.SiteType);
            }

            if (!string.IsNullOrEmpty(request.BoardId))
            {
                filter &= b.Eq(x => x.BoardId, request.BoardId);
            }

            return await _mongoFeedData.Collection.Find(filter)
                .SortByDescending(x => x.DateTime).Limit(request.Limit).ToListAsync();
        }

        public async Task<List<WebHookHistory>> WebHook(ExportWebHook request)
        {
            var b = Builders<WebHookHistory>.Filter;
            var filter = b.Gte(x => x.DateTime, request.From) & b.Lte(x => x.DateTime, request.To);
            if (!string.IsNullOrEmpty(request.SiteType))
            {
                filter &= b.Eq(x => x.SiteType, request.SiteType);
            }

            if (!string.IsNullOrEmpty(request.BoardId))
            {
                filter &= b.Eq(x => x.BoardId, request.BoardId);
            }

            return await _mongoWebHookHistory.Find(filter)
                .SortByDescending(x => x.DateTime).Limit(request.Limit).ToListAsync();
        }

        public static string ToRssXml(string title, string description, IEnumerable<FeedData> items)
        {
            return Build(title, description, items.Select(x =>
                (x.ItemTitle, x.Href, x.ItemContent ?? x.Description, x.ItemAuthor, x.DateTime, x.SiteType)));
        }

        public static string ToRssXml(string title, string description, IEnumerable<WebHookHistory> items)
        {
            return Build(title, description, items.Select(x =>
                (x.Title, x.TitleLink, (string)null, x.Footer, x.DateTime, x.SiteType)));
        }

        private static string Build(string title, string description,
            IEnumerable<(string Title, string Link, string Content, string Author, DateTime DateTime, string Category)> items)
        {
            var channel = new XElement("channel",
                new XElement("title", title),
                new XElement("description", description),
                new XElement("lastBuildDate", DateTime.Now.ToString("r")));

            foreach (var i in items)
            {
                var item = new XElement("item",
                    new XElement("title", i.Title),
                    new XElement("link", i.Link),
                    new XElement("guid", i.Link),
                    new XElement("pubDate", i.DateTime.ToString("r")));
                if (!string.IsNullOrEmpty(i.Content)) item.Add(new XElement("description", i.Content));
                if (!string.IsNullOrEmpty(i.Author)) item.Add(new XElement("author", i.Author));
                if (!string.IsNullOrEmpty(i.Category)) item.Add(new XElement("category", i.Category));
                channel.Add(item);
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement("rss", new XAttribute("version", "2.0"), channel));
            return doc.Declaration + Environment.NewLine + doc.Root;
        }
    }
}
