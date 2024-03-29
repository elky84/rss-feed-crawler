﻿using EzAspDotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RssController : ControllerBase
    {
        private readonly ILogger<RssController> _logger;

        private readonly RssService _rssService;

        public RssController(ILogger<RssController> logger, RssService rssService)
        {
            _logger = logger;
            _rssService = rssService;
        }



        [HttpGet]
        public async Task<Protocols.Response.RssMulti> All()
        {
            return new Protocols.Response.RssMulti
            {
                Datas = MapperUtil.Map<List<FeedCrawler.Models.Rss>,
                                       List<Protocols.Common.Rss>>
                                       (await _rssService.All())
            };
        }

        [HttpGet("Json")]
        public async Task<Protocols.Response.RssJson> Json(string category)
        {
            return new Protocols.Response.RssJson
            {
                Datas = MapperUtil.Map<List<FeedCrawler.Models.Rss>,
                                       List<Protocols.Common.RssJson>>
                                       (await _rssService.Category(category))
            };
        }

        [HttpGet("Json/All")]
        public async Task<Protocols.Response.RssJson> JsonAll()
        {
            return new Protocols.Response.RssJson
            {
                Datas = MapperUtil.Map<List<FeedCrawler.Models.Rss>,
                                       List<Protocols.Common.RssJson>>
                                       (await _rssService.All())
            };
        }

        [HttpGet("Error")]
        public async Task<Protocols.Response.RssMulti> Error()
        {
            return new Protocols.Response.RssMulti
            {
                Datas = MapperUtil.Map<List<FeedCrawler.Models.Rss>,
                                       List<Protocols.Common.Rss>>
                                       (await _rssService.Error())
            };
        }


        [HttpPost]
        public async Task<Protocols.Response.Rss> Create([FromBody] Protocols.Request.Rss rss)
        {
            return await _rssService.Create(rss);
        }

        [HttpPost("Multi")]
        public async Task<Protocols.Response.RssMulti> CreateMulti([FromBody] Protocols.Request.RssMulti rssMulti)
        {
            return await _rssService.CreateMulti(rssMulti);
        }


        [HttpGet("{id}")]
        public async Task<Protocols.Response.Rss> Get(string id)
        {
            return await _rssService.GetById(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.Rss> Update(string id, [FromBody] Protocols.Request.Rss rss)
        {
            return await _rssService.Update(id, rss);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.Rss> Delete(string id)
        {
            return await _rssService.Delete(id);
        }
    }
}
