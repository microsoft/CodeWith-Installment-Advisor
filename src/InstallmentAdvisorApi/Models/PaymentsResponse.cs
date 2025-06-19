using System.Collections.Generic;

namespace InstallmentAdvisor.DataApi.Models
{
    public class PaymentsResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}
