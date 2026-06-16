namespace UserManagement.QATests.Fixtures;

[CollectionDefinition("AccessPolicyCollection")]
public sealed class AccessPolicyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CountryCollection")]
public sealed class CountryCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StateCollection")]
public sealed class StateCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CityCollection")]
public sealed class CityCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CurrencyCollection")]
public sealed class CurrencyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MiscMasterCollection")]
public sealed class MiscMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MiscTypeMasterCollection")]
public sealed class MiscTypeMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DepartmentCollection")]
public sealed class DepartmentCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DepartmentGroupCollection")]
public sealed class DepartmentGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("DivisionCollection")]
public sealed class DivisionCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UnitCollection")]
public sealed class UnitCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CompanyCollection")]
public sealed class CompanyCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("FinancialYearCollection")]
public sealed class FinancialYearCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LanguageCollection")]
public sealed class LanguageCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserGroupCollection")]
public sealed class UserGroupCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserRoleCollection")]
public sealed class UserRoleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SecurityCollection")]
public sealed class SecurityCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AuthCollection")]
public sealed class AuthCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AsyncCollection")]
public sealed class AsyncCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("IsolationCollection")]
public sealed class IsolationCollection : ICollectionFixture<TwoCompanyFixture> { }

// ── Additional master entities ──────────────────────────────────────────────
[CollectionDefinition("ModulesCollection")]
public sealed class ModulesCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("MenuCollection")]
public sealed class MenuCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("EntityCollection")]
public sealed class EntityCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("LocationCollection")]
public sealed class LocationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("StationCollection")]
public sealed class StationCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("IconMasterCollection")]
public sealed class IconMasterCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("PasswordComplexityRuleCollection")]
public sealed class PasswordComplexityRuleCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AdminSecuritySettingsCollection")]
public sealed class AdminSecuritySettingsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CompanySettingsCollection")]
public sealed class CompanySettingsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("CustomFieldCollection")]
public sealed class CustomFieldCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("RoleItemGroupMappingCollection")]
public sealed class RoleItemGroupMappingCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserCollection")]
public sealed class UserCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserSignatureCollection")]
public sealed class UserSignatureCollection : ICollectionFixture<QAServerFixture> { }

// ── Read-only ───────────────────────────────────────────────────────────────
[CollectionDefinition("TimeZonesCollection")]
public sealed class TimeZonesCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("AuditLogCollection")]
public sealed class AuditLogCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("HealthCollection")]
public sealed class HealthCollection : ICollectionFixture<QAServerFixture> { }

// ── Action / RBAC ───────────────────────────────────────────────────────────
[CollectionDefinition("AdminCollection")]
public sealed class AdminCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("NotificationsCollection")]
public sealed class NotificationsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("SwitchProfileCollection")]
public sealed class SwitchProfileCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserFavoriteMenuCollection")]
public sealed class UserFavoriteMenuCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("RoleEntitlementsCollection")]
public sealed class RoleEntitlementsCollection : ICollectionFixture<QAServerFixture> { }

[CollectionDefinition("UserRoleAllocationCollection")]
public sealed class UserRoleAllocationCollection : ICollectionFixture<QAServerFixture> { }
