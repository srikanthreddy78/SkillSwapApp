namespace SkillSwapAPI.Models;

/// <summary>
/// Request model for creating a Stripe payment intent
/// Payment intent represents the intention to collect payment from a customer
/// </summary>
public class CreatePaymentIntentRequest
{
    // Payment amount in smallest currency unit (e.g., cents for USD)
    public decimal Amount { get; set; }
    
    // Three-letter ISO currency code (default USD)
    public string Currency { get; set; } = "usd";
    
    // Optional description of the payment (shown on statements)
    public string? Description { get; set; }
    
    // Optional Stripe customer ID to associate with this payment
    public string? CustomerId { get; set; }
}

/// <summary>
/// Request model for confirming a payment intent
/// Used to complete the payment after client-side confirmation
/// </summary>
public class ConfirmPaymentRequest
{
    // Stripe payment intent ID to confirm
    public string PaymentIntentId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for processing a refund
/// </summary>
public class RefundRequest
{
    // Amount to refund (in smallest currency unit)
    // If null, refunds the full amount
    public decimal? Amount { get; set; }
}

/// <summary>
/// Request model for creating a new Stripe customer
/// Customers can be reused for multiple payments and subscriptions
/// </summary>
public class CreateCustomerRequest
{
    // Customer's email address
    public string Email { get; set; } = string.Empty;
    
    // Customer's full name
    public string Name { get; set; } = string.Empty;
    
    // Optional metadata for storing additional custom data
    // Useful for linking Stripe customers to our internal user IDs
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Request model for attaching a payment method to a customer
/// Allows customers to save payment methods for future use
/// </summary>
public class AttachPaymentMethodRequest
{
    // Stripe customer ID to attach the payment method to
    public string CustomerId { get; set; } = string.Empty;
    
    // Payment method ID (card, bank account, etc.) to attach
    public string PaymentMethodId { get; set; } = string.Empty;
}