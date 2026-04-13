using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Queries.GetSubCostCenter;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetSubCostCenterQueryHandlerBatchATests
    {
        private readonly Mock<IMRSQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSubCostCenterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetSubCostCenter(It.IsAny<string>()))
                .ReturnsAsync(new List<MSubCostCenterDto>());
            _mockMapper.Setup(m => m.Map<List<MSubCostCenterDto>>(It.IsAny<object>()))
                .Returns(new List<MSubCostCenterDto>());

            var result = await CreateSut().Handle(
                new GetSubCostCenterQuery { OldUnitcode = "U01" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithUnitCode()
        {
            _mockQueryRepo.Setup(r => r.GetSubCostCenter("U02"))
                .ReturnsAsync(new List<MSubCostCenterDto>());
            _mockMapper.Setup(m => m.Map<List<MSubCostCenterDto>>(It.IsAny<object>()))
                .Returns(new List<MSubCostCenterDto>());

            await CreateSut().Handle(
                new GetSubCostCenterQuery { OldUnitcode = "U02" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetSubCostCenter("U02"), Times.Once);
        }
    }
}
