using AutoMapper;
using FAM.Application.Common.Interfaces.IWdvDepreciation;
using FAM.Application.WDVDepreciation.Commands.CreateDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.WDVDepreciation.Commands
{
    public sealed class CreateDepreciationCommandHandlerTests
    {
        private readonly Mock<IWdvDepreciationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateDepreciationCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AlreadyLocked_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(1))
                .ReturnsAsync(true);

            var command = new CreateDepreciationCommand { FinYearId = 1 };

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DataAlreadyExists_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(1))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(1))
                .ReturnsAsync(true);

            var command = new CreateDepreciationCommand { FinYearId = 1 };

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(1))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(1))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.WDVDepreciationDetail>(It.IsAny<object>()))
                .Returns(new FAM.Domain.Entities.WDVDepreciationDetail());

            _mockQueryRepo
                .Setup(r => r.CreateAsync(1))
                .ReturnsAsync(new List<CalculationDepreciationDto>
                {
                    new CalculationDepreciationDto { FinYearId = 1 }
                });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // The handler returns null! when result has items (counterintuitive but matches source code)
            var result = await CreateSut().Handle(
                new CreateDepreciationCommand { FinYearId = 1 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.CreateAsync(1), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsEmpty_ThrowsException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(1))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(1))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.WDVDepreciationDetail>(It.IsAny<object>()))
                .Returns(new FAM.Domain.Entities.WDVDepreciationDetail());

            _mockQueryRepo
                .Setup(r => r.CreateAsync(1))
                .ReturnsAsync(new List<CalculationDepreciationDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(new CreateDepreciationCommand { FinYearId = 1 }, CancellationToken.None));
        }
    }
}
