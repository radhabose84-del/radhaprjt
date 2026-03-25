using Contracts.Common;
using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupAutoComplete;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupById;
using FAM.Presentation.Controllers;
using FixedAssetManagement.UnitTests.TestData;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class DepreciationGroupControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateDepreciationGroupCommand>> _mockCreateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateDepreciationGroupCommand>> _mockUpdateValidator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<DeleteDepreciationGroupCommand>> _mockDeleteValidator = new(MockBehavior.Loose);

        private DepreciationGroupController CreateSut() =>
            new(_mockMediator.Object, _mockCreateValidator.Object,
                _mockUpdateValidator.Object, _mockDeleteValidator.Object);

        private void SetupValidCreateValidator() =>
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

        private void SetupValidUpdateValidator() =>
            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

        private void SetupValidDeleteValidator() =>
            _mockDeleteValidator
                .Setup(v => v.ValidateAsync(It.IsAny<DeleteDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            var dtoList = new List<DepreciationGroupDTO> { DepreciationGroupBuilders.ValidDto() };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepreciationGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().GetAllDepreciationGroupsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            var dtoList = new List<DepreciationGroupDTO>();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepreciationGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((dtoList, 0));

            await CreateSut().GetAllDepreciationGroupsAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetDepreciationGroupQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepreciationGroupByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepreciationGroupBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            var list = new List<DepreciationGroupAutoCompleteDTO> { DepreciationGroupBuilders.ValidAutoCompleteDto() };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDepreciationGroupAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(list);

            var result = await CreateSut().GetDepreciationGroup("DG");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_WithValidCommand_ReturnsOkResult()
        {
            SetupValidCreateValidator();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepreciationGroupBuilders.ValidDto());

            var result = await CreateSut().CreateAsync(DepreciationGroupBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WithValidCommand_ReturnsOkResult()
        {
            SetupValidUpdateValidator();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(DepreciationGroupBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsOkResult()
        {
            SetupValidDeleteValidator();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepreciationGroupBuilders.ValidDto());

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            SetupValidDeleteValidator();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDepreciationGroupCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DepreciationGroupBuilders.ValidDto());

            await CreateSut().DeleteAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteDepreciationGroupCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
