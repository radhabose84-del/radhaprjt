using FinanceManagement.Application.TaxCode.Commands.ActivateTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMapping;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeRateVersion;
using FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMapping;
using FinanceManagement.Application.TaxCode.Commands.DeleteTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.DeleteTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMapping;
using FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionMapping;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMappingById;
using FinanceManagement.Application.TaxCode.Queries.GetLinkageChangeAudit;
using FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageByAccount;
using FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageById;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeEffective;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterAutoComplete;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterById;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeRateVersions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class TaxCodeController : ApiControllerBase
    {
        public TaxCodeController(IMediator mediator) : base(mediator) { }

        // ─── Tax Code Master (US-GL02-05A) ─────────────────────────────────

        [HttpGet("tax-code")]
        public async Task<IActionResult> GetAllTaxCodesAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? CompanyId = null,
            [FromQuery] string? TaxType = null)
        {
            var result = await Mediator.Send(new GetAllTaxCodeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                CompanyId = CompanyId,
                TaxType = TaxType
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("tax-code/{id:int}")]
        public async Task<IActionResult> GetTaxCodeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetTaxCodeMasterByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("tax-code/by-name")]
        public async Task<IActionResult> GetTaxCodeAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? CompanyId = null,
            [FromQuery] string? TaxType = null)
        {
            var result = await Mediator.Send(new GetTaxCodeMasterAutoCompleteQuery(term ?? string.Empty, CompanyId, TaxType));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("tax-code/{id:int}/versions")]
        public async Task<IActionResult> GetTaxCodeRateVersionsAsync(int id)
        {
            var result = await Mediator.Send(new GetTaxCodeRateVersionsQuery { TaxCodeId = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data, TotalCount = result.TotalCount });
        }

        [HttpGet("tax-code/effective")]
        public async Task<IActionResult> GetTaxCodeEffectiveAsync(
            [FromQuery] string code,
            [FromQuery] DateOnly asOf,
            [FromQuery] int? CompanyId = null)
        {
            var result = await Mediator.Send(new GetTaxCodeEffectiveQuery { Code = code, CompanyId = CompanyId, AsOf = asOf });
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("tax-code")]
        public async Task<IActionResult> CreateTaxCode([FromBody] CreateTaxCodeMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut("tax-code")]
        public async Task<IActionResult> UpdateTaxCode([FromBody] UpdateTaxCodeMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("tax-code/{id:int}/rate-version")]
        public async Task<IActionResult> CreateTaxCodeRateVersion(int id, [FromBody] CreateTaxCodeRateVersionCommand command)
        {
            command.TaxCodeId = id;
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("tax-code/{id:int}")]
        public async Task<IActionResult> DeleteTaxCode(int id)
        {
            var result = await Mediator.Send(new DeleteTaxCodeMasterCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "Tax Code deleted successfully." : "Failed to delete Tax Code." });
        }

        // ─── Tax Account Linkage (US-GL02-05B) ─────────────────────────────

        [HttpGet("linkage")]
        public async Task<IActionResult> GetAllLinkagesAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? CompanyId = null)
        {
            var result = await Mediator.Send(new GetAllTaxAccountLinkageQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                CompanyId = CompanyId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("linkage/{id:int}")]
        public async Task<IActionResult> GetLinkageByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetTaxAccountLinkageByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("linkage/by-account/{glAccountId:int}")]
        public async Task<IActionResult> GetLinkageByAccountAsync(int glAccountId)
        {
            var result = await Mediator.Send(new GetTaxAccountLinkageByAccountQuery { GlAccountId = glAccountId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpGet("linkage/change-audit")]
        public async Task<IActionResult> GetLinkageChangeAuditAsync(
            [FromQuery] string? Status = null,
            [FromQuery] int? CompanyId = null)
        {
            var result = await Mediator.Send(new GetLinkageChangeAuditQuery { Status = Status, CompanyId = CompanyId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data, TotalCount = result.TotalCount });
        }

        [HttpPost("linkage")]
        public async Task<IActionResult> CreateLinkage([FromBody] CreateTaxAccountLinkageCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("linkage/{id:int}/activate")]
        public async Task<IActionResult> ActivateLinkage(int id)
        {
            var result = await Mediator.Send(new ActivateTaxAccountLinkageCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost("linkage/change-request")]
        public async Task<IActionResult> SubmitLinkageChangeRequest([FromBody] SubmitLinkageChangeRequestCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("linkage/{id:int}")]
        public async Task<IActionResult> DeleteLinkage(int id)
        {
            var result = await Mediator.Send(new DeleteTaxAccountLinkageCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "Linkage deleted successfully." : "Failed to delete linkage." });
        }

        // ─── GSTR Section Mapping (US-GL02-05B AC3) ────────────────────────

        [HttpGet("gstr-section")]
        public async Task<IActionResult> GetAllGstrSectionsAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? CompanyId = null)
        {
            var result = await Mediator.Send(new GetAllGstrSectionMappingQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                CompanyId = CompanyId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("gstr-section/{id:int}")]
        public async Task<IActionResult> GetGstrSectionByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetGstrSectionMappingByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost("gstr-section")]
        public async Task<IActionResult> CreateGstrSection([FromBody] CreateGstrSectionMappingCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut("gstr-section")]
        public async Task<IActionResult> UpdateGstrSection([FromBody] UpdateGstrSectionMappingCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("gstr-section/{id:int}")]
        public async Task<IActionResult> DeleteGstrSection(int id)
        {
            var result = await Mediator.Send(new DeleteGstrSectionMappingCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "GSTR section mapping deleted successfully." : "Failed to delete GSTR section mapping." });
        }
    }
}
