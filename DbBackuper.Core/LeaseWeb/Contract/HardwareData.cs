namespace DbBackuper.Core.LeaseWeb.Contract;

public record HardwareData(CpuData? Cpu, StorageData? Memory, StorageData? Storage, StorageData? AdditionalStorage);