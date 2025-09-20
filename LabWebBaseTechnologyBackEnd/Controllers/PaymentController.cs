using LabWebBaseTechnologyBackEnd.DataAccess;
using LabWebBaseTechnologyBackEnd.DataAccess.ModulEntity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Text.Json;

namespace AirportApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly LabWebBaseTechnologyDBContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(LabWebBaseTechnologyDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["StripeSecretKey"];
        }

        [HttpPost("create-checkout-session")]
        public async Task<ActionResult> CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            var booking = new BookingEntity
            {
                Id = Guid.NewGuid(),
                FlightId = request.FlightId,
                UserId = request.UserId,
                UserName = request.UserName,
                Email = request.Email,
                BookingDate = DateTime.UtcNow
            };

            var payment = new PaymentEntity
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = request.Amount,
                Status = "Pending",
                PaymentDate = DateTime.UtcNow
            };

            booking.Payment = payment;
            _context.Bookings.Add(booking);
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Flight Booking for {request.UserName}",
                            },
                            UnitAmount = (long)(request.Amount * 100), // У центах
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/cancel",
                Metadata = new Dictionary<string, string> { { "bookingId", booking.Id.ToString() } },
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return Ok(new { sessionId = session.Id });
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"];
            var webhookSecret = _configuration["StripeWebhookSecret"]; // Додайте в ENV

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;
                    var bookingId = Guid.Parse(session.Metadata["bookingId"]);
                    var booking = await _context.Bookings
                        .Include(b => b.Payment)
                        .FirstOrDefaultAsync(b => b.Id == bookingId);

                    if (booking?.Payment != null)
                    {
                        booking.Payment.Status = "Completed";
                        await _context.SaveChangesAsync();
                    }
                }


                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest($"Webhook error: {ex.Message}");
            }
        }
    }

    public class PaymentRequest
    {
        public Guid FlightId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; } // Додано для передачі суми
    }
}