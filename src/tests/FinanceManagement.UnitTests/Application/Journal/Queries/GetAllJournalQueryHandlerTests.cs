using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetAllJournal;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.Journal.Queries
{
    public sealed class GetAllJournalQueryHandlerTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllJournalQueryHandler CreateSut()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);
        }

        private void SetupMapper(List<JournalHeaderDto> dtos) =>
            _mockMapper.Setup(m => m.Map<List<JournalHeaderDto>>(It.IsAny<object>())).Returns(dtos);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var dtos = new List<JournalHeaderDto> { JournalBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, 1, null)).ReturnsAsync((dtos, 1));
            SetupMapper(dtos);

            var result = await CreateSut().Handle(new GetAllJournalQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<JournalHeaderDto> { JournalBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "salary", 1, null)).ReturnsAsync((dtos, 11));
            SetupMapper(dtos);

            var result = await CreateSut().Handle(
                new GetAllJournalQuery { PageNumber = 2, PageSize = 5, SearchTerm = "salary" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.TotalCount.Should().Be(11);
        }
    }
}
