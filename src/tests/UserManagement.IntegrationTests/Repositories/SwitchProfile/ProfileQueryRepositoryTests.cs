using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using UserManagement.Infrastructure.Repositories.Profile;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.SwitchProfile
{
    [Collection("DatabaseCollection")]
    public sealed class ProfileQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private string ConnectionString => _fixture.ConnectionString;

        public ProfileQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SqlConnection OpenSql() => new SqlConnection(ConnectionString);

        private ProfileQueryRepository CreateRepository()
        {
            var conn = new SqlConnection(ConnectionString);
            return new ProfileQueryRepository(conn);
        }

        private async Task<int> SeedCompanyDivisionUnitDepartmentUserAsync()
        {
            await using var cnn = OpenSql();
            await cnn.OpenAsync();
            await using var tx = await cnn.BeginTransactionAsync();

            // NOTE:
            // - This seed DOES NOT assume fixed PK column names (Id/CompanyId/etc.)
            // - It creates Company/Division/Unit/Department rows if needed
            // - It sets FK columns (CompanyId/DivisionId) on Unit even if they are nullable
            // - It forces IsActive=1 and IsDeleted/IsDelete=0 where columns exist
            var sql = @"
------------------------------------------------------------
-- 0) Disable FK Users -> Entity (test only)
------------------------------------------------------------
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Users_Entity_EntityId'
      AND parent_object_id = OBJECT_ID('AppSecurity.Users')
)
BEGIN
    ALTER TABLE AppSecurity.Users NOCHECK CONSTRAINT FK_Users_Entity_EntityId;
END;

------------------------------------------------------------
-- 1) Cleanup old test user
------------------------------------------------------------
DELETE FROM AppSecurity.UserUnit WHERE UserId = 1001;
DELETE FROM AppSecurity.Users    WHERE UserId = 1001;

------------------------------------------------------------
-- Helpers (inline)
------------------------------------------------------------
DECLARE @CompanyTable sysname = N'AppData.Company';
DECLARE @DivisionTable sysname = N'AppData.Division';
DECLARE @UnitTable sysname = N'AppData.Unit';
DECLARE @DeptTable sysname = N'AppData.Department';

IF OBJECT_ID(@CompanyTable) IS NULL THROW 52001, 'AppData.Company table not found.', 1;
IF OBJECT_ID(@DivisionTable) IS NULL THROW 52002, 'AppData.Division table not found.', 1;
IF OBJECT_ID(@UnitTable)     IS NULL THROW 52003, 'AppData.Unit table not found.', 1;
IF OBJECT_ID(@DeptTable)     IS NULL THROW 52004, 'AppData.Department table not found.', 1;

------------------------------------------------------------
-- Disable all FKs on involved AppData tables (test only)
------------------------------------------------------------
DECLARE @fk nvarchar(max) = N'';
SELECT @fk = @fk + N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) +
             N' NOCHECK CONSTRAINT [' + fk.name + N'];' + CHAR(10)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id = fk.parent_object_id
WHERE fk.parent_object_id IN (OBJECT_ID(@CompanyTable), OBJECT_ID(@DivisionTable), OBJECT_ID(@UnitTable), OBJECT_ID(@DeptTable));
IF (@fk <> N'') EXEC sp_executesql @fk;

------------------------------------------------------------
-- Get PK column helper (Company/Division/Unit/Dept)
------------------------------------------------------------
DECLARE @CompanyPk sysname =
(
    SELECT TOP 1 c.name
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE i.object_id = OBJECT_ID(@CompanyTable) AND i.is_primary_key=1
    ORDER BY ic.key_ordinal
);

DECLARE @DivisionPk sysname =
(
    SELECT TOP 1 c.name
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE i.object_id = OBJECT_ID(@DivisionTable) AND i.is_primary_key=1
    ORDER BY ic.key_ordinal
);

