using System;

namespace LegacyRenewalApp;

public class FeeService : IFeeService
{
    public decimal CalculateSupportFee(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport)
        {
            return 0m;
        }
        
        decimal supportFee = planCode switch
        {
            "START" => 250m,
            "PRO" => 400m,
            "ENTERPRISE" => 700m,
            _ => 0m
        };

        return supportFee;
    }

    public FeeResult CalculatePaymentFee(string paymentMethod, decimal total)
    {
        var (paymentFee, note) = paymentMethod switch
        {
            "CARD" => (total * 0.02m, "card payment fee; "),
            "BANK_TRANSFER" => (total * 0.01m, "bank transfer fee; "),
            "PAYPAL" => (total * 0.035m, "paypal fee; "),
            "INVOICE" => (0m, "invoice payment; "),
            _ => throw new ArgumentException("Unsupported payment method")
        };
        
        return new FeeResult { Amount = paymentFee, Note = note };
    }
}
