using Microsoft.AspNetCore.Mvc;
using Server.Services;
using System.Text;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportController(ExportService exportService) : ControllerBase
    {
        [HttpGet("Rss")]
        public async Task<IActionResult> Rss([FromQuery] Protocols.Request.ExportRss request)
        {
            var xml = ExportService.ToRssXml("Feed", $"{request.From:s} ~ {request.To:s}", await exportService.Rss(request));
            return Xml(xml, request.Download);
        }

        [HttpGet("RssShort")]
        public async Task<IActionResult> RssShort([FromQuery] Protocols.Request.ExportWebHook request)
        {
            var xml = ExportService.ToRssXml("RSS Short", $"{request.From:s} ~ {request.To:s}", await exportService.WebHook(request));
            return Xml(xml, request.Download);
        }

        private IActionResult Xml(string xml, bool download)
        {
            var bytes = Encoding.UTF8.GetBytes(xml);
            return download
                ? File(bytes, "application/rss+xml; charset=utf-8", "rss.xml")
                : Content(xml, "application/rss+xml; charset=utf-8", Encoding.UTF8);
        }
    }
}
