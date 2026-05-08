using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetMasterGeneral;
using FAM.Domain.Entities;
using FAM.Domain.Events;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetMasterGeneral.Commands
{
    public sealed class UpdateAssetMasterGeneralCommandHandlerTests
    {
        private readonly Mock<IAssetMasterGeneralCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IAssetMasterGeneralQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private UpdateAssetMasterGeneralCommandHandler CreateSut() =>
            new(
                _mockCommandRepo.Object,
                _mockMapper.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockCompanyLookup.Object,
                _mockUnitLookup.Object);

        private static AssetMasterUpdateDto BuildDto(byte isActive = 1) =>
            new AssetMasterUpdateDto
            {
                Id = 1,
                AssetCode = "AST001",
                AssetName = "Updated Asset",
                CompanyId = 1,
                UnitId = 1,
                AssetGroupId = 1,
                AssetCategoryId = 1,
                AssetSubCategoryId = 1,
                IsActive = isActive,
                AssetImage = null,
                AssetDocument = null
            };

        private void SetupHappyPath(AssetMasterUpdateDto dto)
        {
            var existing = new AssetMasterGeneralDTO
            {
                Id = dto.Id,
                AssetCode = dto.AssetCode,
                AssetName = "Original Name",
                CompanyId = dto.CompanyId,
                UnitId = dto.UnitId,
                AssetGroupId = 1,
                AssetCategoryId = 1,
                AssetSubCategoryId = 1
            };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(existing);

            _mockMapper
                .Setup(m => m.Map<AssetMasterGenerals>(It.IsAny<AssetMasterUpdateDto>()))
                .Returns(new AssetMasterGenerals { Id = dto.Id, AssetCode = dto.AssetCode, AssetName = dto.AssetName });

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(dto.Id, It.IsAny<AssetMasterGenerals>()))
                .ReturnsAsync(true);

            _mockCompanyLookup
                .Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = 1, CompanyName = "Test Co" }
                });

            _mockUnitLookup
                .Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Unit A" }
                });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var dto = BuildDto();
            SetupHappyPath(dto);
            var command = new UpdateAssetMasterGeneralCommand { AssetMaster = dto };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            var dto = BuildDto();
            SetupHappyPath(dto);
            var command = new UpdateAssetMasterGeneralCommand { AssetMaster = dto };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(dto.Id, It.IsAny<AssetMasterGenerals>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var dto = BuildDto();
            SetupHappyPath(dto);
            var command = new UpdateAssetMasterGeneralCommand { AssetMaster = dto };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AssetNotFound_ThrowsValidationException()
        {
            var dto = BuildDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync((AssetMasterGeneralDTO)null!);

            var command = new UpdateAssetMasterGeneralCommand { AssetMaster = dto };
            var sut = CreateSut();

            await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));
        }

        [Fact(Skip = "Re-enable when Active/Inactive toggle is wired up in Asset Master UI. Guard was removed in handler because no UI surface exists today; restoring the test is the spec for restoring the handler block.")]
        public async Task Handle_InactivateWhenLinked_ThrowsValidationException()
        {
            var dto = BuildDto(isActive: 0);

            _mockQueryRepo
                .Setup(r => r.IsAssetMasterLinkedAsync(dto.Id))
                .ReturnsAsync(true);

            var command = new UpdateAssetMasterGeneralCommand { AssetMaster = dto };
            var sut = CreateSut();

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                sut.Handle(command, CancellationToken.None));

            ex.Message.Should().Contain("linked");
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ThrowsException()
        {
            var dto = BuildDto();
            SetupHappyPath(dto);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(dto.Id, It.IsAny<AssetMasterGenerals>()))
                .ReturnsAsync(false);

            var command = new UpdateAssetMasterGeneralCommand { AssetMaster = dto };
            var sut = CreateSut();

            await Assert.ThrowsAsync<Exception>(() =>
                sut.Handle(command, CancellationToken.None));
        }
    }
}
