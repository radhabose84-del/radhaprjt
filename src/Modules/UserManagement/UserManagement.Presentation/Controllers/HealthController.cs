using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class HealthController : ApiControllerBase
    {
        public HealthController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Healthy");
        }
    }
}
