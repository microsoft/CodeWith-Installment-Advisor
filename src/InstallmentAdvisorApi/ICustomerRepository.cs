using InstallmentAdvisorApi.Models;

namespace InstallmentAdvisorApi.Repositories;

public interface ICustomerRepository
{
    UsageResponse GetUsage(string customerId);
    PaymentsResponse GetPayments(string customerId);
    EndOfYearEstimate GetEndOfYearEstimate(string customerId);
    PriceSheetResponse GetPriceSheet(string customerId);
    ContractResponse GetContract(string customerId);
    List<Installment> GetInstallments(string customerId);
    InstallmentResponse SaveInstallment(string customerId, InstallmentRequest request);
}
