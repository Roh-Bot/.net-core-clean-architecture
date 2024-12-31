using System.Net.Http.Headers;

namespace Core.ServiceRecords
{
    public record HttpRequestRecord
    {
        public required Uri RequestUri { get; set; }
        public required HttpMethod Method { get; set; }
        public HttpRequestHeaders? Headers { get; set; }
        public object? Body { get; set; }
    }
}