DECLARE @UnitPk sysname =
(
    SELECT TOP 1 c.name
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE i.object_id = OBJECT_ID(@UnitTable) AND i.is_primary_key=1
    ORDER BY ic.key_ordinal
);

DECLARE @DeptPk sysname =
(
    SELECT TOP 1 c.name
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE i.object_id = OBJECT_ID(@DeptTable) AND i.is_primary_key=1
    ORDER BY ic.key_ordinal
);

IF @CompanyPk IS NULL THROW 52005, 'Could not detect PK for AppData.Company.', 1;
IF @DivisionPk IS NULL THROW 52006, 'Could not detect PK for AppData.Division.', 1;
IF @UnitPk IS NULL     THROW 52007, 'Could not detect PK for AppData.Unit.', 1;
IF @DeptPk IS NULL     THROW 52008, 'Could not detect PK for AppData.Department.', 1;

------------------------------------------------------------
-- Detect FK column names:
-- Division -> Company, Unit -> Division, Unit -> Company, Department -> Unit
------------------------------------------------------------
DECLARE @DivCompanyFk sysname =
(
    SELECT TOP 1 pc.name
    FROM sys.foreign_key_columns fkc
    JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
    WHERE fkc.parent_object_id = OBJECT_ID(@DivisionTable)
      AND fkc.referenced_object_id = OBJECT_ID(@CompanyTable)
);

IF @DivCompanyFk IS NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DivisionTable) AND name='CompanyId') SET @DivCompanyFk='CompanyId';
END

DECLARE @UnitDivisionFk sysname =
(
    SELECT TOP 1 pc.name
    FROM sys.foreign_key_columns fkc
    JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
    WHERE fkc.parent_object_id = OBJECT_ID(@UnitTable)
      AND fkc.referenced_object_id = OBJECT_ID(@DivisionTable)
);

IF @UnitDivisionFk IS NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@UnitTable) AND name='DivisionId') SET @UnitDivisionFk='DivisionId';
END

DECLARE @UnitCompanyFk sysname =
(
    SELECT TOP 1 pc.name
    FROM sys.foreign_key_columns fkc
    JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
    WHERE fkc.parent_object_id = OBJECT_ID(@UnitTable)
      AND fkc.referenced_object_id = OBJECT_ID(@CompanyTable)
);

IF @UnitCompanyFk IS NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@UnitTable) AND name='CompanyId') SET @UnitCompanyFk='CompanyId';
END

DECLARE @DeptUnitFk sysname =
(
    SELECT TOP 1 pc.name
    FROM sys.foreign_key_columns fkc
    JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
    WHERE fkc.parent_object_id = OBJECT_ID(@DeptTable)
      AND fkc.referenced_object_id = OBJECT_ID(@UnitTable)
);

IF @DeptUnitFk IS NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DeptTable) AND name='UnitId') SET @DeptUnitFk='UnitId';
    ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DeptTable) AND name='UnitID') SET @DeptUnitFk='UnitID';
END

------------------------------------------------------------
-- Utility: force IsActive=1 and IsDeleted/IsDelete=0 if columns exist (even nullable)
------------------------------------------------------------
DECLARE @forceFlags nvarchar(max);

-- Company flags
SET @forceFlags = N'';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@CompanyTable) AND name='IsActive')
    SET @forceFlags += N'UPDATE ' + @CompanyTable + N' SET IsActive=1 WHERE IsActive IS NULL OR IsActive<>1;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@CompanyTable) AND name='IsDeleted')
    SET @forceFlags += N'UPDATE ' + @CompanyTable + N' SET IsDeleted=0 WHERE IsDeleted IS NULL OR IsDeleted<>0;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@CompanyTable) AND name='IsDelete')
    SET @forceFlags += N'UPDATE ' + @CompanyTable + N' SET IsDelete=0 WHERE IsDelete IS NULL OR IsDelete<>0;' + CHAR(10);

