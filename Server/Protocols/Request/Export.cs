using System;

namespace Server.Protocols.Request
{
    public class ExportRss
    {
        public DateTime From { get; set; } = DateTime.Now.AddDays(-7);

        public DateTime To { get; set; } = DateTime.Now;

        public string SiteType { get; set; }

        public string BoardId { get; set; }

        public int Limit { get; set; } = 1000;

        public bool Download { get; set; }
    }

    public class ExportWebHook : ExportRss
    {
    }
}
