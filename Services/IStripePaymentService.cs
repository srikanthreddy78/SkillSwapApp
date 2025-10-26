using SkillSwapAPI.Models;
using Stripe;

namespace SkillSwapAPI.Services;

public interface IStripePaymentService
{
    Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
    Task<PaymentDetailsResponse> ConfirmPaymentAsync(string paymentIntentId);
    Task<PaymentDetailsResponse> GetPaymentIntentAsync(string paymentIntentId);
    Task<bool> CancelPaymentIntentAsync(string paymentIntentId);
    Task<RefundResponse> CreateRefundAsync(string paymentIntentId, decimal? amount = null);
    Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<Customer> GetCustomerAsync(string customerId);
    Task<PaymentMethod> AttachPaymentMethodAsync(string customerId, string paymentMethodId);
    Task<List<PaymentMethod>> GetCustomerPaymentMethodsAsync(string customerId);
}