-- Division flags
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DivisionTable) AND name='IsActive')
    SET @forceFlags += N'UPDATE ' + @DivisionTable + N' SET IsActive=1 WHERE IsActive IS NULL OR IsActive<>1;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DivisionTable) AND name='IsDeleted')
    SET @forceFlags += N'UPDATE ' + @DivisionTable + N' SET IsDeleted=0 WHERE IsDeleted IS NULL OR IsDeleted<>0;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DivisionTable) AND name='IsDelete')
    SET @forceFlags += N'UPDATE ' + @DivisionTable + N' SET IsDelete=0 WHERE IsDelete IS NULL OR IsDelete<>0;' + CHAR(10);

-- Unit flags
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@UnitTable) AND name='IsActive')
    SET @forceFlags += N'UPDATE ' + @UnitTable + N' SET IsActive=1 WHERE IsActive IS NULL OR IsActive<>1;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@UnitTable) AND name='IsDeleted')
    SET @forceFlags += N'UPDATE ' + @UnitTable + N' SET IsDeleted=0 WHERE IsDeleted IS NULL OR IsDeleted<>0;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@UnitTable) AND name='IsDelete')
    SET @forceFlags += N'UPDATE ' + @UnitTable + N' SET IsDelete=0 WHERE IsDelete IS NULL OR IsDelete<>0;' + CHAR(10);

-- Department flags
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DeptTable) AND name='IsActive')
    SET @forceFlags += N'UPDATE ' + @DeptTable + N' SET IsActive=1 WHERE IsActive IS NULL OR IsActive<>1;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DeptTable) AND name='IsDeleted')
    SET @forceFlags += N'UPDATE ' + @DeptTable + N' SET IsDeleted=0 WHERE IsDeleted IS NULL OR IsDeleted<>0;' + CHAR(10);

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(@DeptTable) AND name='IsDelete')
    SET @forceFlags += N'UPDATE ' + @DeptTable + N' SET IsDelete=0 WHERE IsDelete IS NULL OR IsDelete<>0;' + CHAR(10);

IF (@forceFlags <> N'') EXEC sp_executesql @forceFlags;

------------------------------------------------------------
-- 2) Ensure Company exists and pick CompanyId
------------------------------------------------------------
DECLARE @companyId int = NULL;

IF NOT EXISTS (SELECT 1 FROM AppData.Company)
BEGIN
    DECLARE @colsC nvarchar(max)=N'', @valsC nvarchar(max)=N'';

    ;WITH req AS
    (
        SELECT c.name ColName, t.name TypeName, c.is_nullable, c.is_identity, c.is_computed, dc.definition DefaultDef
        FROM sys.columns c
        JOIN sys.types t ON t.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
        WHERE c.object_id=OBJECT_ID(@CompanyTable)
          AND c.is_nullable=0 AND c.is_identity=0 AND c.is_computed=0
          AND t.name NOT IN ('timestamp','rowversion')
          AND dc.definition IS NULL
    )
    SELECT
        @colsC = STRING_AGG(QUOTENAME(ColName), N','),
        @valsC = STRING_AGG(
            CASE
                WHEN ColName LIKE '%CompanyName%' OR ColName LIKE '%Name%' THEN N'''ProfileCo'''
                WHEN ColName LIKE '%Legal%' THEN N'''ProfileCo Pvt Ltd'''
                WHEN ColName LIKE '%IsActive%' THEN N'1'
                WHEN ColName LIKE '%IsDelete%' OR ColName LIKE '%IsDeleted%' THEN N'0'
                WHEN TypeName IN ('int','bigint','smallint','tinyint') THEN N'1'
                WHEN TypeName='bit' THEN N'0'
                WHEN TypeName IN ('datetime','datetime2','smalldatetime') THEN N'SYSUTCDATETIME()'
                WHEN TypeName='date' THEN N'CONVERT(date, SYSUTCDATETIME())'
                WHEN TypeName='uniqueidentifier' THEN N'NEWID()'
                WHEN TypeName IN ('char','nchar','varchar','nvarchar','text','ntext') THEN N'''seed'''
                WHEN TypeName IN ('varbinary','binary','image') THEN N'0x'
                ELSE N'''seed'''
            END
        , N',')
    FROM req;

    DECLARE @insC nvarchar(max)=N'INSERT INTO AppData.Company ('+@colsC+') VALUES ('+@valsC+');';
    EXEC sp_executesql @insC;
