namespace DbBackuper.Core.LeaseWeb.Contract;

public static class Constants
{
    public static readonly string AuthHeader = "X-LSW-Auth";

    public static readonly string StartingState = "STARTING";
    public static readonly string RunningState = "RUNNING";
    public static readonly string StoppingState = "STOPPING";
    public static readonly string StoppedState = "STOPPED";
}