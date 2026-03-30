using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class UOMConversionBuilders
    {
        public static CreateUOMConversionCommand ValidCreateCommand(
            int fromUOMId = 1,
            int toUOMId = 2,
            decimal conversionValue = 1000m) =>
            new CreateUOMConversionCommand
            {
                FromUOMId = fromUOMId,
                ToUOMId = toUOMId,
                ConversionValue = conversionValue
            };

        public static UpdateUOMConversionCommand ValidUpdateCommand(
            int id = 1,
            int fromUOMId = 1,
            int toUOMId = 2,
            decimal conversionValue = 1000m) =>
            new UpdateUOMConversionCommand
            {
                Id = id,
                FromUOMId = fromUOMId,
                ToUOMId = toUOMId,
                ConversionValue = conversionValue,
                IsActive = 1
            };

        public static DeleteUOMConversionCommand ValidDeleteCommand(int id = 1) =>
            new DeleteUOMConversionCommand { Id = id };

        public static UOMConversionDto ValidDto(int id = 1) =>
            new UOMConversionDto
            {
                Id = id,
                FromUOMId = 1,
                FromUOMCode = "KG",
                ToUOMId = 2,
                ToUOMCode = "G",
                ConversionValue = 1000m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static InventoryManagement.Domain.Entities.UOMConversion ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.UOMConversion
            {
                Id = id,
                FromUOMId = 1,
                ToUOMId = 2,
                ConversionValue = 1000m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
