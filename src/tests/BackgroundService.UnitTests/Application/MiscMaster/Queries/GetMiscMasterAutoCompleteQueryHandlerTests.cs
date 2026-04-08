using AutoMapper;
using BackgroundService.Application.Interfaces.IMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<BackgroundService.Domain.Entities.Notification.MiscMaster> ValidEntities() =>
            new()
            {
                new() { Id = 1, Code = "TEST001", Description = "Test One" },
                new() { Id = 2, Code = "TEST002", Description = "Test Two" }
            };

        private static List<GetMiscMasterAutoCompleteDto> ValidDtos() =>
            new()
            {
                new() { Id = 1, Code = "TEST001", Description = "Test One" },
                new() { Id = 2, Code = "TEST002", Description = "Test Two" }
            };

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster("Test", "TYPE1"))
                .ReturnsAsync(ValidEntities());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "Test", MiscTypeCode = "TYPE1" },
                CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].Code.Should().Be("TEST001");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster("NoMatch", "TYPE1"))
                .ReturnsAsync(new List<BackgroundService.Domain.Entities.Notification.MiscMaster>());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscMaster>>()))
                .Returns(new List<GetMiscMasterAutoCompleteDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "NoMatch", MiscTypeCode = "TYPE1" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscMaster("Test", "TYPE1"))
                .ReturnsAsync(ValidEntities());

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<List<BackgroundService.Domain.Entities.Notification.MiscMaster>>()))
                .Returns(ValidDtos());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "Test", MiscTypeCode = "TYPE1" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAll"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
