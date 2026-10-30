using EzAspDotNet.Services;
using EzMongoDb.Util;
using FeedCrawler.Models;
using MongoDB.Driver;
using Server.Protocols.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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

        // Kind 가 Unspecified 인 입력은 서버 로컬 시간으로 간주해 저장값(UTC)과 같은 기준으로 맞춘다
        private static DateTime ToUtc(DateTime value) =>
            value.Kind == DateTimeKind.Utc ? value : TimeZoneInfo.ConvertTimeToUtc(
                value.Kind == DateTimeKind.Local ? value : DateTime.SpecifyKind(value, DateTimeKind.Local));

        public async Task<List<FeedData>> Rss(ExportRss request)
        {
            var b = Builders<FeedData>.Filter;
            var filter = b.Gte(x => x.DateTime, ToUtc(request.From)) & b.Lte(x => x.DateTime, ToUtc(request.To));
            if (!string.IsNullOrEmpty(request.SiteType))
            {
                filter &= b.Eq(x => x.SiteType, request.SiteType);
            }

            if (!string.IsNullOrEmpty(request.BoardId))
            {
                filter &= b.Eq(x => x.BoardId, request.BoardId);
            }

            return await _mongoFeedData.Collection.Find(filter)
                .SortByDescending(x => x.DateTime).Limit(request.Limit > 0 ? request.Limit : null).ToListAsync();
        }

        public async Task<List<WebHookHistory>> WebHook(ExportWebHook request)
        {
            var b = Builders<WebHookHistory>.Filter;
            var filter = b.Gte(x => x.DateTime, ToUtc(request.From)) & b.Lte(x => x.DateTime, ToUtc(request.To));
            if (!string.IsNullOrEmpty(request.SiteType))
            {
                filter &= b.Eq(x => x.SiteType, request.SiteType);
            }

            if (!string.IsNullOrEmpty(request.BoardId))
            {
                filter &= b.Eq(x => x.BoardId, request.BoardId);
            }

            return await _mongoWebHookHistory.Find(filter)
                .SortByDescending(x => x.DateTime).Limit(request.Limit > 0 ? request.Limit : null).ToListAsync();
        }

        public static string ToRssXml(string title, string description, IEnumerable<FeedData> items)
        {
            return Build(title, description, items.Select(x =>
            {
                var content = ToPlainText(x.ItemContent);
                return (TitleOf(x), x.Href, Truncate(content, ContentMaxLength), x.ItemAuthor, x.DateTime, x.SiteType);
            }));
        }

        public static string TitleOf(FeedData x)
        {
            if (IsValidTitle(x.ItemTitle)) return x.ItemTitle.Trim();

            var content = ToPlainText(x.ItemContent);
            if (!string.IsNullOrEmpty(content)) return Truncate(content, TitleMaxLength);

            return !string.IsNullOrWhiteSpace(x.FeedTitle) ? x.FeedTitle.Trim() : x.Href;
        }

        public static string TitleOf(WebHookHistory x) => CleanHistoryTitle(x.Title, x.TitleLink);

        public static string ToRssXml(string title, string description, IEnumerable<WebHookHistory> items)
        {
            return Build(title, description, items.Select(x =>
                (TitleOf(x), x.TitleLink, (string)null, x.Footer, x.DateTime, x.SiteType)));
        }

        private const int TitleMaxLength = 80;
        private const int ContentMaxLength = 300;

        private static bool IsValidTitle(string title) =>
            !string.IsNullOrWhiteSpace(title) && !title.Trim().Equals("No Title", StringComparison.OrdinalIgnoreCase);

        // 저장된 Title 은 "{ItemTitle} - {FeedTitle}" 형태라 앞쪽 "No Title - " 을 제거한다
        private static string CleanHistoryTitle(string title, string link)
        {
            if (string.IsNullOrWhiteSpace(title)) return link;

            title = title.Trim();
            const string prefix = "No Title";
            if (title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                title = title.Substring(prefix.Length).TrimStart(' ', '-').Trim();
            }

            return string.IsNullOrEmpty(title) ? link : title;
        }

        private static string ToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return null;

            var text = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", " "));
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length == 0 ? null : text;
        }

        private static string Truncate(string text, int max) =>
            string.IsNullOrEmpty(text) || text.Length <= max ? text : text.Substring(0, max).TrimEnd() + "…";

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
                    new XElement("pubDate", i.DateTime.ToUniversalTime().ToString("r")));
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
