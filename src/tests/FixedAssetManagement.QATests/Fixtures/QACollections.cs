namespace FixedAssetManagement.QATests.Fixtures;

// One xUnit collection per entity → each shares a single QAServerFixture (one testsales
// session). Collections are NOT parallelized (see AssemblyInfo) so sessions never clash.

[CollectionDefinition("AssetGroupCollection")]
public sealed class AssetGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetCategoriesCollection")]
public sealed class AssetCategoriesCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LocationCollection")]
public sealed class LocationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UOMCollection")]
public sealed class UOMCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("ManufactureCollection")]
public sealed class ManufactureCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetSubGroupCollection")]
public sealed class AssetSubGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetSubCategoriesCollection")]
public sealed class AssetSubCategoriesCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetLocationCollection")]
public sealed class AssetLocationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SubLocationCollection")]
public sealed class SubLocationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SpecificationMasterCollection")]
public sealed class SpecificationMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DepreciationGroupCollection")]
public sealed class DepreciationGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("FamMiscTypeMasterCollection")]
public sealed class FamMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("FamMiscMasterCollection")]
public sealed class FamMiscMasterCollection : ICollectionFixture<QAServerFixture> { }

// ── Transactional entities ──────────────────────────────────────────────────
[CollectionDefinition("AssetMasterGeneralCollection")]
public sealed class AssetMasterGeneralCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetPurchaseCollection")]
public sealed class AssetPurchaseCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetAdditionalCostCollection")]
public sealed class AssetAdditionalCostCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetAmcCollection")]
public sealed class AssetAmcCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetInsuranceCollection")]
public sealed class AssetInsuranceCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetWarrantyCollection")]
public sealed class AssetWarrantyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetDisposalCollection")]
public sealed class AssetDisposalCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetTransferCollection")]
public sealed class AssetTransferCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetTransferReceiptCollection")]
public sealed class AssetTransferReceiptCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AssetSpecificationCollection")]
public sealed class AssetSpecificationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DepreciationDetailCollection")]
public sealed class DepreciationDetailCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("WDVDepreciationCollection")]
public sealed class WDVDepreciationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
