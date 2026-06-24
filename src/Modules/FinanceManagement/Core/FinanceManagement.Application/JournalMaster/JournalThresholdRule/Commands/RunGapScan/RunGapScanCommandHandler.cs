using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.RunGapScan
{
    public class RunGapScanCommandHandler : IRequestHandler<RunGapScanCommand, ApiResponseDTO<int>>
    {
        private readonly IGapScanService _gapScanService;

        public RunGapScanCommandHandler(IGapScanService gapScanService)
        {
            _gapScanService = gapScanService;
        }

        public async Task<ApiResponseDTO<int>> Handle(RunGapScanCommand request, CancellationToken cancellationToken)
        {
            var gaps = await _gapScanService.ScanAllAsync(cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = gaps == 0
                    ? "Gap scan complete — no missing voucher numbers found."
                    : $"Gap scan complete — {gaps} missing voucher number(s) found.",
                Data = gaps
            };
        }
    }
}
