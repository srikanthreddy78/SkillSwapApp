using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace SkillSwapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IConfiguration configuration, ILogger<WebhookController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeSignature = Request.Headers["Stripe-Signature"];

        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret
            );

            // Handle the event
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogInformation($"Payment succeeded: {paymentIntent?.Id}");
                    // TODO: Handle successful payment (e.g., update database, send confirmation email)
                    break;

                case "payment_intent.payment_failed":
                    var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                    _logger.LogWarning($"Payment failed: {failedPayment?.Id}");
                    // TODO: Handle failed payment (e.g., notify user, log failure)
                    break;

                case "charge.refunded":
                    var refund = stripeEvent.Data.Object as Charge;
                    _logger.LogInformation($"Refund processed: {refund?.Id}");
                    // TODO: Handle refund (e.g., update database, notify user)
                    break;

                case "customer.created":
                    var customer = stripeEvent.Data.Object as Customer;
                    _logger.LogInformation($"Customer created: {customer?.Id}");
                    // TODO: Handle new customer (e.g., save to database)
                    break;

                default:
                    _logger.LogInformation($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing error");
            return StatusCode(500);
        }
    }
}