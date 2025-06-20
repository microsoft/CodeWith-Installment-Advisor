using InstallmentAdvisor.DataApi.Models;

namespace InstallmentAdvisor.DataApi.Models
{
    public class ContractResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public Contract Contract { get; set; } = new Contract();
    }
}
