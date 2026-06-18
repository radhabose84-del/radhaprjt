namespace PurchaseManagement.QATests.Fixtures;

// One collection per entity. All bind the single QAServerFixture; cross-collection
// parallelization is disabled (AssemblyInfo.cs) — every collection shares the one `testsales` session.

[CollectionDefinition("PurMiscMasterCollection")] public sealed class PurMiscMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurMiscTypeMasterCollection")] public sealed class PurMiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("MixCodeMasterCollection")] public sealed class MixCodeMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("DeliveryScoreRuleCollection")] public sealed class DeliveryScoreRuleCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("DutyMasterCollection")] public sealed class DutyMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PaymentTermMasterCollection")] public sealed class PaymentTermMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PortMasterCollection")] public sealed class PortMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ReturnTypeCollection")] public sealed class ReturnTypeCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ReturnReasonCollection")] public sealed class ReturnReasonCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("VendorRatingGradeCollection")] public sealed class VendorRatingGradeCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ServiceMasterCollection")] public sealed class ServiceMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("VendorEvaluationCriteriaCollection")] public sealed class VendorEvaluationCriteriaCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("TnCTemplateMasterCollection")] public sealed class TnCTemplateMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("BarcodeSeriesCollection")] public sealed class BarcodeSeriesCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("BarcodeAllocationCollection")] public sealed class BarcodeAllocationCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ContractPOMasterCollection")] public sealed class ContractPOMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("BlanketMasterCollection")] public sealed class BlanketMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PriceMasterCollection")] public sealed class PriceMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ArrivalCollection")] public sealed class ArrivalCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("GRNEntryCollection")] public sealed class GRNEntryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurGateEntryCollection")] public sealed class PurGateEntryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurIssueCollection")] public sealed class PurIssueCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("IssueReturnCollection")] public sealed class IssueReturnCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurMrsCollection")] public sealed class PurMrsCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("OCREntryCollection")] public sealed class OCREntryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurchaseIndentCollection")] public sealed class PurchaseIndentCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurchaseBillEntryCollection")] public sealed class PurchaseBillEntryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurchaseOrderLocalCollection")] public sealed class PurchaseOrderLocalCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ImportPOCollection")] public sealed class ImportPOCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("BlanketPOCollection")] public sealed class BlanketPOCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ContractPOCollection")] public sealed class ContractPOCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("CombinePOCollection")] public sealed class CombinePOCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurchaseReturnCollection")] public sealed class PurchaseReturnCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("RawMaterialPOCollection")] public sealed class RawMaterialPOCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("QuotationEntryCollection")] public sealed class QuotationEntryCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("QuotationCompareCollection")] public sealed class QuotationCompareCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("FreightRfqCollection")] public sealed class FreightRfqCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("RfqsCollection")] public sealed class RfqsCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("SESCollection")] public sealed class SESCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("VendorEvaluationHeaderCollection")] public sealed class VendorEvaluationHeaderCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("ExchangeRateCollection")] public sealed class ExchangeRateCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurPartyMasterCollection")] public sealed class PurPartyMasterCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurchaseOrderPrintCollection")] public sealed class PurchaseOrderPrintCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurReportsCollection")] public sealed class PurReportsCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurActivityLogsCollection")] public sealed class PurActivityLogsCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("PurAuditLogCollection")] public sealed class PurAuditLogCollection : ICollectionFixture<QAServerFixture> { }
[CollectionDefinition("SecurityCollection")] public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }
