using AutoMapper;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.DeleteDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationDetail.Commands
{
    public sealed class DeleteDepreciationDetailCommandHandlerTests
    {
        private readonly Mock<IDepreciationDetailCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepreciationDetailQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteDepreciationDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidDelete_ReturnsDto()
        {
            var dto = DepreciationDetailBuilders.ValidDto();
            var entity = new DepreciationDetails();

            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<DepreciationDetails>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(1);

            _mockMapper
                .Setup(m => m.Map<DepreciationDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                DepreciationDetailBuilders.ValidDeleteCommand(),
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_DataLocked_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(DepreciationDetailBuilders.ValidDeleteCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DataNotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<ValidationException>(() =>
                CreateSut().Handle(DepreciationDetailBuilders.ValidDeleteCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            var entity = new DepreciationDetails();

            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<DepreciationDetails>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(0);

            await Assert.ThrowsAsync<Exception>(() =>
                CreateSut().Handle(DepreciationDetailBuilders.ValidDeleteCommand(), CancellationToken.None));
        }
    }
}
