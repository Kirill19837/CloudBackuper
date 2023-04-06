namespace DbBackuper.Core.LeaseWeb.Contract;

public record ContractData(string Id, DateTime? StartsAt, DateTime? EndsAt, string ContractTerm, int BillingCycle,
    string BillingFrequency, string PricePerFrequency, string Currency, string BurstEnabled, bool InModification);