namespace DbBackuper.Core.LeaseWeb.Contract;

public record VirtualServerData(string Id, string Name, string Reference, int CustomerId, string SalesOrgId,
    string Platform, string DataCenter, string VirtualNetworkId, string State, string Template,
    string FirewallState, string ServiceOffering, string Sla, string ControlPanel,
    ContractData? Contract, HardwareData? Hardware, IsoData? Iso, List<IpData> Ips, NetworkTrafficData? NetworkTraffic);