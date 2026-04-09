using AutoMapper;
using Contracts.Common;
using FAM.Application.AssetMaster.AssetLocation.Queries.GetCustodian;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetLocation;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetLocation.Queries
{
    public sealed class GetCustodianQueryHandlerTests
    {
        private readonly Mock<IAssetLocationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCustodianQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<Employee> { new() };
            var dtos = new List<GetCustodianDto> { new() };

            _mockRepo
                .Setup(r => r.GetAllCustodianAsync("UNIT1", "search"))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<GetCustodianDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCustodianQuery { OldUnitId = "UNIT1", SearchEmployee = "search" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockRepo
                .Setup(r => r.GetAllCustodianAsync("UNIT1", (string?)null))
                .ReturnsAsync((new List<Employee>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<GetCustodianDto>>(It.IsAny<object>()))
                .Returns(new List<GetCustodianDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCustodianQuery { OldUnitId = "UNIT1", SearchEmployee = null },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
