using System;
using System.Collections.Generic;
using System.Text;

namespace GDUTSharp.Interfaces
{
    public interface ICommonClient
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
