using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Core.ServiceRecords;

namespace Core.ServiceContracts
{
    public interface IHttpClientService
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestRecord request, CancellationToken cancellationToken);

    }
}
