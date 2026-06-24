using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringJournals
{
    public class GenerateRecurringJournalsCommandHandler
        : IRequestHandler<GenerateRecurringJournalsCommand, ApiResponseDTO<int>>
    {
        private readonly IRecurringJournalGenerationService _generationService;
        private readonly IIPAddressService _ipAddressService;

        public GenerateRecurringJournalsCommandHandler(
            IRecurringJournalGenerationService generationService,
            IIPAddressService ipAddressService)
        {
            _generationService = generationService;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(GenerateRecurringJournalsCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var generated = await _generationService.GenerateForPeriodAsync(
                companyId, request.BaseCurrencyId, request.Period ?? string.Empty, request.VoucherDate, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = $"{generated} recurring journal(s) generated for {request.Period}.",
                Data = generated
            };
        }
    }
}
