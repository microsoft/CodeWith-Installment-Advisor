namespace InstallmentAdvisorApi.Models
{
    public class InstallmentResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public Installment Installment { get; set; } = new Installment();
    }
}
