-- =====================================================================
-- US-GL02-03A  Schedule III  —  MiscTypeMaster + MiscMaster seed
-- Schema: Finance.   Idempotent (safe to re-run).   Audit: CreatedBy = 1.
-- Types: S3_STMT_TYPE / S3_NATURE / S3_STATUS / S3_OPERATOR / S3_OPERAND_TYPE
-- =====================================================================

-- 1) MiscTypeMaster groups -------------------------------------------------
INSERT INTO [Finance].[MiscTypeMaster] (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedDate)
SELECT v.MiscTypeCode, v.Description, 1, 0, 1, SYSDATETIMEOFFSET()
FROM (VALUES
    ('S3_STMT_TYPE',    'Schedule III Statement Type'),
    ('S3_NATURE',       'Schedule III Nature / Equation Side'),
    ('S3_STATUS',       'Schedule III Structure Status'),
    ('S3_OPERATOR',     'Sub-total Operator'),
    ('S3_OPERAND_TYPE', 'Sub-total Operand Type')
) v (MiscTypeCode, Description)
WHERE NOT EXISTS (
    SELECT 1 FROM [Finance].[MiscTypeMaster] t
    WHERE t.MiscTypeCode = v.MiscTypeCode AND t.IsDeleted = 0
);

-- 2) MiscMaster values (MiscTypeId resolved by code) -----------------------
;WITH vals AS (
    SELECT * FROM (VALUES
        ('S3_STMT_TYPE',    'BS',       'Balance Sheet',                  1),
        ('S3_STMT_TYPE',    'PL',       'Statement of Profit & Loss',     2),
        ('S3_STMT_TYPE',    'SOCE',     'Statement of Changes in Equity', 3),
        ('S3_NATURE',       'EQLIAB',   'Equity & Liability',             1),
        ('S3_NATURE',       'ASSET',    'Asset',                          2),
        ('S3_NATURE',       'INCOME',   'Income',                         3),
        ('S3_NATURE',       'EXPENSE',  'Expense',                        4),
        ('S3_NATURE',       'EQUITY',   'Equity',                         5),
        ('S3_NATURE',       'FINASSET', 'Financial Asset',                6),
        ('S3_NATURE',       'FINLIAB',  'Financial Liability',            7),
        ('S3_STATUS',       'DRAFT',    'Draft',                          1),
        ('S3_STATUS',       'LOCKED',   'Locked',                         2),
        ('S3_OPERATOR',     'PLUS',     'Plus (+)',                       1),
        ('S3_OPERATOR',     'MINUS',    'Minus (-)',                      2),
        ('S3_OPERAND_TYPE', 'LINEITEM', 'Line Item',                      1),
        ('S3_OPERAND_TYPE', 'SUBTOTAL', 'Sub Total',                      2)
    ) v (TypeCode, Code, Description, SortOrder)
)
INSERT INTO [Finance].[MiscMaster] (MiscTypeId, Code, Description, SortOrder, IsActive, IsDeleted, CreatedBy, CreatedDate)
SELECT t.Id, v.Code, v.Description, v.SortOrder, 1, 0, 1, SYSDATETIMEOFFSET()
FROM vals v
JOIN [Finance].[MiscTypeMaster] t ON t.MiscTypeCode = v.TypeCode AND t.IsDeleted = 0
WHERE NOT EXISTS (
    SELECT 1 FROM [Finance].[MiscMaster] m
    WHERE m.MiscTypeId = t.Id AND m.Code = v.Code AND m.IsDeleted = 0
);
