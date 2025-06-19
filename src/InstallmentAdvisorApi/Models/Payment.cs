using System;

namespace InstallmentAdvisorApi.Models
{
    public class Payment
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
