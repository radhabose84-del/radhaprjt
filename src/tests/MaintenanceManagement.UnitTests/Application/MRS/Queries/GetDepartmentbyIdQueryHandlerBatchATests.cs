using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Queries;
using MaintenanceManagement.Application.MRS.Queries.GetDepartment;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetDepartmentbyIdQueryHandlerBatchATests
    {
        private readonly Mock<IMRSQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepartmentbyIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetMDepartment(It.IsAny<string>()))
                .ReturnsAsync(new List<MDepartmentDto>());
            _mockMapper.Setup(m => m.Map<List<MDepartmentDto>>(It.IsAny<object>()))
                .Returns(new List<MDepartmentDto>());

            var result = await CreateSut().Handle(
                new GetDepartmentbyIdQuery { OldUnitcode = "U01" },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetMDepartment("ABC"))
                .ReturnsAsync(new List<MDepartmentDto>());
            _mockMapper.Setup(m => m.Map<List<MDepartmentDto>>(It.IsAny<object>()))
                .Returns(new List<MDepartmentDto>());

            await CreateSut().Handle(
                new GetDepartmentbyIdQuery { OldUnitcode = "ABC" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMDepartment("ABC"), Times.Once);
        }
    }
}
