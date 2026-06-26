using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILogAnalysis;
using FinanceManagement.Application.JournalMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetLogAnalysisSummary
{
    public class GetLogAnalysisSummaryQueryHandler : IRequestHandler<GetLogAnalysisSummaryQuery, LogAnalysisSummaryDto>
    {
        private readonly ILogAnalysisQueryRepository _queryRepository;

        public GetLogAnalysisSummaryQueryHandler(ILogAnalysisQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        public Task<LogAnalysisSummaryDto> Handle(GetLogAnalysisSummaryQuery request, CancellationToken cancellationToken) =>
            _queryRepository.GetSummaryAsync(request.DateFrom, request.DateTo);
    }
}
