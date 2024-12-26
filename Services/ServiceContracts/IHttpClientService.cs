using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ServiceContracts
{
    public interface IHttpClientService
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken);

    }
}
