namespace SkillSwapAPI.Models;

/// <summary>
/// Response model containing payment intent details for client-side processing
/// Client secret is used on the frontend to complete payment with Stripe.js
/// </summary>
public class PaymentIntentResponse
{
    // Client secret key for frontend Stripe.js SDK
    // This is what the client uses to confirm the payment
    public string ClientSecret { get; set; } = string.Empty;
    
    // Unique payment intent identifier from Stripe
    public string PaymentIntentId { get; set; } = string.Empty;
    
    // Payment amount in smallest currency unit (cents)
    public long Amount { get; set; }
    
    // Three-letter ISO currency code
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// Response model containing detailed payment information
/// Used when retrieving existing payment details
/// </summary>
public class PaymentDetailsResponse
{
    // Stripe payment intent ID
    public string Id { get; set; } = string.Empty;
    
    // Amount charged in smallest currency unit
    public long Amount { get; set; }
    
    // Currency code for the payment
    public string Currency { get; set; } = string.Empty;
    
    // Payment status (succeeded, processing, requires_action, canceled, etc.)
    public string Status { get; set; } = string.Empty;
    
    // Optional payment description
    public string? Description { get; set; }
    
    // Timestamp when payment was created
    public DateTime Created { get; set; }
}

/// <summary>
/// Response model for Stripe customer creation
/// Returns basic customer information after successful creation
/// </summary>
public class CustomerResponse
{
    // Unique Stripe customer ID (starts with "cus_")
    public string CustomerId { get; set; } = string.Empty;
    
    // Customer's email address
    public string Email { get; set; } = string.Empty;
    
    // Customer's full name
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Response model for refund operations
/// Contains refund status and amount information
/// </summary>
public class RefundResponse
{
    // Unique refund identifier from Stripe (starts with "re_")
    public string RefundId { get; set; } = string.Empty;
    
    // Refund status (succeeded, pending, failed, canceled)
    public string Status { get; set; } = string.Empty;
    
    // Amount refunded in smallest currency unit
    public long Amount { get; set; }
}