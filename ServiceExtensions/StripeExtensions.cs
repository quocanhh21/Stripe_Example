using Stripe;

namespace Stripe_Example.ServiceExtensions
{
    public static class StripeExtensions
    {
        public static IServiceCollection AddStripe(this IServiceCollection services, IConfiguration config)
    {
        StripeConfiguration.ApiKey = config["SecretKey"];
        Console.WriteLine(string.Format("StripeConfiguration.ApiKey: {0}------", 
        StripeConfiguration.ApiKey));
        services.Configure<StripeOptions>(config);

        var appInfo = new AppInfo
        {
            Name = "StripeEvents",
            Version = "0.1.0"
        };
        StripeConfiguration.AppInfo = appInfo;

        services.AddHttpClient("Stripe");
        services.AddTransient<IStripeClient, StripeClient>(s =>
        {
            var clientFactory = s.GetRequiredService<IHttpClientFactory>();
            var httpClient = new SystemNetHttpClient(
               httpClient: clientFactory.CreateClient("Stripe"),
               maxNetworkRetries: StripeConfiguration.MaxNetworkRetries,
               appInfo: appInfo,
               enableTelemetry: StripeConfiguration.EnableTelemetry);

            return new StripeClient(apiKey: StripeConfiguration.ApiKey, httpClient: httpClient);
        });

        return services;
    }
    }
}