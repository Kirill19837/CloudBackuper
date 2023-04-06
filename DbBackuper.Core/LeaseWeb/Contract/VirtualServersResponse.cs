namespace DbBackuper.Core.LeaseWeb.Contract;

public record VirtualServersResponse(MetaData? _metaData, List<VirtualServerData> VirtualServers);