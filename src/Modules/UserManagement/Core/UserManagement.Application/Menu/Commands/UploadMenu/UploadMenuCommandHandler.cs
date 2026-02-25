using AutoMapper;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Common.Interfaces.IModule;
using MediatR;
using OfficeOpenXml;

namespace UserManagement.Application.Menu.Commands.UploadMenu
{
    public class UploadMenuCommandHandler : IRequestHandler<UploadMenuCommand, string>
    {
        private readonly IModuleQueryRepository _moduleQueryRepository;
        private readonly IMapper _imapper;
        private readonly IMenuCommand _menuCommand;
        private readonly IMenuQuery _menuQuery;
        public UploadMenuCommandHandler(IModuleQueryRepository moduleQueryRepository, IMapper imapper, IMenuCommand menuCommand, IMenuQuery menuQuery)
        {
            _moduleQueryRepository = moduleQueryRepository;
            _imapper = imapper;
            _menuCommand = menuCommand;
            _menuQuery = menuQuery;
        }
        public async Task<string> Handle(UploadMenuCommand request, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            await request.File.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var ParentWorkSheet = package.Workbook.Worksheets[0];
            var ChildWorkSheet = package.Workbook.Worksheets[1];

            int rowCount = ParentWorkSheet.Dimension.Rows;
            int ChildRowCount = ChildWorkSheet.Dimension.Rows;

            List<UploadMenuDto> MenuDtos = new List<UploadMenuDto>();
            for (int row = 2; row <= rowCount; row++)
            {
                string MenuName = ParentWorkSheet.Cells[row, 1].Text.Trim();
                string ModuleName = ParentWorkSheet.Cells[row, 2].Text.Trim();
                var module = await _moduleQueryRepository.GetModuleByNameAsync(ModuleName);

                var MenuDto = new UploadMenuDto
                {
                    MenuName = MenuName,
                    ModuleId = module.Id,
                    MenuUrl = ParentWorkSheet.Cells[row, 3].Text.Trim(),
                    ParentId = 0,
                    SortOrder = int.Parse(ParentWorkSheet.Cells[row, 5].Text.Trim())
                };
                MenuDtos.Add(MenuDto);
            }
            var ParentMenu = _imapper.Map<List<UserManagement.Domain.Entities.Menu>>(MenuDtos);
            var result = await _menuCommand.BulkImportMenuAsync(ParentMenu);

            List<UploadMenuDto> ChildMenuDtos = new List<UploadMenuDto>();

            for (int row = 2; row <= ChildRowCount; row++)
            {
                string MenuName = ChildWorkSheet.Cells[row, 1].Text.Trim();
                string ModuleName = ChildWorkSheet.Cells[row, 2].Text.Trim();
                int SortOrder = int.Parse(ChildWorkSheet.Cells[row, 5].Text.Trim());
                var ParentMenuName = await _menuQuery.GetMenuByNameAsync(ChildWorkSheet.Cells[row, 4].Text.Trim());
                var module = await _moduleQueryRepository.GetModuleByNameAsync(ModuleName);

                var ChildMenuDto = new UploadMenuDto
                {
                    MenuName = MenuName,
                    ModuleId = module.Id,
                    MenuUrl = ParentWorkSheet.Cells[row, 3].Text.Trim(),
                    ParentId = ParentMenuName.Id,
                    SortOrder = SortOrder
                };
                ChildMenuDtos.Add(ChildMenuDto);
            }
            var ChildMenu = _imapper.Map<List<UserManagement.Domain.Entities.Menu>>(ChildMenuDtos);
            var ChildMenuresult = await _menuCommand.BulkImportMenuAsync(ChildMenu);

             return "Upload successful.";
        }
    }
}