using Contracts.Common;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournalBatch
{
    public class PostJournalBatchCommandHandler
        : IRequestHandler<PostJournalBatchCommand, ApiResponseDTO<List<PostJournalBatchItemDto>>>
    {
        private readonly IMediator _mediator;

        public PostJournalBatchCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PostJournalBatchItemDto>>> Handle(
            PostJournalBatchCommand request, CancellationToken cancellationToken)
        {
            var results = new List<PostJournalBatchItemDto>();

            // Best-effort: reuse the single-post pipeline (validation + posting + audit + flagging) per voucher,
            // catching failures individually so one bad voucher never blocks the rest. Duplicates are collapsed.
            foreach (var id in request.Ids.Distinct())
            {
                try
                {
                    var res = await _mediator.Send(new PostJournalCommand(id), cancellationToken);
                    results.Add(new PostJournalBatchItemDto
                    {
                        JournalId = id,
                        IsSuccess = res.IsSuccess,
                        VoucherNo = res.Data?.VoucherNo,
                        Message = res.Message
                    });
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    results.Add(new PostJournalBatchItemDto
                    {
                        JournalId = id,
                        IsSuccess = false,
                        Message = ex.Message
                    });
                }
            }

            var posted = results.Count(r => r.IsSuccess);
            var failed = results.Count - posted;

            return new ApiResponseDTO<List<PostJournalBatchItemDto>>
            {
                IsSuccess = failed == 0,
                Message = $"{posted} voucher(s) posted, {failed} failed.",
                Data = results,
                TotalCount = results.Count
            };
        }
    }
}
