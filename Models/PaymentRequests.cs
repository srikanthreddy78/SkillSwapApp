namespace SkillSwapAPI.Models;

public class CreatePaymentIntentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string? Description { get; set; }
    public string? CustomerId { get; set; }
}

public class ConfirmPaymentRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
}

public class RefundRequest
{
    public decimal? Amount { get; set; }
}

public class CreateCustomerRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}

public class AttachPaymentMethodRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
}