using InstallmentAdvisor.DataApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace InstallmentAdvisor.DataApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _dataPath;
    public CustomerRepository(string dataPath)
    {
        _dataPath = dataPath;
    }

    public UsageResponse GetUsage(string customerId)
    {
        var file = Path.Combine(_dataPath, "usage.csv");
        var lines = File.ReadAllLines(file).Skip(1);
        var usage = new List<UsageRecord>();
        DateTime? from = null, to = null;
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts[0] != customerId) continue;
            var recFrom = DateTime.Parse(parts[1]);
            var recTo = DateTime.Parse(parts[2]);
            if (from == null || recFrom < from) from = recFrom;
            if (to == null || recTo > to) to = recTo;
            usage.Add(new UsageRecord
            {
                Type = parts[3],
                Amount = double.Parse(parts[4], CultureInfo.InvariantCulture),
                Unit = parts[5]
            });
        }
        return new UsageResponse
        {
            CustomerId = customerId,
            Period = new Period { From = from ?? DateTime.MinValue, To = to ?? DateTime.MinValue },
            Usage = usage
        };
    }

    public PaymentsResponse GetPayments(string customerId)
    {
        var file = Path.Combine(_dataPath, "payments.csv");
        var lines = File.ReadAllLines(file).Skip(1);
        var payments = new List<Payment>();
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts[0] != customerId) continue;
            payments.Add(new Payment
            {
                Date = DateTime.Parse(parts[1]),
                Amount = double.Parse(parts[2], CultureInfo.InvariantCulture),
                Currency = parts[3],
                Type = parts[4]
            });
        }
        return new PaymentsResponse
        {
            CustomerId = customerId,
            Payments = payments
        };
    }

    public EndOfYearEstimate GetEndOfYearEstimate(string customerId)
    {
        var file = Path.Combine(_dataPath, "endofyear_estimate.csv");
        var lines = File.ReadAllLines(file).Skip(1);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts[0] != customerId) continue;
            return new EndOfYearEstimate
            {
                CustomerId = customerId,
                Year = int.Parse(parts[1]),
                Estimate = new Estimate
                {
                    Electricity = new EstimateDetail
                    {
                        Usage = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        Unit = parts[3],
                        TotalCost = double.Parse(parts[4], CultureInfo.InvariantCulture)
                    },
                    Gas = new EstimateDetail
                    {
                        Usage = double.Parse(parts[5], CultureInfo.InvariantCulture),
                        Unit = parts[6],
                        TotalCost = double.Parse(parts[7], CultureInfo.InvariantCulture)
                    },
                    Total = double.Parse(parts[8], CultureInfo.InvariantCulture),
                    Currency = parts[9]
                }
            };
        }
        return null;
    }

    public PriceSheetResponse GetPriceSheet(string customerId)
    {
        var file = Path.Combine(_dataPath, "pricesheet.csv");
        var lines = File.ReadAllLines(file).Skip(1);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts[0] != customerId) continue;
            return new PriceSheetResponse
            {
                CustomerId = customerId,
                Pricesheet = new PriceSheet
                {
                    Electricity = new ElectricityPrice
                    {
                        BasePricePerKwh = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        TaxPerKwh = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        VatPerKwh = double.Parse(parts[3], CultureInfo.InvariantCulture),
                        GridChargePerDay = double.Parse(parts[4], CultureInfo.InvariantCulture),
                        TaxDiscountPerDay = double.Parse(parts[5], CultureInfo.InvariantCulture)
                    },
                    Gas = new GasPrice
                    {
                        BasePricePerM3 = double.Parse(parts[6], CultureInfo.InvariantCulture),
                        TaxPerM3 = double.Parse(parts[7], CultureInfo.InvariantCulture),
                        VatPerM3 = double.Parse(parts[8], CultureInfo.InvariantCulture),
                        GridChargePerDay = double.Parse(parts[9], CultureInfo.InvariantCulture)
                    }
                },
                Currency = parts[10]
            };
        }
        return null;
    }

    public ContractResponse GetContract(string customerId)
    {
        var file = Path.Combine(_dataPath, "contract.csv");
        var lines = File.ReadAllLines(file).Skip(1);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts[0] != customerId) continue;
            return new ContractResponse
            {
                CustomerId = customerId,
                Contract = new Contract
                {
                    StartDate = DateTime.Parse(parts[1]),
                    EndDate = DateTime.Parse(parts[2]),
                    Type = parts[3],
                    EnergyTypes = parts[4].Split(';').ToList(),
                    Status = parts[5]
                }
            };
        }
        return null;
    }

    public InstallmentResponse SaveInstallment(string customerId, InstallmentRequest request)
    {
        var file = Path.Combine(_dataPath, "installment.csv");
        var status = "scheduled";
        var line = string.Join(",",
            customerId,
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.Currency,
            request.StartDate.ToString("yyyy-MM-dd"),
            request.Frequency,
            status
        );
        // Append to CSV (simple, not thread-safe)
        File.AppendAllText(file, line + "\n");
        return new InstallmentResponse
        {
            CustomerId = customerId,
            Installment = new Installment
            {
                Amount = request.Amount,
                Currency = request.Currency,
                StartDate = request.StartDate,
                Frequency = request.Frequency,
                Status = status
            }
        };
    }

    public List<Installment> GetInstallments(string customerId)
    {
        var file = Path.Combine(_dataPath, "installment.csv");
        var result = new List<Installment>();
        if (!File.Exists(file))
            return result;

        var lines = File.ReadAllLines(file);
        foreach (var line in lines.Skip(1)) // skip header
        {
            var parts = line.Split(',');
            if (parts.Length < 6) continue;
            if (parts[0] != customerId) continue;
            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)) continue;
            if (!DateTime.TryParse(parts[3], out var startDate)) continue;
            result.Add(new Installment
            {
                Amount = amount,
                Currency = parts[2],
                StartDate = startDate,
                Frequency = parts[4],
                Status = parts[5]
            });
        }
        return result;
    }
}
