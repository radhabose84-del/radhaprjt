using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.TestData
{
    public static class StateBuilders
    {
        public static CreateStateCommand ValidCreateCommand(
            string? stateCode = "MH",
            string? stateName = "Maharashtra",
            int countryId = 1) =>
            new CreateStateCommand
            {
                StateCode = stateCode,
                StateName = stateName,
                CountryId = countryId
            };

        public static UpdateStateCommand ValidUpdateCommand(
            int id = 1,
            string? stateCode = "MH",
            string? stateName = "Maharashtra Updated",
            int countryId = 1,
            byte isActive = 1) =>
            new UpdateStateCommand
            {
                Id = id,
                StateCode = stateCode,
                StateName = stateName,
                CountryId = countryId,
                IsActive = isActive
            };

        public static DeleteStateCommand ValidDeleteCommand(int id = 1) =>
            new DeleteStateCommand { Id = id };

        public static StateDto ValidDto(
            int id = 1,
            string? stateCode = "MH",
            string? stateName = "Maharashtra",
            int countryId = 1) =>
            new StateDto
            {
                Id = id,
                StateCode = stateCode,
                StateName = stateName,
                CountryId = countryId,
                IsActive = Status.Active
            };

        public static States ValidEntity(
            int id = 1,
            string? stateCode = "MH",
            string? stateName = "Maharashtra",
            int countryId = 1) =>
            new States
            {
                Id = id,
                StateCode = stateCode,
                StateName = stateName,
                CountryId = countryId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static StateAutoCompleteDTO ValidAutoCompleteDto(
            int id = 1,
            string? stateCode = "MH",
            string? stateName = "Maharashtra") =>
            new StateAutoCompleteDTO
            {
                Id = id,
                StateCode = stateCode,
                StateName = stateName
            };
    }
}
