using AutoMapper;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<PartyManagement.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 15, null))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 15, null))
                .ReturnsAsync((new List<PartyManagement.Domain.Entities.MiscTypeMaster>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscTypeMasterDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
