using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.SearchJournal;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class SearchJournalQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private SearchJournalQueryHandler CreateSut() => new(_query.Object, _ip.Object, _mediator.Object);

        [Fact]
        public async Task Handle_MapsFilters_AndReturnsPagedResults()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(1);

            JournalSearchFilter? captured = null;
            _query.Setup(r => r.SearchAsync(It.IsAny<JournalSearchFilter>(), 2, 25, 1))
                .Callback<JournalSearchFilter, int, int, int?>((f, _, _, _) => captured = f)
                .ReturnsAsync((new List<JournalListItemDto> { new() { Id = 1 } }, 40));

            var result = await CreateSut().Handle(new SearchJournalQuery
            {
                PageNumber = 2,
                PageSize = 25,
                VoucherNo = "JV",
                StatusId = 65,
                AmountMin = 100000,
                AccountId = 5,
                Narration = "salary"
            }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(40);
            result.PageNumber.Should().Be(2);

            captured.Should().NotBeNull();
            captured!.VoucherNo.Should().Be("JV");
            captured.StatusId.Should().Be(65);
            captured.AmountMin.Should().Be(100000);
            captured.AccountId.Should().Be(5);
            captured.Narration.Should().Be("salary");
        }
    }
}
