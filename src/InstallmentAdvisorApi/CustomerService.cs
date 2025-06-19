using InstallmentAdvisorApi.Models;
using InstallmentAdvisorApi.Repositories;

namespace InstallmentAdvisorApi.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public UsageResponse GetUsage(string customerId) => _repository.GetUsage(customerId);
    public PaymentsResponse GetPayments(string customerId) => _repository.GetPayments(customerId);
    public EndOfYearEstimate GetEndOfYearEstimate(string customerId) => _repository.GetEndOfYearEstimate(customerId);
    public PriceSheetResponse GetPriceSheet(string customerId) => _repository.GetPriceSheet(customerId);
    public ContractResponse GetContract(string customerId) => _repository.GetContract(customerId);
    public List<Installment> GetInstallments(string customerId) => _repository.GetInstallments(customerId);
    public InstallmentResponse SaveInstallment(string customerId, InstallmentRequest request) => _repository.SaveInstallment(customerId, request);
}
