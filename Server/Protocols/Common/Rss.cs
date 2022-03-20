using System;

namespace Server.Protocols.Common
{
    public class Rss : Header
    {
        public string Url { get; set; }

        public string Name { get; set; }

        public DateTime? ErrorTime { get; set; }

        public string Error { get; set; }
    }
}
