namespace FixedAssetManagement.FunctionalTests.Fixtures;

// One collection per workflow story. Steps within a story are ordered via [TestPriority]
// and share a single QAServerFixture (one login, one run-unique EntityCode).

[CollectionDefinition("US-FAM-01-AssetHierarchySetup")]
public sealed class AssetHierarchySetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-02-ReferenceMasters")]
public sealed class ReferenceMastersCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-03-DepreciationSetup")]
public sealed class DepreciationSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-04-AssetOnboarding")]
public sealed class AssetOnboardingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-05-AcquisitionCapitalization")]
public sealed class AcquisitionCapitalizationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-06-CoverageManagement")]
public sealed class CoverageManagementCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-07-TransferReceipt")]
public sealed class TransferReceiptCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-08-DepreciationRun")]
public sealed class DepreciationRunCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-09-Disposal")]
public sealed class DisposalCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-10-MiscMasterSetup")]
public sealed class MiscMasterSetupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-11-DashboardReporting")]
public sealed class DashboardReportingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("US-FAM-12-AuditLog")]
public sealed class AuditLogCollection : ICollectionFixture<QAServerFixture> { }
