using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using EzMongoDb.Util;
using FeedCrawler.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Services
{
    public class RssService
    {
        private readonly MongoDbUtil<Rss> _mongoDbRss;

        public RssService(MongoDbService mongoDbService)
        {
            _mongoDbRss = new MongoDbUtil<Rss>(mongoDbService.Database);

            _mongoDbRss.Collection.Indexes.CreateOne(new CreateIndexModel<Rss>(
                Builders<Rss>.IndexKeys.Ascending(x => x.Url)));
        }

        public async Task<List<Rss>> All()
        {
            return await _mongoDbRss.All();
        }

        public async Task<List<Rss>> Category(string category)
        {
            return await _mongoDbRss.FindAsync(Builders<Rss>.Filter.Eq(x => x.Category, category));
        }

        public async Task<List<Rss>> Error()
        {
            return await _mongoDbRss.FindAsync(Builders<Rss>.Filter.Ne(x => x.ErrorTime, null));
        }

        public async Task<Protocols.Response.Rss> Create(Protocols.Request.Rss rss)
        {
            var created = await Create(rss.Data);
            if (created == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Rss>(created)
            };

        }

        private async Task<Rss> Create(Protocols.Common.Rss rss)
        {
            try
            {
                rss.Created = DateTime.Now;
                return await _mongoDbRss.UpsertAsync(Builders<Rss>.Filter.Eq(x => x.Url, rss.Url),
                    MapperUtil.Map<Rss>(rss));
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingRssId);
            }
        }

        public async Task<Protocols.Response.RssMulti> CreateMulti(Protocols.Request.RssMulti rssMulti)
        {
            var rsses = new List<Rss>();
            foreach (var rss in rssMulti.Datas)
            {
                rsses.Add(await Create(rss));
            }

            return new Protocols.Response.RssMulti
            {
                Datas = MapperUtil.Map<List<Rss>,
                                       List<Protocols.Common.Rss>>
                                       (rsses)
            };
        }

        public async Task<Protocols.Response.Rss> GetById(string id)
        {
            var rss = await _mongoDbRss.FindOneAsyncById(id);
            if (rss == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Rss>(rss)
            };
        }

        public async Task<Rss> Get(string url)
        {
            return await _mongoDbRss.FindOneAsync(Builders<Rss>.Filter.Eq(x => x.Url, url));
        }

        public async Task<Protocols.Response.Rss> Update(string id, Protocols.Request.Rss rss)
        {
            var update = MapperUtil.Map<Rss>(rss);

            var updated = await _mongoDbRss.UpdateAsync(id, update);
            if (updated == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Rss>(updated)
            };
        }


        public async Task<Rss> Update(Rss rss)
        {
            return await _mongoDbRss.UpdateAsync(rss.Id, rss);
        }

        public async Task<Protocols.Response.Rss> Delete(string id)
        {
            var deleted = await _mongoDbRss.RemoveGetAsync(id);
            if (deleted == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }


            return new Protocols.Response.Rss
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Rss>(deleted)
            };
        }
    }
}
