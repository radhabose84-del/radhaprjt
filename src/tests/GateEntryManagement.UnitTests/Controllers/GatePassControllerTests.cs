using Contracts.Common;
using Contracts.Dtos.Lookups.Finance;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Presentation.Controllers;
using GateEntryManagement.Application.GatePass.Commands.CreateGatePass;
using GateEntryManagement.Application.GatePass.Commands.DeleteGatePass;
using GateEntryManagement.Application.GatePass.Queries.GetAllGatePass;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassById;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassAutoComplete;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassDocTypes;
using GateEntryManagement.Application.GatePass.Dto;

namespace GateEntryManagement.UnitTests.Controllers
{
    public sealed class GatePassControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private GatePassController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllGatePassQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GatePassHdrDto>>
                {
                    IsSuccess = true,
                    Data = new List<GatePassHdrDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllGatePassAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllGatePassQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GatePassHdrDto>>
                {
                    IsSuccess = true,
                    Data = new List<GatePassHdrDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllGatePassAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllGatePassQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGatePassByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GatePassHdrDto());

            var result = await CreateSut().GetGatePassByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDocTypes_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGatePassDocTypesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionTypeLookupDto>());

            var result = await CreateSut().GetGatePassDocTypesAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDocTypes_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGatePassDocTypesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TransactionTypeLookupDto>());

            await CreateSut().GetGatePassDocTypesAsync();

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetGatePassDocTypesQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGatePassAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GatePassAutoCompleteDto>() as IReadOnlyList<GatePassAutoCompleteDto>);

            var result = await CreateSut().GetGatePassAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateGatePassCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Gate Pass created successfully.",
                    Data = 1
                });

            var result = await CreateSut().CreateGatePass(new CreateGatePassCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGatePassCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteGatePass(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGatePassCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteGatePass(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteGatePassCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
