using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetInsurance;
using FAM.Domain.Events;
using FluentValidation;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetInsurance.Commands
{
    public sealed class CreateAssetInsuranceCommandHandlerTests
    {
        private readonly Mock<IAssetInsuranceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetInsuranceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateAssetInsuranceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = AssetInsuranceBuilders.ValidEntity(id);
            var dto = AssetInsuranceBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetMaster.AssetInsurance>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByAssetIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetAssetInsuranceDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath(1);
            var command = AssetInsuranceBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = AssetInsuranceBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetMaster.AssetInsurance>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = AssetInsuranceBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ThrowsValidationException()
        {
            var entityWithZeroId = AssetInsuranceBuilders.ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<FAM.Domain.Entities.AssetMaster.AssetInsurance>(It.IsAny<object>()))
                .Returns(entityWithZeroId);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FAM.Domain.Entities.AssetMaster.AssetInsurance>()))
                .ReturnsAsync(entityWithZeroId);

            var command = AssetInsuranceBuilders.ValidCreateCommand();
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
