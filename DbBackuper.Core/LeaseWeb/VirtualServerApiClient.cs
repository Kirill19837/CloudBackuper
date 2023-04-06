using DbBackuper.Core.LeaseWeb.Contract;
using Flurl;
using Flurl.Http;

namespace DbBackuper.Core.LeaseWeb;

public class VirtualServerApiClient
{
    private readonly string _apiKey;
    private readonly string _apiRoot;

    public VirtualServerApiClient(string apiKey, string apiRoot)
    {
        _apiKey = apiKey;
        _apiRoot = apiRoot;
    }

    public async Task<VirtualServersResponse> ListVirtualServersAsync()
    {
        //https://api.leaseweb.com/cloud/v2/virtualServers
        var url = _apiRoot
            .AppendPathSegment("virtualServers")
            .WithHeader(Constants.AuthHeader, _apiKey);

        try
        {
            var response = await url.GetJsonAsync<VirtualServersResponse>();
            return response;
        }
        catch(FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<ErrorDetails>();
            throw new ApiException(error?.ErrorMessage ?? ex.Message, ex, error);
        }
    }

    //https://api.leaseweb.com/cloud/v2/virtualServers/{virtualServerId}
    public async Task<VirtualServerData> InspectVirtualServerAsync(string vpsId)
    {
        //https://api.leaseweb.com/cloud/v2/virtualServers
        var url = _apiRoot
            .AppendPathSegment("virtualServers")
            .AppendPathSegment(vpsId)
            .WithHeader(Constants.AuthHeader, _apiKey);

        try
        {
            var response = await url.GetJsonAsync<VirtualServerData>();
            return response;
        }
        catch (FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<ErrorDetails>();
            throw new ApiException(error?.ErrorMessage ?? ex.Message, ex, error);
        }
    }


    //https://api.leaseweb.com/cloud/v2/virtualServers/{virtualServerId}/powerOn
    public async Task<VirtualServerCommandResponse> PowerOnVirtualServerAsync(string vpsId)
    {
        //https://api.leaseweb.com/cloud/v2/virtualServers
        var url = _apiRoot
            .AppendPathSegment("virtualServers")
            .AppendPathSegment(vpsId)
            .AppendPathSegment("powerOn")
            .WithHeader(Constants.AuthHeader, _apiKey);

        try
        {
            var response = await url.PostAsync().ReceiveJson<VirtualServerCommandResponse>(); ;
            return response;
        }
        catch (FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<ErrorDetails>();
            throw new ApiException(error?.ErrorMessage ?? ex.Message, ex, error);
        }
    }

    //https://api.leaseweb.com/cloud/v2/virtualServers/{virtualServerId}/powerOff
    public async Task<VirtualServerCommandResponse> PowerOffVirtualServerAsync(string vpsId)
    {
        //https://api.leaseweb.com/cloud/v2/virtualServers
        var url = _apiRoot
            .AppendPathSegment("virtualServers")
            .AppendPathSegment(vpsId)
            .AppendPathSegment("powerOff")
            .WithHeader(Constants.AuthHeader, _apiKey);

        try
        {
            var response = await url.PostAsync().ReceiveJson<VirtualServerCommandResponse>(); ;
            return response;
        }
        catch (FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<ErrorDetails>();
            throw new ApiException(error?.ErrorMessage ?? ex.Message, ex, error);
        }
    }

    //https://api.leaseweb.com/cloud/v2/virtualServers/{virtualServerId}/reboot
    public async Task<VirtualServerCommandResponse> RebootVirtualServerAsync(string vpsId)
    {
        //https://api.leaseweb.com/cloud/v2/virtualServers
        var url = _apiRoot
            .AppendPathSegment("virtualServers")
            .AppendPathSegment(vpsId)
            .AppendPathSegment("reboot")
            .WithHeader(Constants.AuthHeader, _apiKey);

        try
        {
            var response = await url.PostAsync().ReceiveJson<VirtualServerCommandResponse>(); ;
            return response;
        }
        catch (FlurlHttpException ex)
        {
            var error = await ex.GetResponseJsonAsync<ErrorDetails>();
            throw new ApiException(error?.ErrorMessage ?? ex.Message, ex, error);
        }
    }

    public record VirtualServerCommandResponse(string Id, string Name, string Status, DateTime CreatedAt);
}