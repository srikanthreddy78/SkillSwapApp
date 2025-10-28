using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace SkillSwapAPI.Controllers;

/// <summary>
/// Webhook controller for handling external service callbacks
/// Currently handles Stripe payment events
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;

    // Constructor - inject config for webhook secrets and logger
    public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming Stripe webhook events
    /// Validates signature and processes payment-related events
    /// POST: api/webhook/stripe
    /// </summary>
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        // Read raw request body for signature validation
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        try
        {
            // Get webhook secret from config for signature verification
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            
            // Construct and verify the event using Stripe signature
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            // Handle different event types from Stripe
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    // Payment was successful
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation($"Payment succeeded: {paymentIntent?.Id}");
                    // TODO: Handle successful payment (e.g., update database, send confirmation email)
                    break;

                case "payment_intent.payment_failed":
                    // Payment attempt failed
                    var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogWarning($"Payment failed: {failedPayment?.Id}");
                    // TODO: Handle failed payment (e.g., notify user, log failure)
                    break;

                case "charge.refunded":
                    // Refund was processed
                    var refund = stripeEvent.Data.Object as Charge;
                    _logger.LogInformation($"Refund processed: {refund?.Id}");
                    // TODO: Handle refund (e.g., update database, notify user)
                    break;

                case "customer.created":
                    // New customer created in Stripe
                    var customer = stripeEvent.Data.Object as Customer;
                    _logger.LogInformation($"Customer created: {customer?.Id}");
                    // TODO: Handle new customer (e.g., save to database)
                    break;

                default:
                    // Log any unhandled event types for monitoring
                    _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }

            // Return 200 to acknowledge receipt to Stripe
            return Ok();
        }
        catch (StripeException ex)
        {
            // Signature validation failed or other Stripe-specific error
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
        catch (Exception ex)
        {
            // Catch any other unexpected errors
            _logger.LogError(ex, "Webhook processing error");
            return StatusCode(500);
        }
    }
}