namespace InstallmentAdvisor.DataApi.Models
{
    public class PriceSheetResponse
    {
        public string CustomerId { get; set; } = string.Empty;
        public PriceSheet Pricesheet { get; set; } = new PriceSheet();
        public string Currency { get; set; } = string.Empty;
    }
}
