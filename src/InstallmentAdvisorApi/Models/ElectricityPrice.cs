namespace InstallmentAdvisor.DataApi.Models
{
    public class ElectricityPrice
    {
        public double BasePricePerKwh { get; set; }
        public double TaxPerKwh { get; set; }
        public double VatPerKwh { get; set; }
        public double GridChargePerDay { get; set; }
        public double TaxDiscountPerDay { get; set; }
    }
}
