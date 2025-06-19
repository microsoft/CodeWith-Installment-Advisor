namespace InstallmentAdvisor.DataApi.Models
{
    public class EndOfYearEstimate
    {
        public string CustomerId { get; set; } = string.Empty;
        public int Year { get; set; }
        public Estimate Estimate { get; set; } = new Estimate();
    }
}
