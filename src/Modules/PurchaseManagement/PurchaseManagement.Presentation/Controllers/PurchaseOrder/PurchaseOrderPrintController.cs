using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.PurchaseOrder.Print.Queries.GetPurchaseOrderPrintDetails;

namespace PurchaseManagement.Presentation.Controllers.PurchaseOrder;

[Route("api/[controller]")]
public class PurchaseOrderPrintController : ApiControllerBase
{
    public PurchaseOrderPrintController(ISender mediator) : base(mediator) { }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPurchaseOrderPrintDetailsAsync(int id)
    {
        var result = await Mediator.Send(new GetPurchaseOrderPrintDetailsQuery(id));

        return Ok(new
        {
            StatusCode = StatusCodes.Status200OK,
            data = result
        });
    }
}