END

DECLARE @pickC nvarchar(max)=N'SELECT TOP 1 @out = CAST(['+@CompanyPk+'] as int) FROM AppData.Company ORDER BY ['+@CompanyPk+'] DESC;';
EXEC sp_executesql @pickC, N'@out int OUTPUT', @out=@companyId OUTPUT;

IF @companyId IS NULL THROW 52020, 'Could not pick CompanyId.', 1;

------------------------------------------------------------
-- 3) Ensure Division exists and references Company
------------------------------------------------------------
DECLARE @divisionId int = NULL;

IF NOT EXISTS (SELECT 1 FROM AppData.Division)
BEGIN
    DECLARE @colsV nvarchar(max)=N'', @valsV nvarchar(max)=N'';

    ;WITH req AS
    (
        SELECT c.name ColName, t.name TypeName, c.is_nullable, c.is_identity, c.is_computed, dc.definition DefaultDef
        FROM sys.columns c
        JOIN sys.types t ON t.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
        WHERE c.object_id=OBJECT_ID(@DivisionTable)
          AND c.is_nullable=0 AND c.is_identity=0 AND c.is_computed=0
          AND t.name NOT IN ('timestamp','rowversion')
          AND dc.definition IS NULL
    )
    SELECT
        @colsV = STRING_AGG(QUOTENAME(ColName), N','),
        @valsV = STRING_AGG(
            CASE
                WHEN @DivCompanyFk IS NOT NULL AND ColName=@DivCompanyFk THEN CAST(@companyId as nvarchar(50))
                WHEN ColName LIKE '%Name%' THEN N'''Profile Division'''
                WHEN ColName LIKE '%ShortName%' THEN N'''PD'''
                WHEN ColName LIKE '%IsActive%' THEN N'1'
                WHEN ColName LIKE '%IsDelete%' OR ColName LIKE '%IsDeleted%' THEN N'0'
                WHEN TypeName IN ('int','bigint','smallint','tinyint') THEN N'1'
                WHEN TypeName='bit' THEN N'0'
                WHEN TypeName IN ('datetime','datetime2','smalldatetime') THEN N'SYSUTCDATETIME()'
                WHEN TypeName='date' THEN N'CONVERT(date, SYSUTCDATETIME())'
                WHEN TypeName='uniqueidentifier' THEN N'NEWID()'
                WHEN TypeName IN ('char','nchar','varchar','nvarchar','text','ntext') THEN N'''seed'''
                WHEN TypeName IN ('varbinary','binary','image') THEN N'0x'
                ELSE N'''seed'''
            END
        , N',')
    FROM req;

    DECLARE @insV nvarchar(max)=N'INSERT INTO AppData.Division ('+@colsV+') VALUES ('+@valsV+');';
    EXEC sp_executesql @insV;
END

DECLARE @pickV nvarchar(max)=N'SELECT TOP 1 @out = CAST(['+@DivisionPk+'] as int) FROM AppData.Division ORDER BY ['+@DivisionPk+'] DESC;';
EXEC sp_executesql @pickV, N'@out int OUTPUT', @out=@divisionId OUTPUT;

IF @divisionId IS NULL THROW 52030, 'Could not pick DivisionId.', 1;

------------------------------------------------------------
-- 4) Ensure Unit exists and references Division/Company (even if nullable)
------------------------------------------------------------
DECLARE @unitId int = NULL;

