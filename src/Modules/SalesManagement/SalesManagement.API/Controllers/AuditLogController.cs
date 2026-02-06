using SalesManagement.Application.AuditLog.Queries;
using SalesManagement.Application.AuditLog.Queries.GetAuditLog;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SalesManagement.API.Controllers
{
    [ApiController]
    [Route("api/sales/[controller]")]
    
    public class AuditLogController : ApiControllerBase
    {       
         
       public AuditLogController(ISender mediator) 
         : base(mediator)
        {       
             
             
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAuditLogsAsync()
        {            

            var auditLogs = await Mediator.Send(new GetAuditLogQuery());            
            return Ok(auditLogs);
        }              
        [HttpGet("GetAuditLogSearch")]
            public async Task<IActionResult> GetAuditLog([FromQuery] string searchPattern)
            {
            
                 var result = await Mediator.Send(new GetAuditLogBySearchPatternQuery {SearchPattern = searchPattern}); // Pass `searchPattern` to the constructor
                if (!result.IsSuccess)
                {                    
                    return Ok(new 
                    {
                        StatusCode=StatusCodes.Status200OK,
                        message = result.Message,
                        data = result.Data
                    });
                }
                return Ok(result.Data);
            }

        
        }
}