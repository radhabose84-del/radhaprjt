using ProductionManagement.Application.Common.Interfaces.ICertificationMaster;
using ProductionManagement.Application.CertificationMaster.Dto;
using ProductionManagement.Application.CertificationMaster.Queries.GetAllCertificationMaster;

namespace ProductionManagement.UnitTests.Application.CertificationMaster.Queries
{
    public sealed class GetAllCertificationMasterQueryHandlerTests
    {
        private readonly Mock<ICertificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllCertificationMasterQueryHandler CreateSut() =>
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
        public async Task Handle_WithSeededData_ReturnsSuccess()
        {
            // Arrange
            var dtoList = new List<CertificationMasterDto> { BuildDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));
            _mockMapper
                .Setup(m => m.Map<List<CertificationMasterDto>>(It.IsAny<List<CertificationMasterDto>>()))
                .Returns(dtoList);
            SetupMediator();

            // Act
            var result = await CreateSut().Handle(
                new GetAllCertificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithSeededData_ReturnsCorrectCount()
        {
            // Arrange
            var dtoList = new List<CertificationMasterDto> { BuildDto(1), BuildDto(2, "ISO 14001") };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 2));
            _mockMapper
                .Setup(m => m.Map<List<CertificationMasterDto>>(It.IsAny<List<CertificationMasterDto>>()))
                .Returns(dtoList);
            SetupMediator();

            // Act
            var result = await CreateSut().Handle(
                new GetAllCertificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var emptyList = new List<CertificationMasterDto>();
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((emptyList, 0));
            _mockMapper
                .Setup(m => m.Map<List<CertificationMasterDto>>(It.IsAny<List<CertificationMasterDto>>()))
                .Returns(emptyList);
            SetupMediator();

            // Act
            var result = await CreateSut().Handle(
                new GetAllCertificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithPagination_ReturnsPaginationMetadata()
        {
            // Arrange
            var dtoList = new List<CertificationMasterDto> { BuildDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "ISO"))
                .ReturnsAsync((dtoList, 11));
            _mockMapper
                .Setup(m => m.Map<List<CertificationMasterDto>>(It.IsAny<List<CertificationMasterDto>>()))
                .Returns(dtoList);
            SetupMediator();

            // Act
            var result = await CreateSut().Handle(
                new GetAllCertificationMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "ISO" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_ValidQuery_PublishesAuditEvent()
        {
            // Arrange
            var dtoList = new List<CertificationMasterDto> { BuildDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));
            _mockMapper
                .Setup(m => m.Map<List<CertificationMasterDto>>(It.IsAny<List<CertificationMasterDto>>()))
                .Returns(dtoList);
            SetupMediator();

            // Act
            await CreateSut().Handle(
                new GetAllCertificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsGetAllAsyncOnce()
        {
            // Arrange
            var dtoList = new List<CertificationMasterDto> { BuildDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));
            _mockMapper
                .Setup(m => m.Map<List<CertificationMasterDto>>(It.IsAny<List<CertificationMasterDto>>()))
                .Returns(dtoList);
            SetupMediator();

            // Act
            await CreateSut().Handle(
                new GetAllCertificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, null), Times.Once);
        }
    }
}
