using InstallmentAdvisor.DataApi.Models;

namespace InstallmentAdvisor.DataApi.Models
{
    public class Installment
    {
        public double Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
