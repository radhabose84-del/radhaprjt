using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.IconMaster.Queries
{
    public sealed class GetAllIconMasterQueryHandlerTests
    {
        private readonly Mock<IIconMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetIconMasterQueryHandler>> _mockLogger = new();

        private GetIconMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.IconMaster> { IconMasterBuilders.ValidEntity() };
            var dtos = new List<IconMasterDto> { IconMasterBuilders.ValidDto() };

            _mockQueryRepo.Setup(r => r.GetAllIconMasterAsync(1, 10, null)).ReturnsAsync((entities, 1));
            _mockMapper.Setup(m => m.Map<List<IconMasterDto>>(entities)).Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetIconMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.IconMaster> { IconMasterBuilders.ValidEntity() };
            var dtos = new List<IconMasterDto> { IconMasterBuilders.ValidDto() };

            _mockQueryRepo.Setup(r => r.GetAllIconMasterAsync(2, 5, "set")).ReturnsAsync((entities, 11));
            _mockMapper.Setup(m => m.Map<List<IconMasterDto>>(entities)).Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetIconMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "set" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.IconMaster>();
            var dtos = new List<IconMasterDto>();

            _mockQueryRepo.Setup(r => r.GetAllIconMasterAsync(1, 10, null)).ReturnsAsync((entities, 0));
            _mockMapper.Setup(m => m.Map<List<IconMasterDto>>(entities)).Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetIconMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
