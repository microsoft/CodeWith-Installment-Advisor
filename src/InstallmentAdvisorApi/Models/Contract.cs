using System;
using System.Collections.Generic;

namespace InstallmentAdvisorApi.Models
{
    public class Contract
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = string.Empty;
        public List<string> EnergyTypes { get; set; } = new List<string>();
        public string Status { get; set; } = string.Empty;
    }
}
