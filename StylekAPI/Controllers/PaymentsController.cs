using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StylekAPI.DTOs.Payments;
using StylekAPI.Helpers;
using StylekAPI.Services;

namespace StylekAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpPost("create-intent")]
    public async Task<ActionResult<ApiResponse<PaymentIntentResponseDto>>> CreatePaymentIntent(CreatePaymentIntentDto dto)
    {
        var result = await _paymentService.CreatePaymentIntentAsync(User.GetUserId(), dto);
        return Ok(ApiResponse<PaymentIntentResponseDto>.Ok(result));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        await _paymentService.HandleWebhookAsync(json, signature);
        return Ok();
    }
}
