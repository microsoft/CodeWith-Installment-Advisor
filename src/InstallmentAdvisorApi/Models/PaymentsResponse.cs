using System.Collections.Generic;

namespace InstallmentAdvisorApi.Models
{
    public class PaymentsResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}
