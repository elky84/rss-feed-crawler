namespace Server.Protocols.Response
{
    public class Rss : EzAspDotNet.Protocols.ResponseHeader
    {
        public Common.Rss Data { get; set; }
    }
}
