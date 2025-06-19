using System;

namespace InstallmentAdvisorApi.Models
{
    public class UsageRecord
    {
        public string Type { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
