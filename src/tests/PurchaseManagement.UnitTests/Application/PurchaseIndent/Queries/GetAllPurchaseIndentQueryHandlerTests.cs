using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Queries
{
    public sealed class GetAllPurchaseIndentQueryHandlerTests
    {
        private readonly Mock<IPurchaseIndentQuery> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetAllPurchaseIndentQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllPurchaseIndentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync((new List<IndentDto>(), 0));

            _mockUnitLookup
                .Setup(l => l.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());

            _mockDeptLookup
                .Setup(l => l.GetAllDepartmentAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(
                new GetAllPurchaseIndentQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(
                new GetAllPurchaseIndentQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(
                new GetAllPurchaseIndentQuery { PageNumber = 2, PageSize = 10 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(10);
        }
    }
}
