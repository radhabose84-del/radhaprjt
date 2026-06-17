namespace GateEntryManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("GateMiscMasterCollection")]
public sealed class GateMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("GateMiscTypeMasterCollection")]
public sealed class GateMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("VehicleMovementRecordCollection")]
public sealed class VehicleMovementRecordCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("GateInwardCollection")]
public sealed class GateInwardCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("GatePassCollection")]
public sealed class GatePassCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("GateAuditLogCollection")]
public sealed class GateAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
