using SkillSwapAPI.Models;
using Stripe;

namespace SkillSwapAPI.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _configuration;

    public StripePaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Convert to cents
            Currency = request.Currency,
            Description = request.Description,
            Customer = request.CustomerId,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return new PaymentIntentResponse
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id,
            Amount = paymentIntent.Amount,
            Currency = paymentIntent.Currency
        };
    }

    public async Task<PaymentDetailsResponse> ConfirmPaymentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.ConfirmAsync(paymentIntentId);

        return MapToPaymentDetailsResponse(paymentIntent);
    }

    public async Task<PaymentDetailsResponse> GetPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId);

        return MapToPaymentDetailsResponse(paymentIntent);
    }

    public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.CancelAsync(paymentIntentId);

        return paymentIntent.Status == "canceled";
    }

    public async Task<RefundResponse> CreateRefundAsync(string paymentIntentId, decimal? amount = null)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
        };

        if (amount.HasValue)
        {
            options.Amount = (long)(amount.Value * 100); // Convert to cents
        }

        var service = new RefundService();
        var refund = await service.CreateAsync(options);

        return new RefundResponse
        {
            RefundId = refund.Id,
            Status = refund.Status,
            Amount = refund.Amount
        };
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var options = new CustomerCreateOptions
        {
            Email = request.Email,
            Name = request.Name,
            Metadata = request.Metadata
        };

        var service = new CustomerService();
        var customer = await service.CreateAsync(options);

        return new CustomerResponse
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            Name = customer.Name
        };
    }

    public async Task<Customer> GetCustomerAsync(string customerId)
    {
        var service = new CustomerService();
        return await service.GetAsync(customerId);
    }

    public async Task<PaymentMethod> AttachPaymentMethodAsync(string customerId, string paymentMethodId)
    {
        var options = new PaymentMethodAttachOptions
        {
            Customer = customerId,
        };

        var service = new PaymentMethodService();
        return await service.AttachAsync(paymentMethodId, options);
    }

    public async Task<List<PaymentMethod>> GetCustomerPaymentMethodsAsync(string customerId)
    {
        var options = new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card",
        };

        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(options);

        return paymentMethods.Data.ToList();
    }

    private static PaymentDetailsResponse MapToPaymentDetailsResponse(PaymentIntent paymentIntent)
    {
        return new PaymentDetailsResponse
        {
            Id = paymentIntent.Id,
            Amount = paymentIntent.Amount,
            Currency = paymentIntent.Currency,
            Status = paymentIntent.Status,
            Description = paymentIntent.Description,
            Created = paymentIntent.Created
        };
    }
}