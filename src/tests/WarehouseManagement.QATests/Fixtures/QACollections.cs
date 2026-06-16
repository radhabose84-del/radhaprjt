namespace WarehouseManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("WarehouseMasterCollection")]
public sealed class WarehouseMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("RackMasterCollection")]
public sealed class RackMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BinMasterCollection")]
public sealed class BinMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("WarehouseAuditLogCollection")]
public sealed class WarehouseAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
