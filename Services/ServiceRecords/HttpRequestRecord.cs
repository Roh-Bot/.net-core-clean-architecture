using System.Net.Http.Headers;

namespace Core.ServiceRecords
{
    public record HttpRequestRecord
    {
        public required Uri RequestUri { get; init; }
        public required HttpMethod Method { get; init; }
        public HttpRequestHeaders Headers { get; init; } = new HttpRequestMessage().Headers;
        public object? Body { get; init; }
    }
}
