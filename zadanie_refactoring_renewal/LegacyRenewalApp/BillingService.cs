namespace LegacyRenewalApp;

public class BillingService : IBillingService
{
    public void SaveInvoice(RenewalInvoice renewalInvoice)
    {
        LegacyBillingGateway.SaveInvoice(renewalInvoice);
    }

    public void SendEmail(string email, string subject, string body)
    {
        LegacyBillingGateway.SendEmail(email, subject, body);
    }
}