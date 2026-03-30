using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.SpecificationMasters.Queries
{
    public sealed class GetSpecificationMasterQueryHandlerTests
    {
        private readonly Mock<ISpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSpecificationMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<SpecificationMasterDTO> { SpecificationMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllSpecificationGroupAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterDTO>>(It.IsAny<object>()))
                .Returns(dtoList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSpecificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<SpecificationMasterDTO> { SpecificationMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllSpecificationGroupAsync(2, 5, "test"))
                .ReturnsAsync((dtoList, 11));
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterDTO>>(It.IsAny<object>()))
                .Returns(dtoList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSpecificationMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = new List<SpecificationMasterDTO>();
            _mockQueryRepo
                .Setup(r => r.GetAllSpecificationGroupAsync(1, 10, null))
                .ReturnsAsync((emptyList, 0));
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterDTO>>(It.IsAny<object>()))
                .Returns(emptyList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetSpecificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var dtoList = new List<SpecificationMasterDTO>();
            _mockQueryRepo
                .Setup(r => r.GetAllSpecificationGroupAsync(1, 10, null))
                .ReturnsAsync((dtoList, 0));
            _mockMapper
                .Setup(m => m.Map<List<SpecificationMasterDTO>>(It.IsAny<object>()))
                .Returns(dtoList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetSpecificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
