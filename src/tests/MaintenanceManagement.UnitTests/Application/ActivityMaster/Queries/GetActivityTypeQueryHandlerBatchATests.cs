using AutoMapper;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityType;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetActivityTypeQueryHandlerBatchATests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetActivityTypeQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockRepo.Setup(r => r.GetActivityTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(new GetActivityTypeQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var items = new List<MaintenanceManagement.Domain.Entities.MiscMaster>
            {
                new() { Id = 1, Code = "X" }
            };
            var dtos = new List<GetMiscMasterDto> { new() };

            _mockRepo.Setup(r => r.GetActivityTypeAsync()).ReturnsAsync(items);
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetActivityTypeQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            _mockRepo.Setup(r => r.GetActivityTypeAsync())
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster>());
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscMasterDto>());

            await CreateSut().Handle(new GetActivityTypeQuery(), CancellationToken.None);

            _mockRepo.Verify(r => r.GetActivityTypeAsync(), Times.Once);
        }
    }
}
