using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.DeleteAttachment;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.RequestRework;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.SubmitFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UpdateFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UploadAttachment;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetAllFeedback;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackByAssignment;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbackById;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetFeedbacksByComplaint;
using SalesManagement.Application.ComplaintDepartmentFeedback.Queries.GetMyPendingFeedbacks;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ComplaintDepartmentFeedbackController : ApiControllerBase
    {
        public ComplaintDepartmentFeedbackController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] string? StatusFilter = null,
            [FromQuery] bool MyPendingOnly = false)
        {
            var result = await Mediator.Send(new GetAllComplaintDepartmentFeedbackQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusFilter = StatusFilter,
                MyPendingOnly = MyPendingOnly
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetComplaintDepartmentFeedbackByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("by-assignment/{assignmentId}")]
        public async Task<IActionResult> GetByAssignmentIdAsync(int assignmentId)
        {
            var result = await Mediator.Send(new GetFeedbackByAssignmentIdQuery
            {
                AssignmentId = assignmentId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpGet("by-complaint/{complaintHeaderId}")]
        public async Task<IActionResult> GetByComplaintIdAsync(int complaintHeaderId)
        {
            var result = await Mediator.Send(new GetFeedbacksByComplaintIdQuery
            {
                ComplaintHeaderId = complaintHeaderId
            });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("my-pending")]
        public async Task<IActionResult> GetMyPendingAsync()
        {
            var result = await Mediator.Send(new GetMyPendingFeedbacksQuery());
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpPost("upload-attachment")]
        public async Task<IActionResult> UploadAttachment([FromForm] UploadFeedbackAttachmentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Attachment uploaded successfully.",
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitComplaintDepartmentFeedbackCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateFeedback([FromBody] UpdateComplaintDepartmentFeedbackCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpDelete("delete-attachment/{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var result = await Mediator.Send(new DeleteFeedbackAttachmentCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPut("request-rework")]
        public async Task<IActionResult> RequestRework([FromBody] RequestReworkCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
