using System.Diagnostics;
using Flurl.Http;
using Flurl.Http.Configuration;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace DbBackuper.Core.LeaseWeb;

public static class Policies
{
    public static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy
    {
        get
        {
            return Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<TimeoutRejectedException>()
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15)
                    },
                    (delegateResult, retryCount) =>
                    {
                        Debug.WriteLine($"[App|Policy]: Retry delegate fired, attempt {retryCount}");
                    });
        }
    }

    public static void ConfigureRetryPolicy()
    {
        FlurlHttp.Configure(settings => settings.HttpClientFactory = new PollyHttpClientFactory());
    }
}

public class PollyHttpClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler()
    {
        return new PolicyHandler
        {
            InnerHandler = base.CreateMessageHandler()
        };
    }
}

public class PolicyHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await Policies.RetryPolicy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}