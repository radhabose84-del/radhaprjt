using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using OfficeOpenXml;

namespace FAM.Application.ExcelImport
{
    public class AssetSpecificationHandler
    {
        private readonly ExcelWorksheet _worksheet;
        private readonly int _row;

        public AssetSpecificationHandler(ExcelWorksheet worksheet, int row)
        {
            _worksheet = worksheet;
            _row = row;
        }

        public List<AssetSpecificationCombineDto> ProcessSpecifications()
        {
            var specifications = new List<AssetSpecificationCombineDto>();

            AddSpecification(specifications, 16, 7);   // Make
            AddSpecification(specifications, 17, 9);   // Model Number
            AddSpecification(specifications, 18, 8);   // Serial Number
            AddSpecification(specifications, 19, 11);  // Capacity
            AddSpecification(specifications, 20, 4);   // UOM
            AddSpecification(specifications, 21, 12);  // Power
            AddSpecification(specifications, 22, 13);  // RPM
            AddSpecification(specifications, 23, 14);  // DOI
            AddSpecification(specifications, 24, 15);  // Product Specification
            AddSpecification(specifications, 25, 16);  // Size
            AddSpecification(specifications, 26, 17);  // Weight
            AddSpecification(specifications, 27, 18);  // Color
            AddSpecification(specifications, 28, 19);  // Engine Number
            AddSpecification(specifications, 29, 20);  // Chasis Number
            AddSpecification(specifications, 30, 21);  // Vehicle Number
            AddSpecification(specifications, 31, 22);  // Type Of Building
            AddSpecification(specifications, 32, 23);  // No Of Rooms
            AddSpecification(specifications, 33, 24);  // Ownership
            AddSpecification(specifications, 34, 25);  // Year Of Construction
            AddSpecification(specifications, 35, 26);  // No Of Floors

            return specifications;
        }

        private void AddSpecification(List<AssetSpecificationCombineDto> specifications, int columnIndex, int specificationId)
        {
            string? value = _worksheet.Cells[_row, columnIndex].Value?.ToString()?.Trim();

            if (!string.IsNullOrWhiteSpace(value))
            {
                specifications.Add(new AssetSpecificationCombineDto
                {
                    SpecificationId = specificationId,
                    SpecificationValue = value
                });
            }
        }
    }
}
