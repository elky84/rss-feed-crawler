using EzAspDotNet.Exception;
using EzAspDotNet.Notification.Models;
using EzAspDotNet.Notification.Types;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using MongoDB.Driver;
using Server.Code;
using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;

        public NotificationService(MongoDbService mongoDbService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification>(mongoDbService.Database);
            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(x => x.SourceId)
                .Ascending(x => x.CrawlingType)
                .Ascending(x => x.Type)));
        }

        public async Task<List<Notification>> All()
        {
            return await _mongoDbNotification.All();
        }

        public async Task<Protocols.Response.Notification> Create(Protocols.Request.NotificationCreate notification)
        {
            var created = await Create(notification.Data);

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = created?.ToProtocol()
            };

        }

        private FilterDefinition<Notification> GetFilterDefinition(NotificationType notificationType)
        {
            return Builders<Notification>.Filter.Eq(x => x.Type, notificationType);
        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                var filter = GetFilterDefinition(notification.Type);
                return await _mongoDbNotification.UpsertAsync(filter, notification.ToModel());
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(ResultCode.UsingNotificationId);
            }
        }

        public async Task<Protocols.Response.NotificationMulti> CreateMulti(Protocols.Request.NotificationMulti notificationMulti)
        {
            var notifications = new List<Notification>();
            foreach (var notification in notificationMulti.Datas)
            {
                notifications.Add(await Create(notification));
            }

            return new Protocols.Response.NotificationMulti
            {
                Datas = notifications.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.Notification> Get(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (await _mongoDbNotification.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<List<Notification>> Get(FilterDefinition<Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task<Protocols.Response.Notification> Update(Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = notificationUpdate.Data.ToModel();

            var filter = GetFilterDefinition(update.Type);
            var updated = await _mongoDbNotification.UpsertAsync(filter, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Update(string id, Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = notificationUpdate.Data.ToModel();

            var updated = await _mongoDbNotification.UpdateAsync(id, update);
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = (await _mongoDbNotification.RemoveGetAsync(id))?.ToProtocol()
            };
        }
    }
}
