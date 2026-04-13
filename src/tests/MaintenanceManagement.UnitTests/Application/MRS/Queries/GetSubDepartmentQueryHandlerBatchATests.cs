using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Queries.GetSubDepartment;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetSubDepartmentQueryHandlerBatchATests
    {
        private readonly Mock<IMRSQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSubDepartmentQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetSubDepartment(It.IsAny<string>()))
                .ReturnsAsync(new List<MSubDepartment>());
            _mockMapper.Setup(m => m.Map<List<MSubDepartment>>(It.IsAny<object>()))
                .Returns(new List<MSubDepartment>());

            var result = await CreateSut().Handle(
                new GetSubDepartmentQuery { OldUnitcode = "U01" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetSubDepartment("CODE"))
                .ReturnsAsync(new List<MSubDepartment>());
            _mockMapper.Setup(m => m.Map<List<MSubDepartment>>(It.IsAny<object>()))
                .Returns(new List<MSubDepartment>());

            await CreateSut().Handle(
                new GetSubDepartmentQuery { OldUnitcode = "CODE" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetSubDepartment("CODE"), Times.Once);
        }
    }
}
