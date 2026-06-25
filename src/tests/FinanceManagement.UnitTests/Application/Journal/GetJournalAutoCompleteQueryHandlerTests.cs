using AutoMapper;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalAutoComplete;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class GetJournalAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GetJournalAutoCompleteQueryHandler CreateSut() =>
            new(_query.Object, _ip.Object, _mapper.Object, _mediator.Object);

        [Fact]
        public async Task Handle_PassesTermCompanyAndStatus_AndReturnsRows()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            var rows = new List<JournalLookupDto> { new() { Id = 1, VoucherNo = "JV/2026-27/0001", StatusId = 66 } };

            _query.Setup(r => r.AutocompleteAsync("JV", 1, 66, It.IsAny<CancellationToken>())).ReturnsAsync(rows);
            _mapper.Setup(m => m.Map<List<JournalLookupDto>>(rows)).Returns(rows);

            var result = await CreateSut().Handle(new GetJournalAutoCompleteQuery("JV", 66), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].VoucherNo.Should().Be("JV/2026-27/0001");
            _query.Verify(r => r.AutocompleteAsync("JV", 1, 66, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoStatus_PassesNull()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);
            var rows = new List<JournalLookupDto>();
            _query.Setup(r => r.AutocompleteAsync("", 1, null, It.IsAny<CancellationToken>())).ReturnsAsync(rows);
            _mapper.Setup(m => m.Map<List<JournalLookupDto>>(rows)).Returns(rows);

            var result = await CreateSut().Handle(new GetJournalAutoCompleteQuery(""), CancellationToken.None);

            result.Should().BeEmpty();
            _query.Verify(r => r.AutocompleteAsync("", 1, null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
