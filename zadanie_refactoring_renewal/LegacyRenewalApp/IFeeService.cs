namespace LegacyRenewalApp;

public interface IFeeService
{
    decimal CalculateSupportFee(string planCode, bool includePremiumSupport);
    FeeResult CalculatePaymentFee(string paymentMethod, decimal total);
}