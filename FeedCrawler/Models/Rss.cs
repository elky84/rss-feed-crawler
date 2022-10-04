using EzMongoDb.Models;
using System;

namespace FeedCrawler.Models
{
    public class Rss : MongoDbHeader
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public DateTime? ErrorTime { get; set; }

        public string Error { get; set; }
    }
}
