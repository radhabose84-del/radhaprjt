using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries.BatchD
{
    public sealed class GetExternalRequestByIdQueryHandlerBatchDTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetExternalRequestByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_NullIds_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new GetExternalRequestsByIdsQuery { Ids = null }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_EmptyIds_ReturnsFailure()
        {
            var result = await CreateSut().Handle(
                new GetExternalRequestsByIdsQuery { Ids = new List<int>() }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetExternalRequestByIdAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetExternalRequestByIdDto>());

            var result = await CreateSut().Handle(
                new GetExternalRequestsByIdsQuery { Ids = new List<int> { 1 } }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetExternalRequestByIdAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(new List<GetExternalRequestByIdDto> { new() { Id = 1 } });

            var result = await CreateSut().Handle(
                new GetExternalRequestsByIdsQuery { Ids = new List<int> { 1 } }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }
    }
}
