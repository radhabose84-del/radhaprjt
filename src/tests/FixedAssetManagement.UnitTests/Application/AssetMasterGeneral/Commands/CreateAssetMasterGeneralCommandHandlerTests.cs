using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class CreateAssetMasterGeneralCommandHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private CreateAssetMasterGeneralCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockCommandRepo.Object, _mockQueryRepo.Object,
                _mockMediator.Object, _mockCompanyLookup.Object, _mockUnitLookup.Object);

        // Create command with all required nested objects
        private static CreateAssetMasterGeneralCommand ValidCommand() =>
            new CreateAssetMasterGeneralCommand
            {
                AssetMaster = new AssetMasterDto
                {
                    AssetName = "Test Asset",
                    CompanyId = 1,
                    UnitId = 1,
                    AssetGroupId = 1,
                    AssetCategoryId = 1,
                    AssetSubCategoryId = 1,
                    // AssetLocation needed for line 36 of handler
                    AssetLocation = new AssetLocationCombineDto
                    {
                        DepartmentId = 1,
                        LocationId = 1
                    }
                }
            };

        private void SetupHappyPath(AssetMasterGenerals entity)
        {
            _mockQueryRepo
                .Setup(r => r.GetLatestAssetCode(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync("AST001");

            _mockMapper
                .Setup(m => m.Map<AssetMasterGenerals>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<AssetMasterGenerals>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<AssetMasterDto>(It.IsAny<object>()))
                .Returns(AssetMasterGeneralBuilders.ValidAssetMasterDto());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var entity = AssetMasterGeneralBuilders.ValidEntity();
            SetupHappyPath(entity);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var entity = AssetMasterGeneralBuilders.ValidEntity();
            SetupHappyPath(entity);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<AssetMasterGenerals>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var entity = AssetMasterGeneralBuilders.ValidEntity();
            SetupHappyPath(entity);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