IF NOT EXISTS (SELECT 1 FROM AppData.Unit)
BEGIN
    DECLARE @colsU nvarchar(max)=N'', @valsU nvarchar(max)=N'';

    ;WITH req AS
    (
        SELECT c.name ColName, t.name TypeName, c.is_nullable, c.is_identity, c.is_computed, dc.definition DefaultDef
        FROM sys.columns c
        JOIN sys.types t ON t.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
        WHERE c.object_id=OBJECT_ID(@UnitTable)
          AND c.is_nullable=0 AND c.is_identity=0 AND c.is_computed=0
          AND t.name NOT IN ('timestamp','rowversion')
          AND dc.definition IS NULL
    )
    SELECT
        @colsU = STRING_AGG(QUOTENAME(ColName), N','),
        @valsU = STRING_AGG(
            CASE
                WHEN @UnitDivisionFk IS NOT NULL AND ColName=@UnitDivisionFk THEN CAST(@divisionId as nvarchar(50))
                WHEN @UnitCompanyFk  IS NOT NULL AND ColName=@UnitCompanyFk  THEN CAST(@companyId as nvarchar(50))
                WHEN ColName LIKE '%UnitName%' THEN N'''Profile Unit'''
                WHEN ColName LIKE '%ShortName%' THEN N'''PU'''
                WHEN ColName LIKE '%Head%' THEN N'''Profile Head'''
                WHEN ColName LIKE '%IsActive%' THEN N'1'
                WHEN ColName LIKE '%IsDelete%' OR ColName LIKE '%IsDeleted%' THEN N'0'
                WHEN TypeName IN ('int','bigint','smallint','tinyint') THEN N'1'
                WHEN TypeName='bit' THEN N'0'
                WHEN TypeName IN ('datetime','datetime2','smalldatetime') THEN N'SYSUTCDATETIME()'
                WHEN TypeName='date' THEN N'CONVERT(date, SYSUTCDATETIME())'
                WHEN TypeName='uniqueidentifier' THEN N'NEWID()'
                WHEN TypeName IN ('char','nchar','varchar','nvarchar','text','ntext') THEN N'''seed'''
                WHEN TypeName IN ('varbinary','binary','image') THEN N'0x'
                ELSE N'''seed'''
            END
        , N',')
    FROM req;

    DECLARE @insU nvarchar(max)=N'INSERT INTO AppData.Unit ('+@colsU+') VALUES ('+@valsU+');';
    EXEC sp_executesql @insU;
END

DECLARE @pickU nvarchar(max)=N'SELECT TOP 1 @out = CAST(['+@UnitPk+'] as int) FROM AppData.Unit ORDER BY ['+@UnitPk+'] DESC;';
EXEC sp_executesql @pickU, N'@out int OUTPUT', @out=@unitId OUTPUT;

IF @unitId IS NULL THROW 52040, 'Could not pick UnitId.', 1;

-- Force FK columns for joins even if columns are nullable
IF @UnitDivisionFk IS NOT NULL
BEGIN
    DECLARE @updUD nvarchar(max) = N'UPDATE AppData.Unit SET ['+@UnitDivisionFk+']=@d WHERE ['+@UnitDivisionFk+'] IS NULL;';
    EXEC sp_executesql @updUD, N'@d int', @d=@divisionId;
END

IF @UnitCompanyFk IS NOT NULL
BEGIN
    DECLARE @updUC nvarchar(max) = N'UPDATE AppData.Unit SET ['+@UnitCompanyFk+']=@c WHERE ['+@UnitCompanyFk+'] IS NULL;';
    EXEC sp_executesql @updUC, N'@c int', @c=@companyId;
END

------------------------------------------------------------
-- 5) Ensure Department exists for Unit (needed for Users.DepartmentId)
------------------------------------------------------------
DECLARE @departmentId int = NULL;

