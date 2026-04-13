using AutoMapper;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.UpdateDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationDetail.Commands
{
    public sealed class UpdateDepreciationDetailCommandHandlerTests
    {
        private readonly Mock<IDepreciationDetailCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDepreciationDetailQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateDepreciationDetailCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private static UpdateDepreciationDetailCommand ValidCommand() =>
            new()
            {
                companyId = 1,
                unitId = 1,
                finYearId = 2026,
                depreciationType = 1,
                depreciationPeriod = 12
            };

        private void SetupHappyPath(int updateResult = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            var entity = new DepreciationDetails { Finyear = 2026 };

            _mockMapper
                .Setup(m => m.Map<DepreciationDetails>(It.IsAny<object>()))
                .Returns(entity);

            _mockMapper
                .Setup(m => m.Map<DepreciationDto>(It.IsAny<object>()))
                .Returns(new DepreciationDto { Company = "TestCo", Unit = "TestUnit" });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(updateResult);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.Company.Should().Be("TestCo");
            result.Unit.Should().Be("TestUnit");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ChecksLockedAndExistData()
        {
            SetupHappyPath();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
            _mockQueryRepo.Verify(
                r => r.ExistDataAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenLocked_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(true);

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidCommand(), CancellationToken.None));

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenNoDataExists_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.ExistDataLockedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ExistDataAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(ValidCommand(), CancellationToken.None));

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUpdateReturnsZero_ThrowsException()
        {
            SetupHappyPath(updateResult: 0);

            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(ValidCommand(), CancellationToken.None));

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
