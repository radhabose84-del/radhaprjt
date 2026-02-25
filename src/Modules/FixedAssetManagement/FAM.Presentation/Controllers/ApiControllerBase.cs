using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FAM.Presentation.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private readonly ISender _mediator;

        protected ApiControllerBase(ISender mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected ISender Mediator => _mediator;
    }
}

