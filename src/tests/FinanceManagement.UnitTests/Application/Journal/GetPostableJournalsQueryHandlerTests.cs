using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetPostableJournals;
using MediatR;

namespace FinanceManagement.UnitTests.Application.Journal
{
    public sealed class GetPostableJournalsQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _query = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Loose);

        private GetPostableJournalsQueryHandler CreateSut() => new(_query.Object, _ip.Object, _mediator.Object);

        [Fact]
        public async Task Handle_ReturnsPagedPostableVouchers_ScopedToCompany()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns(7);
            _query.Setup(r => r.GetPostableAsync(2, 25, 7))
                .ReturnsAsync((new List<JournalListItemDto> { new() { Id = 1 }, new() { Id = 2 } }, 12));

            var result = await CreateSut().Handle(
                new GetPostableJournalsQuery { PageNumber = 2, PageSize = 25 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(12);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(25);
            _query.Verify(r => r.GetPostableAsync(2, 25, 7), Times.Once);
        }

        [Fact]
        public async Task Handle_NoActiveCompany_Throws()
        {
            _ip.Setup(s => s.GetCompanyId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new GetPostableJournalsQuery(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
