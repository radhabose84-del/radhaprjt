using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsSuccess()
        {
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster { Id = 1, MiscTypeCode = "MISC01", Description = "Test" };
            var dto = new GetMiscTypeMasterDto { Id = 1, MiscTypeCode = "MISC01", Description = "Test" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsCorrectDto()
        {
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster { Id = 7, MiscTypeCode = "MISC07" };
            var dto = new GetMiscTypeMasterDto { Id = 7, MiscTypeCode = "MISC07" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 7 }, CancellationToken.None);

            result.Data!.MiscTypeCode.Should().Be("MISC07");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.MiscTypeMaster)null!);

            var result = await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = new UserManagement.Domain.Entities.MiscTypeMaster { Id = 1, MiscTypeCode = "MISC01" };
            var dto = new GetMiscTypeMasterDto { Id = 1, MiscTypeCode = "MISC01" };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<GetMiscTypeMasterDto>(entity)).Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMiscTypeMasterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
