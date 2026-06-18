namespace PartyManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("PartyMiscMasterCollection")]
public sealed class PartyMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PartyMiscTypeMasterCollection")]
public sealed class PartyMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PartyGroupCollection")]
public sealed class PartyGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BankMasterCollection")]
public sealed class BankMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("BankAccountCollection")]
public sealed class BankAccountCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("GSTCollection")]
public sealed class GSTCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PartyMasterCollection")]
public sealed class PartyMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PartyAuditLogCollection")]
public sealed class PartyAuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
