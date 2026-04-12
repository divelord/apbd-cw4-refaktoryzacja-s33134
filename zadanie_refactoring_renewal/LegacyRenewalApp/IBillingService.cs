namespace LegacyRenewalApp;

public interface IBillingService
{
    void SaveInvoice(RenewalInvoice renewalInvoice);
    void SendEmail(string email, string subject, string body);
}