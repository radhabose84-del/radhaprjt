using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(1);
            var dto = MiscTypeMasterBuilders.ValidDto(1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((PartyManagement.Domain.Entities.MiscTypeMaster)null!);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterByIdQuery { Id = 99 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
