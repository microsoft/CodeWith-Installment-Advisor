using System;
using System.Collections.Generic;

namespace InstallmentAdvisor.DataApi.Models
{
    public class UsageResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public Period Period { get; set; } = new Period();
        public List<UsageRecord> Usage { get; set; } = new List<UsageRecord>();
    }
}
