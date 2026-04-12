namespace LegacyRenewalApp;

public class TaxService : ITaxService
{
    public decimal CalculateTax(string country)
    {
        decimal taxRate = country switch
        {
            "Poland" => 0.23m,
            "Germany" => 0.19m,
            "Czech Republic" => 0.21m,
            "Norway" => 0.25m,
            _ => 0.20m
        };
        
        return  taxRate;
    }
}
