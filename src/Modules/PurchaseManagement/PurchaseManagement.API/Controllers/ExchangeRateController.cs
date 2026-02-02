using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;
using PurchaseManagement.Application.ExchangeRate.Queries.GetRateByDate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public sealed class ExchangeRateController : ControllerBase
{
    private readonly IMediator _mediator;
    public ExchangeRateController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
            /*     {
            "baseCurrency": "INR",
            "symbols": ["USD","EUR"]
            } */
    public async Task<IActionResult> CreateAsync([FromBody] IngestBody body, CancellationToken ct)
    {
        var count = await _mediator.Send(new ExchangeRateCommand(body.BaseCurrency, body.Symbols), ct);

        return Ok(new
        {
            StatusCode = StatusCodes.Status201Created,
            message = "Created Successfully",
            errors = "",
            data = new { upserted = count }
        });
    }
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] string @base, [FromQuery] string quote, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetLatestRateQuery(@base, quote), ct);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Success",
            errors = "",
            data = dto
        });
    }

    // GET api/fx/by-date?base=INR&quote=EUR&date=2025-10-30
    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDate([FromQuery] string @base, [FromQuery] string quote, [FromQuery] DateOnly date, CancellationToken ct)
    {
        var dto = await _mediator.Send(new GetRateByDateQuery(@base, quote, date), ct);

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            message = "Success",
            errors = "",
            data = dto
        });
    }
    public sealed class IngestBody
    {        
        public string BaseCurrency { get; set; } = "INR";
        public string[] Symbols { get; set; } = new[] { "USD", "EUR" };
    }
}
