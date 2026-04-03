using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;
using ProductionManagement.Application.CertificationMaster.Queries.GetCertificationMasterById;

namespace ProductionManagement.UnitTests.Application.CertificationMaster.Queries
{
    public sealed class GetCertificationMasterByIdQueryHandlerTests
    {
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCertificationMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupMediator()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static CertificationMasterDto BuildDto(int id = 1, string name = "ISO 9001") =>
            new CertificationMasterDto
            {
                Id = id,
                CertificationName = name,
                IsActive = true,
                IsDeleted = false
            };

        [Fact]
        public async Task Handle_ExistingId_ReturnsMappedDto()
        {
            // Arrange
            var dto = BuildDto(1, "ISO 9001");
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<CertificationMasterDto>(It.IsAny<CertificationMasterDto>()))
                .Returns(dto);
            SetupMediator();

            // Act
            var result = await CreateSut().Handle(
                new GetCertificationMasterByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.CertificationName.Should().Be("ISO 9001");
        }

        [Fact]
        public async Task Handle_NonExistingId_ReturnsNull()
        {
            // Arrange — handler does `return null!` when repo returns null; no mapping, no audit
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((CertificationMasterDto?)null);

            // Act
            var result = await CreateSut().Handle(
                new GetCertificationMasterByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_NonExistingId_DoesNotPublishAuditEvent()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((CertificationMasterDto?)null);

            // Act
            await CreateSut().Handle(
                new GetCertificationMasterByIdQuery { Id = 999 }, CancellationToken.None);

            // Assert — audit is only published when record is found
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            // Arrange
            var dto = BuildDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<CertificationMasterDto>(It.IsAny<CertificationMasterDto>()))
                .Returns(dto);
            SetupMediator();

            // Act
            await CreateSut().Handle(
                new GetCertificationMasterByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesCorrectAuditActionDetail()
        {
            // Arrange
            var dto = BuildDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<CertificationMasterDto>(It.IsAny<CertificationMasterDto>()))
                .Returns(dto);
            SetupMediator();

            // Act
            await CreateSut().Handle(
                new GetCertificationMasterByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "GetCertificationMasterByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsGetByIdAsyncOnce()
        {
            // Arrange
            var dto = BuildDto(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<CertificationMasterDto>(It.IsAny<CertificationMasterDto>()))
                .Returns(dto);
            SetupMediator();

            // Act
            await CreateSut().Handle(
                new GetCertificationMasterByIdQuery { Id = 1 }, CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
