using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "MISC01" }
            };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto>
            {
                new() { Id = 1, MiscTypeCode = "MISC01" }
            };

            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster("MISC")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(entities)).Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "MISC" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_CallsQueryRepo()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "MISC01" }
            };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { new() { Id = 1 } };

            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster("MISC")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(entities)).Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "MISC" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMiscTypeMaster("MISC"), Times.Once);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsFailureWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetMiscTypeMaster("UNKNOWN"))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.MiscTypeMaster>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "UNKNOWN" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.MiscTypeMaster>
            {
                new() { Id = 1, MiscTypeCode = "MISC01" }
            };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { new() { Id = 1 } };

            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster("MISC")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(entities)).Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "MISC" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
