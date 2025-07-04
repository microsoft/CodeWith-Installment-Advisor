using System;

namespace InstallmentAdvisor.DataApi.Models
{
    public class InstallmentRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }
}
