using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class RssMulti : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.Rss> Datas { get; set; }
    }
}
