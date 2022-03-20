namespace Server.Protocols.Response
{
    public class Pagable : EzAspDotNet.Protocols.ResponseHeader
    {
        public long Total { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public string Sort { get; set; }

        public bool Asc { get; set; }
    }
}
