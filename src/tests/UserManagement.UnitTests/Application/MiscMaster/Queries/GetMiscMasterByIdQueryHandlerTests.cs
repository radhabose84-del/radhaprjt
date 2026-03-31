using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using UserManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsMappedDto()
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster { Id = 1, Code = "MC001", Description = "Test" };
            var dto = new GetMiscMasterDto { Id = 1, Code = "MC001", Description = "Test" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Code.Should().Be("MC001");
        }

        [Fact]
        public async Task Handle_ExistingId_CallsQueryRepoOnce()
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster { Id = 5, Code = "MC005" };
            var dto = new GetMiscMasterDto { Id = 5 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster { Id = 1, Code = "MC001" };
            var dto = new GetMiscMasterDto { Id = 1 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetById"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectDescription()
        {
            var entity = new UserManagement.Domain.Entities.MiscMaster { Id = 3, Code = "MC003", Description = "Active Type" };
            var dto = new GetMiscMasterDto { Id = 3, Code = "MC003", Description = "Active Type" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 3 }, CancellationToken.None);

            result.Description.Should().Be("Active Type");
        }
    }
}
