using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class RssJson : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.RssJson> Datas { get; set; }
    }
}
