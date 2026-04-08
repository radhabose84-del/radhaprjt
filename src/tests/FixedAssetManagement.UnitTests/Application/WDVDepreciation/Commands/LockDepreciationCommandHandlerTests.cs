using AutoMapper;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Commands.LockDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.WDVDepreciation.Commands
{
    public sealed class LockDepreciationCommandHandlerTests
    {
        private readonly Mock<IWdvDepreciationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IWdvDepreciationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private LockDepreciationCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AlreadyLocked_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(1))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new LockDepreciationCommand { FinYearId = 1 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NoDataExists_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(1))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(1))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(new LockDepreciationCommand { FinYearId = 1 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.ExistDataLockedAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ExistDataAsync(1)).ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<CalculationDepreciationDto>(It.IsAny<object>()))
                .Returns(new CalculationDepreciationDto { FinYearId = 1 });

            _mockCommandRepo
                .Setup(r => r.LockWDVDepreciationAsync(1))
                .ReturnsAsync(1);

            var result = await CreateSut().Handle(
                new LockDepreciationCommand { FinYearId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_LockFails_ThrowsException()
        {
            _mockQueryRepo.Setup(r => r.ExistDataLockedAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ExistDataAsync(1)).ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<CalculationDepreciationDto>(It.IsAny<object>()))
                .Returns(new CalculationDepreciationDto { FinYearId = 1 });

            _mockCommandRepo
                .Setup(r => r.LockWDVDepreciationAsync(1))
                .ReturnsAsync(0);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(new LockDepreciationCommand { FinYearId = 1 }, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.ExistDataLockedAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ExistDataAsync(1)).ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<CalculationDepreciationDto>(It.IsAny<object>()))
                .Returns(new CalculationDepreciationDto { FinYearId = 1 });

            _mockCommandRepo.Setup(r => r.LockWDVDepreciationAsync(1)).ReturnsAsync(1);

            await CreateSut().Handle(new LockDepreciationCommand { FinYearId = 1 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
