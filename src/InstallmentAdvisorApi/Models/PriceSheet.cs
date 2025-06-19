namespace InstallmentAdvisor.DataApi.Models
{
    public class PriceSheet
    {
        public ElectricityPrice Electricity { get; set; } = new ElectricityPrice();
        public GasPrice Gas { get; set; } = new GasPrice();
    }
}
