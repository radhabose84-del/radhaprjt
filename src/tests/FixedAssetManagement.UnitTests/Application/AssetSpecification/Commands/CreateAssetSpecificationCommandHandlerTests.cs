using AutoMapper;
using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetSpecification.Commands
{
    public sealed class CreateAssetSpecificationCommandHandlerTests
    {
        private readonly Mock<IAssetSpecificationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateAssetSpecificationCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_NoExistingSpec_ReturnsSavedMessage()
        {
            var entity = AssetSpecificationBuilders.ValidEntity(1);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<AssetSpecifications>()))
                .ReturnsAsync(entity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = AssetSpecificationBuilders.ValidCreateCommand();
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be("Specifications saved successfully.");
        }

        [Fact]
        public async Task Handle_NoExistingSpec_CallsCreateOnce()
        {
            var entity = AssetSpecificationBuilders.ValidEntity(1);

            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<AssetSpecifications>()))
                .ReturnsAsync(entity);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = AssetSpecificationBuilders.ValidCreateCommand();
            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<AssetSpecifications>()), Times.Once);
        }

        [Fact]
        public async Task Handle_AlreadyExists_ThrowsValidationException()
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByAssetSpecIdAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = AssetSpecificationBuilders.ValidCreateCommand();

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptySpecifications_ReturnsNoNewMessage()
        {
            var command = new CreateAssetSpecificationCommand
            {
                AssetId = 1,
                Specifications = new List<SpecificationItem>()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be("No new specifications were saved.");
        }
    }
}
