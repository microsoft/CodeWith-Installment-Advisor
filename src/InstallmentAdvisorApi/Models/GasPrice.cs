namespace InstallmentAdvisorApi.Models
{
    public class GasPrice
    {
        public double BasePricePerM3 { get; set; }
        public double TaxPerM3 { get; set; }
        public double VatPerM3 { get; set; }
        public double GridChargePerDay { get; set; }
    }
}
