using System.Net;
using dl4_net6_api.Common.Configuration;
using Microsoft.Extensions.Options;

namespace dl4_net6_api.Common
{
    /// <summary>
    /// Delegate Handler for adding TCI-relevant headers to each request of named HttpClient.
    /// </summary>
    public class AddDefaultHeadersHandler : DelegatingHandler
    {
        private readonly IOptions<TciAuthorizationOptions> _options;

        public AddDefaultHeadersHandler(IOptions<TciAuthorizationOptions> options)
        {
            _options = options;
        }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiKeyClient = new ApiKeyClient(_options.Value.BaseUrl, _options.Value.ClientId, _options.Value.ApiClientKey,
                _options.Value.ApiDealerId, _options.Value.UserId, _options.Value.ApiKeyName,
                _options.Value.ApiKeyValue);
            
            apiKeyClient.AddAuthHeaderToRequest(request);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
