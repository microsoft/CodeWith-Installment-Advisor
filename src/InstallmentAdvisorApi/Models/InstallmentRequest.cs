using System;

namespace InstallmentAdvisorApi.Models
{
    public class InstallmentRequest
    {
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }
}
