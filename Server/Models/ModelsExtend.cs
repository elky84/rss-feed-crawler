using EzAspDotNet.Models;
using EzAspDotNet.Notification.Models;

namespace Server.Models
{
    public static class ModelsExtend
    {
        public static T ToProtocol<T>(this T t, MongoDbHeader header)
            where T : Protocols.Common.Header
        {
            t.Id = header.Id;
            t.Created = header.Created;
            t.Updated = header.Updated;
            return t;
        }

        public static Protocols.Common.Notification ToProtocol(this Notification notification)
        {
            return new Protocols.Common.Notification
            {
                Type = notification.Type,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                SourceId = notification.SourceId,
                Keyword = notification.Keyword,
                FilterDayOfWeeks = notification.FilterDayOfWeeks,
                FilterStartTime = notification.FilterStartTime,
                FilterEndTime = notification.FilterEndTime
            }.ToProtocol(notification);
        }

        public static Notification ToModel(this Protocols.Common.Notification notification)
        {
            return new Notification
            {
                Id = notification.Id,
                Type = notification.Type,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                Keyword = notification.Keyword,
                FilterDayOfWeeks = notification.FilterDayOfWeeks,
                FilterStartTime = notification.FilterStartTime,
                FilterEndTime = notification.FilterEndTime
            };
        }

        public static Notification ToModel(this Protocols.Common.NotificationCreate notification)
        {
            return new Notification
            {
                Type = notification.Type,
                Name = notification.Name,
                HookUrl = notification.HookUrl,
                Channel = notification.Channel,
                IconUrl = notification.IconUrl,
                Keyword = notification.Keyword,
                FilterDayOfWeeks = notification.FilterDayOfWeeks,
                FilterStartTime = notification.FilterStartTime,
                FilterEndTime = notification.FilterEndTime
            };
        }

        public static Protocols.Common.Rss ToProtocol(this FeedCrawler.Models.Rss rss)
        {
            return new Protocols.Common.Rss
            {
                Url = rss.Url,
                Name = rss.Name,
                ErrorTime = rss.ErrorTime,
                Error = rss.Error
            }.ToProtocol(rss);
        }

        public static FeedCrawler.Models.Rss ToModel(this Protocols.Common.Rss rss)
        {
            return new FeedCrawler.Models.Rss
            {
                Id = rss.Id,
                Url = rss.Url,
                Name = rss.Name,
                ErrorTime = rss.ErrorTime,
                Error = rss.Error
            };
        }


        public static Protocols.Common.FeedData ToProtocol(this FeedCrawler.Models.FeedData feed)
        {
            return new Protocols.Common.FeedData
            {
                FeedTitle = feed.FeedTitle,
                Description = feed.Description,
                Href = feed.Href,
                DateTime = feed.DateTime,
                Url = feed.Url,
                ItemTitle = feed.ItemTitle,
                ItemAuthor = feed.ItemAuthor,
                ItemContent = feed.ItemContent
            }.ToProtocol(feed);
        }

        public static FeedCrawler.Models.FeedData ToModel(this Protocols.Common.FeedData feed)
        {
            return new FeedCrawler.Models.FeedData
            {
                Id = feed.Id,
                FeedTitle = feed.FeedTitle,
                Description = feed.Description,
                Href = feed.Href,
                DateTime = feed.DateTime,
                Url = feed.Url,
                ItemTitle = feed.ItemTitle,
                ItemAuthor = feed.ItemAuthor,
                ItemContent = feed.ItemContent
            };
        }

    }
}