IF @DeptUnitFk IS NOT NULL
BEGIN
    DECLARE @pickDbyU nvarchar(max)=
        N'SELECT TOP 1 @out = CAST(['+@DeptPk+'] as int)
          FROM AppData.Department
          WHERE ['+@DeptUnitFk+'] = @u
          ORDER BY ['+@DeptPk+'] DESC;';
    EXEC sp_executesql @pickDbyU, N'@u int, @out int OUTPUT', @u=@unitId, @out=@departmentId OUTPUT;
END

IF @departmentId IS NULL
BEGIN
    DECLARE @colsD nvarchar(max)=N'', @valsD nvarchar(max)=N'';

    ;WITH req AS
    (
        SELECT c.name ColName, t.name TypeName, c.is_nullable, c.is_identity, c.is_computed, dc.definition DefaultDef
        FROM sys.columns c
        JOIN sys.types t ON t.user_type_id=c.user_type_id
        LEFT JOIN sys.default_constraints dc ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
        WHERE c.object_id=OBJECT_ID(@DeptTable)
          AND c.is_nullable=0 AND c.is_identity=0 AND c.is_computed=0
          AND t.name NOT IN ('timestamp','rowversion')
          AND dc.definition IS NULL
    )
    SELECT
        @colsD = STRING_AGG(QUOTENAME(ColName), N','),
        @valsD = STRING_AGG(
            CASE
                WHEN @DeptUnitFk IS NOT NULL AND ColName=@DeptUnitFk THEN CAST(@unitId as nvarchar(50))
                WHEN ColName LIKE '%DepartmentName%' OR ColName LIKE '%DeptName%' THEN N'''Profile Dept'''
                WHEN ColName LIKE '%IsActive%' THEN N'1'
                WHEN ColName LIKE '%IsDelete%' OR ColName LIKE '%IsDeleted%' THEN N'0'
                WHEN TypeName IN ('int','bigint','smallint','tinyint') THEN N'1'
                WHEN TypeName='bit' THEN N'0'
                WHEN TypeName IN ('datetime','datetime2','smalldatetime') THEN N'SYSUTCDATETIME()'
                WHEN TypeName='date' THEN N'CONVERT(date, SYSUTCDATETIME())'
                WHEN TypeName='uniqueidentifier' THEN N'NEWID()'
                WHEN TypeName IN ('char','nchar','varchar','nvarchar','text','ntext') THEN N'''seed'''
                WHEN TypeName IN ('varbinary','binary','image') THEN N'0x'
                ELSE N'''seed'''
            END
        , N',')
    FROM req;

    DECLARE @insD nvarchar(max)=N'INSERT INTO AppData.Department ('+@colsD+') VALUES ('+@valsD+');';
    EXEC sp_executesql @insD;

    IF @DeptUnitFk IS NOT NULL
    BEGIN
        DECLARE @pickDbyU2 nvarchar(max)=
            N'SELECT TOP 1 @out = CAST(['+@DeptPk+'] as int)
              FROM AppData.Department
              WHERE ['+@DeptUnitFk+'] = @u
              ORDER BY ['+@DeptPk+'] DESC;';
        EXEC sp_executesql @pickDbyU2, N'@u int, @out int OUTPUT', @u=@unitId, @out=@departmentId OUTPUT;
    END
    ELSE
    BEGIN
        DECLARE @pickDany nvarchar(max)=
            N'SELECT TOP 1 @out = CAST(['+@DeptPk+'] as int)
              FROM AppData.Department
              ORDER BY ['+@DeptPk+'] DESC;';
        EXEC sp_executesql @pickDany, N'@out int OUTPUT', @out=@departmentId OUTPUT;
    END
END

IF @departmentId IS NULL THROW 52050, 'Could not pick/create DepartmentId.', 1;

