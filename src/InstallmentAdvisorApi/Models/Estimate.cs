namespace InstallmentAdvisor.DataApi.Models
{
    public class Estimate
    {
        public EstimateDetail Electricity { get; set; } = new EstimateDetail();
        public EstimateDetail Gas { get; set; } = new EstimateDetail();
        public double Total { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
