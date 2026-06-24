using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal
{
    public class PostJournalCommandHandler : IRequestHandler<PostJournalCommand, ApiResponseDTO<PostJournalResultDto>>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IMediator _mediator;

        public PostJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IJournalQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<PostJournalResultDto>> Handle(PostJournalCommand request, CancellationToken cancellationToken)
        {
            var journal = await _queryRepository.GetByIdAsync(request.Id)
                ?? throw new ExceptionRules("Journal voucher not found.");

            var postedStatusId = await _queryRepository.GetStatusIdAsync("POSTED");
            var postedById = _ipAddressService.GetUserId();
            var postedByName = _ipAddressService.GetUserName();
            var now = _timeZoneService.GetCurrentTime();

            var result = await _commandRepository.PostAsync(
                request.Id, postedStatusId, journal.FinancialYearName, postedByName, postedById, now, cancellationToken)
                ?? throw new ExceptionRules("Journal voucher could not be posted (missing or already posted).");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Post",
                actionCode: "JOURNAL_POST",
                actionName: result.VoucherNo ?? request.Id.ToString(),
                details: $"Journal voucher {request.Id} posted as {result.VoucherNo}.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            // US-GL01-16B — fire the flagging engine (alert-only; its handler never blocks posting).
            await _mediator.Publish(
                new JournalPostedDomainEvent(request.Id, journal.CompanyId, journal.TotalDr,
                    DateOnly.FromDateTime(now.DateTime), journal.IsReversal),
                cancellationToken);

            return new ApiResponseDTO<PostJournalResultDto>
            {
                IsSuccess = true,
                Message = $"Journal voucher posted successfully as {result.VoucherNo}.",
                Data = result
            };
        }
    }
}
