using dl4_net6_api.Common;
using dl4_net6_api.Common.Configuration;

namespace dl4_net6_api
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureTciAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = new TciAuthorizationOptions()
            {
                BaseUrl = configuration.GetSection("TCI")["BaseUrl"],
                ApiClientKey = configuration.GetSection("TCI")["ApiClientKey"],
                ApiDealerId = configuration.GetSection("TCI")["ApiDealerId"],
                ApiKeyName = configuration.GetSection("TCI")["ApiKeyName"],
                ApiKeyValue = configuration.GetSection("TCI")["ApiKeyValue"],
                ClientId = configuration.GetSection("TCI")["ClientId"],
                UserId = configuration.GetSection("TCI")["UserId"]
            };

            services.Configure<TciAuthorizationOptions>(opts =>
            {
                opts.BaseUrl = options.BaseUrl;
                opts.ApiClientKey = options.ApiClientKey;
                opts.ApiDealerId = options.ApiDealerId;
                opts.ApiKeyName = options.ApiKeyName;
                opts.ApiKeyValue = options.ApiKeyValue;
                opts.ClientId = options.ClientId;
                opts.UserId = options.UserId;
            });

            services.AddTransient<AddDefaultHeadersHandler>();

            // Add named Client in case there are other Http Clients in the app.
            services.AddHttpClient(HttpClients.TciDecisionLenderClient, client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }).AddHttpMessageHandler<AddDefaultHeadersHandler>();
            return services;
        }
    }
}
