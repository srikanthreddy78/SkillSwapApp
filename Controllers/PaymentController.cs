using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Models;
using SkillSwapAPI.Services;

namespace SkillSwapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize] // All endpoints require authentication
public class PaymentController : ControllerBase
{
    private readonly IStripePaymentService _stripeService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IStripePaymentService stripeService, ILogger<PaymentController> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new payment intent
    /// </summary>
    [HttpPost("create-payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            var result = await _stripeService.CreatePaymentIntentAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            return StatusCode(500, new { message = "Failed to create payment intent", error = ex.Message });
        }
    }

    /// <summary>
    /// Confirm a payment intent
    /// </summary>
    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var result = await _stripeService.ConfirmPaymentAsync(request.PaymentIntentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment");
            return StatusCode(500, new { message = "Failed to confirm payment", error = ex.Message });
        }
    }

    /// <summary>
    /// Get payment intent details
    /// </summary>
    [HttpGet("payment-intent/{id}")]
    public async Task<IActionResult> GetPaymentIntent(string id)
    {
        try
        {
            var result = await _stripeService.GetPaymentIntentAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment intent");
            return StatusCode(500, new { message = "Failed to retrieve payment intent", error = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a payment intent
    /// </summary>
    [HttpPost("cancel-payment/{id}")]
    public async Task<IActionResult> CancelPayment(string id)
    {
        try
        {
            var result = await _stripeService.CancelPaymentIntentAsync(id);
            return Ok(new { message = "Payment cancelled successfully", cancelled = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment");
            return StatusCode(500, new { message = "Failed to cancel payment", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a refund for a payment
    /// </summary>
    [HttpPost("refund/{paymentIntentId}")]
    public async Task<IActionResult> CreateRefund(string paymentIntentId, [FromBody] RefundRequest? request = null)
    {
        try
        {
            var result = await _stripeService.CreateRefundAsync(paymentIntentId, request?.Amount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating refund");
            return StatusCode(500, new { message = "Failed to create refund", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost("create-customer")]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            var result = await _stripeService.CreateCustomerAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, new { message = "Failed to create customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Get customer details
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomer(string customerId)
    {
        try
        {
            var result = await _stripeService.GetCustomerAsync(customerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer");
            return StatusCode(500, new { message = "Failed to retrieve customer", error = ex.Message });
        }
    }

    /// <summary>
    /// Attach a payment method to a customer
    /// </summary>
    [HttpPost("attach-payment-method")]
    public async Task<IActionResult> AttachPaymentMethod([FromBody] AttachPaymentMethodRequest request)
    {
        try
        {
            var result = await _stripeService.AttachPaymentMethodAsync(request.CustomerId, request.PaymentMethodId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attaching payment method");
            return StatusCode(500, new { message = "Failed to attach payment method", error = ex.Message });
        }
    }

    /// <summary>
    /// Get customer's payment methods
    /// </summary>
    [HttpGet("customer/{customerId}/payment-methods")]
    public async Task<IActionResult> GetPaymentMethods(string customerId)
    {
        try
        {
            var result = await _stripeService.GetCustomerPaymentMethodsAsync(customerId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment methods");
            return StatusCode(500, new { message = "Failed to retrieve payment methods", error = ex.Message });
        }
    }
}