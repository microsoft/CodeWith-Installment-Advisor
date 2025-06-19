using InstallmentAdvisor.DataApi.Models;

namespace InstallmentAdvisor.DataApi.Models
{
    public class EstimateDetail
    {
        public double Usage { get; set; }
        public string Unit { get; set; } = string.Empty;
        public double TotalCost { get; set; }
    }
}
