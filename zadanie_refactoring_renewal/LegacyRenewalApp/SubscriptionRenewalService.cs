using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private ICustomerRepository _customerRepository;
        private ISubscriptionPlanRepository _subscriptionPlanRepository;
        private IBillingService _billingService;
        private IDiscountCalculator _discountCalculator;
        private IFeeService _feeService;
        private ITaxService _taxService;

        public SubscriptionRenewalService
        (
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository,
            IBillingService billingService,
            IDiscountCalculator discountCalculator,
            IFeeService feeService,
            ITaxService taxService
        )
        {
            _customerRepository = customerRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _billingService = billingService;
            _discountCalculator = discountCalculator;
            _feeService = feeService;
            _taxService = taxService;
        }

        public SubscriptionRenewalService() : this
        (
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new BillingService(),
            new DiscountCalculator(),
            new FeeService(),
            new TaxService()
        ) {}

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
            
            var customer = _customerRepository.GetById(customerId);
            var plan = _subscriptionPlanRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            var discountResult = _discountCalculator.CalculateDiscount(customer, plan, seatCount, baseAmount, useLoyaltyPoints);

            decimal subtotalAfterDiscount = baseAmount - discountResult.DiscountAmount;
            string notes = discountResult.DiscountNotes;
            
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            decimal supportFee = _feeService.CalculateSupportFee(normalizedPlanCode, includePremiumSupport);
            if (includePremiumSupport)
            {
                notes += "premium support included; ";
            }
            
            var paymentResult = _feeService.CalculatePaymentFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
            decimal paymentFee = paymentResult.Amount;
            notes += paymentResult.Note;
            
            decimal taxRate = _taxService.CalculateTax(customer.Country);
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountResult.DiscountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };

            _billingService.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingService.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}