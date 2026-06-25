using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetAllFinancialYearMaster;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Queries
{
    public sealed class GetAllFinancialYearMasterQueryHandlerTests
    {
        private readonly Mock<IFinancialYearMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp       = new(MockBehavior.Loose);
        private readonly Mock<IMapper>           _mockMapper   = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);

        private GetAllFinancialYearMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int companyId = 1, int totalCount = 1)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(companyId);

            var data = new List<FinancialYearMasterDto> { FinancialYearMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), companyId, It.IsAny<int?>()))
                .ReturnsAsync((data, totalCount));

            _mockMapper
                .Setup(m => m.Map<List<FinancialYearMasterDto>>(It.IsAny<List<FinancialYearMasterDto>>()))
                .Returns(data);
        }

        [Fact]
        public async Task Handle_ValidQuery_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new GetAllFinancialYearMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupHappyPath(totalCount: 25);
            var result = await CreateSut().Handle(
                new GetAllFinancialYearMasterQuery { PageNumber = 2, PageSize = 5 },
                CancellationToken.None);

            result.TotalCount.Should().Be(25);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_NoCompanyInSession_ThrowsExceptionRules()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            Func<Task> act = () => CreateSut().Handle(
                new GetAllFinancialYearMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ScopesQueryByCompanyIdFromSession()
        {
            SetupHappyPath(companyId: 7);
            await CreateSut().Handle(
                new GetAllFinancialYearMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), 7, It.IsAny<int?>()),
                Times.Once);
        }
    }
}