------------------------------------------------------------
-- 6) Insert User (DepartmentId NOT NULL)
------------------------------------------------------------
SET IDENTITY_INSERT AppSecurity.Users ON;

INSERT INTO AppSecurity.Users
(
    UserId,
    FirstName,
    LastName,
    UserName,
    DepartmentId,
    IsActive,
    PasswordHash,
    UserType,
    Mobile,
    EmailId,
    IsFirstTimeUser,
    IsDeleted,
    UserGroupId,
    EntityId,
    PartyId,
    CreatedAt,
    CreatedBy,
    CreatedByName,
    CreatedIp,
    IsLocked
)
VALUES
(
    1001,
    'Profile',
    'User',
    'profileuser',
    @departmentId,
    1,
    'x',
    0,
    '9999999999',
    'profile@e.com',
    0,
    0,
    NULL,
    1,
    NULL,
    SYSUTCDATETIME(),
    1,
    'seed',
    '127.0.0.1',
    0
);

SET IDENTITY_INSERT AppSecurity.Users OFF;

------------------------------------------------------------
-- 7) Insert UserUnit (set IsActive=1 + IsDeleted/IsDelete=0 if exists)
------------------------------------------------------------
DECLARE @uuDeleteCol sysname =
(
    SELECT TOP 1 name
    FROM sys.columns
    WHERE object_id = OBJECT_ID('AppSecurity.UserUnit')
      AND name IN ('IsDeleted','IsDelete','Is_Deleted','Is_Delete')
);

IF @uuDeleteCol IS NULL
BEGIN
    INSERT INTO AppSecurity.UserUnit (UserId, UnitId, IsActive)
    VALUES (1001, @unitId, 1);
END
ELSE
BEGIN
    DECLARE @insUU nvarchar(max)=
        N'INSERT INTO AppSecurity.UserUnit (UserId, UnitId, IsActive, ['+@uuDeleteCol+N'])
          VALUES (1001, @u, 1, 0);';
    EXEC sp_executesql @insUU, N'@u int', @u=@unitId;
END

-- Force UserUnit delete flag = 0 if column exists and is NULL (very common reason of empty results)
IF @uuDeleteCol IS NOT NULL
BEGIN
    DECLARE @updUU nvarchar(max)=
        N'UPDATE AppSecurity.UserUnit SET ['+@uuDeleteCol+N']=0
          WHERE UserId=1001 AND UnitId=@u AND (['+@uuDeleteCol+N'] IS NULL OR ['+@uuDeleteCol+N']<>0);';
    EXEC sp_executesql @updUU, N'@u int', @u=@unitId;
END

SELECT @unitId;
";

            try
            {
                var unitId = await cnn.ExecuteScalarAsync<int>(sql, transaction: tx);
                await tx.CommitAsync();
                return unitId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        [Fact]
        public async Task GetUnit_Should_Return_Active_Units_For_Given_User()
        {
            var unitId = await SeedCompanyDivisionUnitDepartmentUserAsync();
            var repo = CreateRepository();

            var units = await repo.GetUnit(1001);

            units.Should().NotBeNull();
            units.Should().NotBeEmpty();           // ✅ should pass now
            units[0].Id.Should().Be(unitId);
        }

        [Fact]
        public async Task GetUnit_Should_Ignore_Inactive_UserUnit_Rows()
        {
            var unitId = await SeedCompanyDivisionUnitDepartmentUserAsync();

            await using (var cnn = OpenSql())
            {
                await cnn.OpenAsync();
                await cnn.ExecuteAsync(
                    @"UPDATE AppSecurity.UserUnit
                      SET IsActive = 0
                      WHERE UserId = 1001 AND UnitId = @UnitId;",
                    new { UnitId = unitId });
            }

            var repo = CreateRepository();

            var units = await repo.GetUnit(1001);

            units.Should().NotBeNull();
            units.Should().BeEmpty("inactive UserUnit rows must not be returned by GetUnit");
        }
    }
}
