using Dapper;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;

namespace FinanceManagement.IntegrationTests.Repositories.FinancialYearMaster
{
    /// <summary>
    /// Helpers that seed the MiscTypeMaster + MiscMaster status rows that the FinancialYearMaster
    /// / PeriodStatusOverride tables FK to. Called per test via the DbFixture connection.
    /// </summary>
    internal static class IntegrationTestSeeder
    {
        public static async Task<(int FysOpenId, int FysClosedId, int FpsOpenId, int FpsSoftClosedId, int FpsHardClosedId,
                                  int PsoPendingId, int PsoFullyApprovedId, int PsoAppliedId, int PsoRejectedId)>
            SeedStatusRowsAsync(DbFixture fixture)
        {
            await using var conn = new SqlConnection(fixture.ConnectionString);
            await conn.OpenAsync();

            // Insert MiscTypeMaster rows (FYS, FPS, PSO) if not present
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscTypeMaster WHERE MiscTypeCode = 'FYS' AND IsDeleted = 0)
                    INSERT INTO Finance.MiscTypeMaster (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES ('FYS', 'Financial Year Status', 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');

                IF NOT EXISTS (SELECT 1 FROM Finance.MiscTypeMaster WHERE MiscTypeCode = 'FPS' AND IsDeleted = 0)
                    INSERT INTO Finance.MiscTypeMaster (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES ('FPS', 'Financial Period Status', 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');

                IF NOT EXISTS (SELECT 1 FROM Finance.MiscTypeMaster WHERE MiscTypeCode = 'PSO' AND IsDeleted = 0)
                    INSERT INTO Finance.MiscTypeMaster (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES ('PSO', 'Period Status Override State', 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
            ");

            // Insert MiscMaster value rows
            await conn.ExecuteAsync(@"
                DECLARE @FysId INT = (SELECT TOP 1 Id FROM Finance.MiscTypeMaster WHERE MiscTypeCode='FYS' AND IsDeleted=0);
                DECLARE @FpsId INT = (SELECT TOP 1 Id FROM Finance.MiscTypeMaster WHERE MiscTypeCode='FPS' AND IsDeleted=0);
                DECLARE @PsoId INT = (SELECT TOP 1 Id FROM Finance.MiscTypeMaster WHERE MiscTypeCode='PSO' AND IsDeleted=0);

                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@FysId AND Code='OPEN'   AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@FysId, 'OPEN',   'Open',   1, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@FysId AND Code='CLOSED' AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@FysId, 'CLOSED', 'Closed', 2, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');

                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@FpsId AND Code='OPEN'       AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@FpsId, 'OPEN',       'Open',        1, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@FpsId AND Code='SOFTCLOSED' AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@FpsId, 'SOFTCLOSED', 'Soft Closed', 2, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@FpsId AND Code='HARDCLOSED' AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@FpsId, 'HARDCLOSED', 'Hard Closed', 3, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');

                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@PsoId AND Code='PENDING'       AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@PsoId, 'PENDING',       'Pending',        1, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@PsoId AND Code='FULLYAPPROVED' AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@PsoId, 'FULLYAPPROVED', 'Fully Approved', 2, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@PsoId AND Code='APPLIED'       AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@PsoId, 'APPLIED',       'Applied',        3, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
                IF NOT EXISTS (SELECT 1 FROM Finance.MiscMaster WHERE MiscTypeId=@PsoId AND Code='REJECTED'      AND IsDeleted=0)
                    INSERT INTO Finance.MiscMaster (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP)
                    VALUES (@PsoId, 'REJECTED',      'Rejected',       4, 1, 0, 0, SYSDATETIMEOFFSET(), 'System', '127.0.0.1');
            ");

            // Return the resolved ids
            var ids = await conn.QueryFirstAsync<(int FysOpenId, int FysClosedId, int FpsOpenId, int FpsSoftClosedId, int FpsHardClosedId,
                                                  int PsoPendingId, int PsoFullyApprovedId, int PsoAppliedId, int PsoRejectedId)>(@"
                SELECT
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='FYS' AND mm.Code='OPEN'   AND mm.IsDeleted=0) AS FysOpenId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='FYS' AND mm.Code='CLOSED' AND mm.IsDeleted=0) AS FysClosedId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='FPS' AND mm.Code='OPEN'       AND mm.IsDeleted=0) AS FpsOpenId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='FPS' AND mm.Code='SOFTCLOSED' AND mm.IsDeleted=0) AS FpsSoftClosedId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='FPS' AND mm.Code='HARDCLOSED' AND mm.IsDeleted=0) AS FpsHardClosedId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='PSO' AND mm.Code='PENDING'       AND mm.IsDeleted=0) AS PsoPendingId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='PSO' AND mm.Code='FULLYAPPROVED' AND mm.IsDeleted=0) AS PsoFullyApprovedId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='PSO' AND mm.Code='APPLIED'       AND mm.IsDeleted=0) AS PsoAppliedId,
                    (SELECT TOP 1 mm.Id FROM Finance.MiscMaster mm
                        JOIN Finance.MiscTypeMaster mt ON mt.Id = mm.MiscTypeId WHERE mt.MiscTypeCode='PSO' AND mm.Code='REJECTED'      AND mm.IsDeleted=0) AS PsoRejectedId
            ");

            return ids;
        }
    }
}
