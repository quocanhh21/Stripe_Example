using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Stripe_Example.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _configuration;
    public IStripeClient _stripeClient { get; }

    public WeatherForecastController(ILogger<WeatherForecastController> logger, 
                                            IConfiguration configuration,
                                            IStripeClient stripeClient)
    {
        _stripeClient = stripeClient;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        //StripeConfiguration.ApiKey = "sk_test_51J4ZQzK5J6";
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost("create-payment-intent")]
    public async Task<ActionResult> CreatePaymentIntent()
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = 1099,
            Currency = "usd",
            PaymentMethodTypes = new List<string>
            {
                "au_debit",
            },
        };
        var service = new PaymentIntentService(_stripeClient);
        try
        {
            var intent = await service.CreateAsync(options);
            return Ok(new { clientSecret = intent.ClientSecret });
                
        }
        catch(StripeException ex){
            return BadRequest(new 
            {
                Error = new
                {
                    Message= ex.Message
                }
            });
        }

    }

    [HttpPost("checkout")]
    public async Task<ActionResult> CreateCheckoutSession(CheckoutSessionRequest payload)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = payload.PriceId,
                    Quantity = payload.Quantity,
                },
            },
            Mode = "payment",
            ConsentCollection = new()
            {
                Promotions = "auto"
            },
            PhoneNumberCollection = new()
            {
                Enabled = true,
            },
            ShippingAddressCollection = new()
            {
                AllowedCountries = new List<string> { "US" }
            },
            //SuccessUrl = baseUrl + "/success?session_id={CHECKOUT_SESSION_ID}",
            SuccessUrl = baseUrl + $"/success",
            CancelUrl = baseUrl,
        };
        var service = new SessionService(_stripeClient);
        var session = await service.CreateAsync(options);

        return Ok(new { session.Url });
    }


    public record CheckoutSessionRequest(string PriceId, int Quantity);
}
