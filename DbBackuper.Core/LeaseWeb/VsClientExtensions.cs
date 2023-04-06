using DbBackuper.Core.LeaseWeb.Contract;

namespace DbBackuper.Core.LeaseWeb;

public static class VsClientExtensions
{
    public static async Task<(bool Result, string FinalVpsState, string? LastError)> TryStopVpsAsync(this VirtualServerApiClient vsClient, string vpsId, int waitVpsStatusRetryCount, int waitVpsStatusRetryDelaySec)
    {
        bool result;
        string? lastError = null;
        var finalVpsState = "NONE";

        try
        {
            var vps = await vsClient.InspectVirtualServerAsync(vpsId);
            result = true;
            finalVpsState = vps.State;

            if (vps.State != Constants.StoppedState)
            {
                await vsClient.PowerOffVirtualServerAsync(vpsId);

                var retryCount = 0;
                while (vps.State != Constants.StoppedState)
                {
                    if (retryCount++ > waitVpsStatusRetryCount)
                    {
                        result = false;
                        lastError = "Exceeded retry limit";
                        break;
                    }

                    await Task.Delay(waitVpsStatusRetryDelaySec * 1000);
                    vps = await vsClient.InspectVirtualServerAsync(vpsId);
                    finalVpsState = vps.State;
                }
            }
        }
        catch (Exception ex)
        {
            result = false;
            lastError = ex.Message;
        }

        return (result, finalVpsState, lastError);
    }

    public static async Task<(bool Result, string InitialVpsState, string? LastError)> TryRunVpsAsync(this VirtualServerApiClient vsClient, string vpsId, int waitVpsStatusRetryCount, int waitVpsStatusRetryDelaySec)
    {
        bool result;
        var initialVpsState = "NONE";
        string? lastError = null;
        try
        {
            var vps = await vsClient.InspectVirtualServerAsync(vpsId);
            initialVpsState = vps.State;
            result = true;

            if (vps.State != Constants.RunningState)
            {
                await vsClient.PowerOnVirtualServerAsync(vpsId);

                var retryCount = 0;
                while (vps.State != Constants.RunningState)
                {
                    if (retryCount++ > waitVpsStatusRetryCount)
                    {
                        result = false;
                        lastError = "Exceeded retry limit";
                        break;
                    }

                    await Task.Delay(waitVpsStatusRetryDelaySec * 1000);
                    vps = await vsClient.InspectVirtualServerAsync(vpsId);
                }
            }
        }
        catch (Exception ex)
        {
            result = false;
            lastError = ex.Message;
        }

        return (result, initialVpsState, lastError);
    }
}