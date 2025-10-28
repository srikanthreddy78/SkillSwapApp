using SkillSwapAPI.Models;
using Stripe;

namespace SkillSwapAPI.Services;

/// <summary>
/// Interface for Stripe payment service operations
/// Defines methods for payment intents, refunds, customers, and payment methods
/// </summary>
public interface IStripePaymentService
{
    /// <summary>
    /// Creates a new payment intent for processing a payment
    /// Returns client secret for completing payment on frontend
    /// </summary>
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    
    /// <summary>
    /// Confirms a payment intent after client-side authentication
    /// Returns updated payment details with final status
    /// </summary>
    Task<PaymentDetailsResponse> ConfirmPaymentAsync(string paymentIntentId);
    
    /// <summary>
    /// Retrieves details of an existing payment intent
    /// Used to check payment status and details
    /// </summary>
    Task<PaymentDetailsResponse> GetPaymentIntentAsync(string paymentIntentId);
    
    /// <summary>
    /// Cancels a payment intent before it's completed
    /// Returns true if cancellation was successful
    /// </summary>
    Task<bool> CancelPaymentIntentAsync(string paymentIntentId);
    
    /// <summary>
    /// Creates a refund for a successful payment
    /// Can refund full amount or partial if amount is specified
    /// </summary>
    Task<RefundResponse> CreateRefundAsync(string paymentIntentId, decimal? amount = null);
    
    /// <summary>
    /// Creates a new customer in Stripe
    /// Allows saving payment methods and tracking payment history
    /// </summary>
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    
    /// <summary>
    /// Retrieves customer details from Stripe by customer ID
    /// </summary>
    Task<Customer> GetCustomerAsync(string customerId);
    
    /// <summary>
    /// Attaches a payment method to a customer for future use
    /// Enables saved cards and one-click payments
    /// </summary>
    Task<PaymentMethod> AttachPaymentMethodAsync(string customerId, string paymentMethodId);
    
    /// <summary>
    /// Gets all payment methods associated with a customer
    /// Returns list of saved cards, bank accounts, etc.
    /// </summary>
    Task<List<PaymentMethod>> GetCustomerPaymentMethodsAsync(string customerId);
}