using SkillSwapAPI.Models;
using Stripe;

namespace SkillSwapAPI.Services;

/// <summary>
/// Implementation of Stripe payment service
/// Handles all Stripe payment operations including payment intents, refunds, and customer management
/// </summary>
public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _configuration;

    // Constructor - inject configuration for Stripe settings
    public StripePaymentService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Creates a new payment intent in Stripe
    /// Converts dollar amount to cents as required by Stripe API
    /// </summary>
    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request)
    {
        // Setup payment intent options
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(request.Amount * 100), // Stripe requires amount in smallest currency unit (cents)
            Currency = request.Currency,
            Description = request.Description,
            Customer = request.CustomerId,
            // Enable automatic payment methods (card, Apple Pay, Google Pay, etc.)
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        // Create payment intent via Stripe SDK
        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        // Return response with client secret for frontend
        return new PaymentIntentResponse
        {
            ClientSecret = paymentIntent.ClientSecret,
            PaymentIntentId = paymentIntent.Id,
            Amount = paymentIntent.Amount,
            Currency = paymentIntent.Currency
        };
    }

    /// <summary>
    /// Confirms a payment intent (usually done after client-side authentication)
    /// </summary>
    public async Task<PaymentDetailsResponse> ConfirmPaymentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.ConfirmAsync(paymentIntentId);

        // Map Stripe object to our response model
        return MapToPaymentDetailsResponse(paymentIntent);
    }

    /// <summary>
    /// Retrieves payment intent details from Stripe
    /// Used to check payment status
    /// </summary>
    public async Task<PaymentDetailsResponse> GetPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.GetAsync(paymentIntentId);

        return MapToPaymentDetailsResponse(paymentIntent);
    }

    /// <summary>
    /// Cancels a pending payment intent
    /// Only works if payment hasn't been completed yet
    /// </summary>
    public async Task<bool> CancelPaymentIntentAsync(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var paymentIntent = await service.CancelAsync(paymentIntentId);

        // Verify cancellation was successful
        return paymentIntent.Status == "canceled";
    }

    /// <summary>
    /// Creates a refund for a successful payment
    /// If amount is null, refunds the full payment amount
    /// </summary>
    public async Task<RefundResponse> CreateRefundAsync(string paymentIntentId, decimal? amount = null)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId,
        };

        // If partial refund amount specified, convert to cents
        if (amount.HasValue)
        {
            options.Amount = (long)(amount.Value * 100);
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

    /// <summary>
    /// Creates a new customer in Stripe
    /// Customers can have saved payment methods and transaction history
    /// </summary>
    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var options = new CustomerCreateOptions
        {
            Email = request.Email,
            Name = request.Name,
            Metadata = request.Metadata  // Custom key-value data (e.g., our internal user ID)
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

    /// <summary>
    /// Retrieves customer details from Stripe
    /// </summary>
    public async Task<Customer> GetCustomerAsync(string customerId)
    {
        var service = new CustomerService();
        return await service.GetAsync(customerId);
    }

    /// <summary>
    /// Attaches a payment method to a customer
    /// Allows customer to reuse saved payment methods
    /// </summary>
    public async Task<PaymentMethod> AttachPaymentMethodAsync(string customerId, string paymentMethodId)
    {
        var options = new PaymentMethodAttachOptions
        {
            Customer = customerId,
        };

        var service = new PaymentMethodService();
        return await service.AttachAsync(paymentMethodId, options);
    }

    /// <summary>
    /// Gets all saved payment methods for a customer
    /// Currently filters to cards only
    /// </summary>
    public async Task<List<PaymentMethod>> GetCustomerPaymentMethodsAsync(string customerId)
    {
        var options = new PaymentMethodListOptions
        {
            Customer = customerId,
            Type = "card",  // Filter to card payment methods only
        };

        var service = new PaymentMethodService();
        var paymentMethods = await service.ListAsync(options);

        return paymentMethods.Data.ToList();
    }

    /// <summary>
    /// Helper method to map Stripe PaymentIntent to our response model
    /// Centralizes the mapping logic for reuse
    /// </summary>
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