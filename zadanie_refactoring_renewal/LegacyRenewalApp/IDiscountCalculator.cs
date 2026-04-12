namespace LegacyRenewalApp;

public interface IDiscountCalculator
{
    DiscountResult CalculateDiscount(Customer customer, SubscriptionPlan subscriptionPlan, int seatCount, decimal amount, bool useLoyaltyPoints);
}