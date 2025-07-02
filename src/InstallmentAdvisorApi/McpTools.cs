using InstallmentAdvisor.DataApi.Models;
using InstallmentAdvisor.DataApi.Services;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace InstallmentAdvisor.DataApi;

[McpServerToolType]
public class McpTools(ICustomerService service)
{

    [McpServerTool(Name = "getCustomerEnergyConsumption"), Description("Get the usage history for the customer per month.")]
    public UsageResponse GetCustomerUsage(string customerId)
    {
        return service.GetUsage(customerId);
    }

    [McpServerTool(Name = "getCustomerEnergyPayments"), Description("Get the payment history for the customer, including dates and amounts.")]
    public PaymentsResponse GetCustomerPayments(string customerId)
    {
        return service.GetPayments(customerId);
    }

    [McpServerTool(Name = "getEndOfYearEstimate"), Description("Get the end-of-year estimate for the customer, including usage and payments.")]
    public EndOfYearEstimate GetEndOfYearEstimate(string customerId)
    {
        return service.GetEndOfYearEstimate(customerId);
    }

    [McpServerTool(Name = "getPriceSheet"), Description("Get the current and historical price sheet for the customer.")]
    public PriceSheetResponse GetPriceSheet(string customerId)
    {
        return service.GetPriceSheet(customerId);
    }

    [McpServerTool(Name = "getCustomerEnergyContract"), Description("Get the contract details for the customer, start and end dates and energy types supplied to the customer.")]
    public ContractResponse GetCustomerContract(string customerId)
    {
        return service.GetContract(customerId);
    }

    [McpServerTool(Name = "getCustomerEnergyInstallments"), Description("Get the installment history for the customer, including amount, currency, start date, frequency, and status.")]
    public List<Installment> GetCustomerInstallments(string customerId)
    {
        return service.GetInstallments(customerId);
    }

    [McpServerTool(Name = "saveEnergyInstallment"), Description("Save the installment amount for the customer.")]
    public InstallmentResponse SaveInstallment(string customerId, InstallmentRequest request)
    {
        return service.SaveInstallment(customerId, request);
    }

}
