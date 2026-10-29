using EzMongoDb.Models;
using System;

namespace FeedCrawler.Models
{
    public class WebHookHistory : MongoDbHeader
    {
        public string Title { get; set; }

        public string Footer { get; set; }

        public string TitleLink { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime Sent { get; set; }

        public string SiteType { get; set; }

        public string BoardId { get; set; }
    }
}
