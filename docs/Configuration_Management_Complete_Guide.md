# BSOFT Modular Monolith - Configuration Management Complete Guide

## 📋 Table of Contents

1. [Configuration File Placement](#1-configuration-file-placement)
2. [Environment-Specific Configuration](#2-environment-specific-configuration)
3. [Configuration Loading Mechanism](#3-configuration-loading-mechanism)
4. [Accessing Configuration Across Modules](#4-accessing-configuration-across-modules)
5. [Module-Specific Configuration](#5-module-specific-configuration)
6. [Best Practices](#6-best-practices)
7. [Complete Examples](#7-complete-examples)

---

## 1️⃣ Configuration File Placement

### Overview

In BSOFT Modular Monolith, **all configuration files are placed in the API project** - the single entry point for the entire application.

### File Structure

```
BSOFT.ModularMonolith/
└── src/
    └── API/                                    ← ALL config files here
        ├── BSOFT.API.csproj
        ├── Program.cs
        │
        ├── appsettings.json                    ← Base configuration
        ├── appsettings.Development.json        ← Development overrides
        ├── appsettings.QA.json                 ← QA overrides
        ├── appsettings.Testing.json            ← Testing overrides
        ├── appsettings.Production.json         ← Production overrides
        │
        ├── settings/                           ← Optional: Separate files
        │   ├── jwtsetting.json
        │   ├── serilogsetting.Development.json
        │   ├── serilogsetting.Production.json
        │   └── emailsetting.json
        │
        └── Controllers/
            └── ...
```

### Why API Project?

**Reasons**:
1. ✅ **Single entry point** - Program.cs is the startup file
2. ✅ **Simpler deployment** - All configs in one place
3. ✅ **Easier management** - One location to update
4. ✅ **Clear ownership** - API layer owns all configuration

**NOT in modules**:
- ❌ NOT in `src/Modules/UserManagement/appsettings.json`
- ❌ NOT in `src/Modules/FixedAssetManagement/appsettings.json`
- ❌ NOT in `src/Shared.Infrastructure/appsettings.json`

**Key Principle**: Modules are **configuration-agnostic**. They receive configuration through dependency injection.

---

## 2️⃣ Environment-Specific Configuration

### Configuration Hierarchy

ASP.NET Core loads configuration files in this order (later files override earlier ones):

```
1. appsettings.json                    ← Base settings (always loaded)
2. appsettings.{Environment}.json      ← Environment-specific overrides
3. Environment variables               ← Override both files
4. Command-line arguments              ← Highest priority
```

### Complete appsettings.json (Base Configuration)

**Location**: `src/API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BSOFT_Dev;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "BSOFT",
    "Audience": "BSOFT_Users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "BSOFT ERP",
    "SenderEmail": "noreply@bsoft.com",
    "Username": "",
    "Password": "",
    "EnableSsl": true
  },
  
  "SmsSettings": {
    "Provider": "Twilio",
    "AccountSid": "",
    "AuthToken": "",
    "FromNumber": ""
  },
  
  "FileStorage": {
    "Provider": "Local",
    "BasePath": "./uploads",
    "MaxFileSizeMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".docx"]
  },
  
  "AllowedOrigins": [
    "http://localhost:4200",
    "http://localhost:3000"
  ],
  
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "ServerName": "BSOFT-Worker",
    "WorkerCount": 5
  },
  
  "ModuleSettings": {
    "UserManagement": {
      "PasswordComplexity": {
        "MinLength": 8,
        "RequireUppercase": true,
        "RequireLowercase": true,
        "RequireDigit": true,
        "RequireSpecialChar": true
      },
      "SessionTimeout": 30,
      "MaxLoginAttempts": 5,
      "LockoutDurationMinutes": 15
    },
    "FixedAssetManagement": {
      "AssetNumberPrefix": "FA",
      "AutoGenerateAssetTag": true,
      "DefaultDepreciationMethod": "StraightLine",
      "RequireApprovalForTransfer": true,
      "RequireApprovalForDisposal": true
    },
    "Inventory": {
      "AutoGenerateItemCode": true,
      "EnableBatchTracking": true,
      "EnableSerialTracking": false,
      "LowStockThreshold": 10
    }
  },
  
  "CacheSettings": {
    "DefaultExpirationMinutes": 60,
    "SlidingExpiration": true
  },
  
  "AuditSettings": {
    "EnableAuditLog": true,
    "LogRetentionDays": 90
  }
}
```

### appsettings.Development.json

**Location**: `src/API/appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BSOFT_Dev;User Id=dev_user;Password=Dev@123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  
  "JwtSettings": {
    "Secret": "DevSecretKey32CharactersLongForDevelopment!",
    "ExpirationMinutes": 480
  },
  
  "EmailSettings": {
    "SmtpServer": "localhost",
    "SmtpPort": 1025,
    "EnableSsl": false
  },
  
  "FileStorage": {
    "BasePath": "./uploads/dev"
  },
  
  "AllowedOrigins": [
    "http://localhost:4200",
    "http://localhost:3000",
    "http://localhost:5173"
  ],
  
  "ModuleSettings": {
    "UserManagement": {
      "PasswordComplexity": {
        "MinLength": 4,
        "RequireUppercase": false,
        "RequireLowercase": false,
        "RequireDigit": false,
        "RequireSpecialChar": false
      },
      "SessionTimeout": 480,
      "MaxLoginAttempts": 10
    }
  },
  
  "DetailedErrors": true,
  "EnableSwagger": true
}
```

### appsettings.QA.json

**Location**: `src/API/appsettings.QA.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=qa-sql-server.database.windows.net;Database=BSOFT_QA;User Id=qa_user;Password=QA@SecurePassword123!;Encrypt=True;TrustServerCertificate=False"
  },
  
  "JwtSettings": {
    "Secret": "QASecretKey32CharactersLongForQATesting!",
    "Issuer": "BSOFT_QA",
    "Audience": "BSOFT_QA_Users",
    "ExpirationMinutes": 120
  },
  
  "EmailSettings": {
    "SmtpServer": "smtp-qa.bsoft.com",
    "SmtpPort": 587,
    "SenderEmail": "qa-noreply@bsoft.com",
    "Username": "qa-smtp@bsoft.com",
    "Password": "${EMAIL_PASSWORD}",
    "EnableSsl": true
  },
  
  "SmsSettings": {
    "Provider": "Twilio",
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "FromNumber": "${TWILIO_FROM_NUMBER}"
  },
  
  "FileStorage": {
    "Provider": "AzureBlob",
    "ConnectionString": "${AZURE_STORAGE_CONNECTION}",
    "ContainerName": "bsoft-qa-uploads"
  },
  
  "AllowedOrigins": [
    "https://qa.bsoft.com",
    "https://qa-admin.bsoft.com"
  ],
  
  "ModuleSettings": {
    "UserManagement": {
      "PasswordComplexity": {
        "MinLength": 8,
        "RequireUppercase": true,
        "RequireLowercase": true,
        "RequireDigit": true,
        "RequireSpecialChar": true
      },
      "SessionTimeout": 60,
      "MaxLoginAttempts": 5
    }
  },
  
  "EnableSwagger": true,
  "DetailedErrors": false
}
```

### appsettings.Testing.json

**Location**: `src/API/appsettings.Testing.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BSOFT_Test;Integrated Security=true;TrustServerCertificate=True"
  },
  
  "JwtSettings": {
    "Secret": "TestSecretKey32CharactersLongForTesting!",
    "ExpirationMinutes": 60
  },
  
  "EmailSettings": {
    "SmtpServer": "localhost",
    "SmtpPort": 1025,
    "EnableSsl": false
  },
  
  "FileStorage": {
    "Provider": "InMemory",
    "BasePath": "./test-uploads"
  },
  
  "Hangfire": {
    "ServerName": "Test-Worker",
    "WorkerCount": 1
  },
  
  "EnableSwagger": false,
  "DetailedErrors": true
}
```

### appsettings.Production.json

**Location**: `src/API/appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_CONNECTION_STRING}"
  },
  
  "JwtSettings": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "BSOFT_Production",
    "Audience": "BSOFT_Production_Users",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  
  "EmailSettings": {
    "SmtpServer": "${SMTP_SERVER}",
    "SmtpPort": 587,
    "SenderName": "BSOFT ERP",
    "SenderEmail": "noreply@bsoft.com",
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}",
    "EnableSsl": true
  },
  
  "SmsSettings": {
    "Provider": "Twilio",
    "AccountSid": "${TWILIO_ACCOUNT_SID}",
    "AuthToken": "${TWILIO_AUTH_TOKEN}",
    "FromNumber": "${TWILIO_FROM_NUMBER}"
  },
  
  "FileStorage": {
    "Provider": "AzureBlob",
    "ConnectionString": "${AZURE_STORAGE_CONNECTION}",
    "ContainerName": "bsoft-prod-uploads",
    "MaxFileSizeMB": 10
  },
  
  "AllowedOrigins": [
    "https://app.bsoft.com",
    "https://admin.bsoft.com"
  ],
  
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "ServerName": "BSOFT-Prod-Worker",
    "WorkerCount": 10
  },
  
  "ModuleSettings": {
    "UserManagement": {
      "PasswordComplexity": {
        "MinLength": 12,
        "RequireUppercase": true,
        "RequireLowercase": true,
        "RequireDigit": true,
        "RequireSpecialChar": true
      },
      "SessionTimeout": 30,
      "MaxLoginAttempts": 3,
      "LockoutDurationMinutes": 30
    },
    "FixedAssetManagement": {
      "RequireApprovalForTransfer": true,
      "RequireApprovalForDisposal": true,
      "AutoGenerateAssetTag": true
    }
  },
  
  "CacheSettings": {
    "DefaultExpirationMinutes": 60,
    "SlidingExpiration": true
  },
  
  "EnableSwagger": false,
  "DetailedErrors": false,
  "UseHttpsRedirection": true,
  "EnableHSTS": true
}
```

---

## 3️⃣ Configuration Loading Mechanism

### How ASP.NET Core Loads Configuration

#### Step 1: Program.cs Creates Builder

```csharp
// src/API/Program.cs
var builder = WebApplication.CreateBuilder(args);

// WebApplicationBuilder automatically:
// 1. Loads appsettings.json
// 2. Loads appsettings.{Environment}.json based on ASPNETCORE_ENVIRONMENT
// 3. Loads environment variables
// 4. Loads command-line arguments
```

#### Step 2: Environment Detection

```csharp
// Environment is determined by ASPNETCORE_ENVIRONMENT variable
// Default: Production

// Check current environment
var env = builder.Environment;

if (env.IsDevelopment())
{
    // Loads: appsettings.Development.json
    // Enable: Swagger, detailed errors, etc.
}
else if (env.EnvironmentName == "QA")
{
    // Loads: appsettings.QA.json
}
else if (env.EnvironmentName == "Testing")
{
    // Loads: appsettings.Testing.json
}
else if (env.IsProduction())
{
    // Loads: appsettings.Production.json
    // Disable: Swagger, detailed errors, etc.
}
```

#### Step 3: Accessing Configuration

```csharp
// Program.cs

// Access configuration values
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var jwtSecret = builder.Configuration["JwtSettings:Secret"];

// Or bind to strongly-typed objects
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
```

### Configuration Loading Diagram

```
Application Start
    ↓
1. Load appsettings.json (base)
    ↓
2. Check ASPNETCORE_ENVIRONMENT
    ├─ Development → Load appsettings.Development.json
    ├─ QA → Load appsettings.QA.json
    ├─ Testing → Load appsettings.Testing.json
    └─ Production → Load appsettings.Production.json
    ↓
3. Load Environment Variables
    ↓
4. Load Command-line Arguments
    ↓
5. Merge All (later overrides earlier)
    ↓
6. Configuration Available via IConfiguration
```

### Setting Environment

**Development**:
```bash
# Windows
set ASPNETCORE_ENVIRONMENT=Development
dotnet run

# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Development
dotnet run

# VS Code (launch.json)
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development"
}
```

**QA**:
```bash
export ASPNETCORE_ENVIRONMENT=QA
dotnet run
```

**Production**:
```bash
# Usually not set explicitly (defaults to Production)
dotnet run

# Or explicitly
export ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

---

## 4️⃣ Accessing Configuration Across Modules

### Key Principle

**Configuration flows from API → Modules via Dependency Injection**

Modules **never** read configuration files directly. They receive configuration through:
1. Strongly-typed options classes
2. Constructor injection
3. Registered services

### Pattern 1: Strongly-Typed Options Classes

#### Step 1: Define Options Class

```csharp
// Shared.Kernel/Configuration/JwtSettings.cs
namespace BSOFT.Shared.Kernel.Configuration;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
```

```csharp
// Shared.Kernel/Configuration/EmailSettings.cs
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}
```

```csharp
// UserManagement/Core/Application/Common/Configuration/UserManagementSettings.cs
namespace UserManagement.Core.Application.Common.Configuration;

public class UserManagementSettings
{
    public PasswordComplexitySettings PasswordComplexity { get; set; } = new();
    public int SessionTimeout { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
}

public class PasswordComplexitySettings
{
    public int MinLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireDigit { get; set; }
    public bool RequireSpecialChar { get; set; }
}
```

#### Step 2: Register in Program.cs

```csharp
// src/API/Program.cs

// Register options from configuration
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<UserManagementSettings>(
    builder.Configuration.GetSection("ModuleSettings:UserManagement"));

builder.Services.Configure<FixedAssetSettings>(
    builder.Configuration.GetSection("ModuleSettings:FixedAssetManagement"));
```

#### Step 3: Inject in Module Services

```csharp
// UserManagement/Infrastructure/Services/TokenService.cs
using Microsoft.Extensions.Options;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            // ... rest of token generation
        };
        
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
```

```csharp
// UserManagement/Application/Commands/CreateUser/CreateUserCommandHandler.cs
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    private readonly BSoftDbContext _context;
    private readonly UserManagementSettings _settings;

    public CreateUserCommandHandler(
        BSoftDbContext context,
        IOptions<UserManagementSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
    }

    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Use configuration
        if (request.Password.Length < _settings.PasswordComplexity.MinLength)
            return Result.Failure<int>(
                Error.Validation("Password", 
                    $"Password must be at least {_settings.PasswordComplexity.MinLength} characters"));

        // ... rest of logic
    }
}
```

### Pattern 2: IConfiguration Injection (When Needed)

```csharp
// Shared.Infrastructure/Services/FileStorageService.cs
public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;

    public FileStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetUploadPath()
    {
        // Access configuration directly
        var provider = _configuration["FileStorage:Provider"];
        var basePath = _configuration["FileStorage:BasePath"];
        
        return provider == "Local" ? basePath : null;
    }
}
```

**Note**: Use strongly-typed options when possible. Direct IConfiguration injection is for dynamic scenarios.

### Module-Specific Configuration Access Flow

```
appsettings.json (in API)
    ↓
Program.cs reads configuration
    ↓
builder.Services.Configure<ModuleSettings>(...)
    ↓
DI Container holds options
    ↓
Module Service Constructor
    ↓
IOptions<ModuleSettings> injected
    ↓
Service uses settings.Value
```

---

## 5️⃣ Module-Specific Configuration

### Organization Strategy

**Option 1: Nested in Main appsettings.json** (Recommended)

```json
{
  "ModuleSettings": {
    "UserManagement": {
      "PasswordComplexity": { ... },
      "SessionTimeout": 30
    },
    "FixedAssetManagement": {
      "AssetNumberPrefix": "FA",
      "RequireApprovalForTransfer": true
    },
    "Inventory": {
      "AutoGenerateItemCode": true,
      "LowStockThreshold": 10
    }
  }
}
```

**Option 2: Separate Files (Optional)**

```
src/API/
├── appsettings.json
└── settings/
    ├── usermanagement.json
    ├── fixedasset.json
    └── inventory.json
```

```csharp
// Program.cs - Load additional files
builder.Configuration
    .AddJsonFile("settings/usermanagement.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/fixedasset.json", optional: true, reloadOnChange: true)
    .AddJsonFile("settings/inventory.json", optional: true, reloadOnChange: true);
```

### Module Settings Classes

```csharp
// FixedAssetManagement/Core/Application/Common/Configuration/FixedAssetSettings.cs
namespace FixedAssetManagement.Core.Application.Common.Configuration;

public class FixedAssetSettings
{
    public string AssetNumberPrefix { get; set; } = "FA";
    public bool AutoGenerateAssetTag { get; set; } = true;
    public string DefaultDepreciationMethod { get; set; } = "StraightLine";
    public bool RequireApprovalForTransfer { get; set; } = true;
    public bool RequireApprovalForDisposal { get; set; } = true;
    public int DepreciationCalculationDay { get; set; } = 1;
    public bool EnableMaintenanceScheduling { get; set; } = true;
}
```

**Register in Program.cs**:
```csharp
builder.Services.Configure<FixedAssetSettings>(
    builder.Configuration.GetSection("ModuleSettings:FixedAssetManagement"));
```

**Use in Module**:
```csharp
// FixedAssetManagement/Application/Commands/CreateAsset/CreateAssetCommandHandler.cs
public class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, Result<int>>
{
    private readonly BSoftDbContext _context;
    private readonly FixedAssetSettings _settings;
    private readonly IAssetNumberGenerator _numberGenerator;

    public CreateAssetCommandHandler(
        BSoftDbContext context,
        IOptions<FixedAssetSettings> settings,
        IAssetNumberGenerator numberGenerator)
    {
        _context = context;
        _settings = settings.Value;
        _numberGenerator = numberGenerator;
    }

    public async Task<Result<int>> Handle(CreateAssetCommand request, CancellationToken ct)
    {
        // Use module-specific configuration
        var assetNumber = _settings.AutoGenerateAssetTag
            ? await _numberGenerator.GenerateAsync(_settings.AssetNumberPrefix, ct)
            : request.AssetNumber;

        var asset = new Asset
        {
            AssetNumber = assetNumber,
            // ... other properties
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync(ct);

        return Result.Success(asset.AssetId);
    }
}
```

---

## 6️⃣ Best Practices

### ✅ DO

1. **Keep all configuration in API project**
   ```
   ✅ src/API/appsettings.json
   ❌ src/Modules/UserManagement/appsettings.json
   ```

2. **Use strongly-typed options classes**
   ```csharp
   ✅ IOptions<JwtSettings> jwtSettings
   ❌ IConfiguration["JwtSettings:Secret"]
   ```

3. **Use environment-specific overrides**
   ```json
   ✅ appsettings.Development.json overrides base
   ❌ Duplicate entire config in each environment file
   ```

4. **Store secrets securely**
   ```bash
   ✅ Environment variables: ${JWT_SECRET}
   ✅ Azure Key Vault
   ✅ User Secrets (development)
   ❌ Hardcoded in appsettings.json
   ```

5. **Validate configuration on startup**
   ```csharp
   ✅ builder.Services.AddOptions<JwtSettings>()
       .Bind(builder.Configuration.GetSection("JwtSettings"))
       .ValidateDataAnnotations()
       .ValidateOnStart();
   ```

6. **Group by module**
   ```json
   ✅ "ModuleSettings": {
         "UserManagement": { ... },
         "FixedAssetManagement": { ... }
       }
   ```

### ❌ DON'T

1. **Don't put config in modules**
   ```
   ❌ Module projects should NOT contain appsettings.json
   ```

2. **Don't hardcode values in code**
   ```csharp
   ❌ const string SECRET = "hardcoded-secret";
   ✅ _jwtSettings.Secret (from configuration)
   ```

3. **Don't expose secrets in appsettings.json**
   ```json
   ❌ "Password": "ActualPassword123!"
   ✅ "Password": "${SMTP_PASSWORD}"
   ```

4. **Don't duplicate configuration**
   ```json
   ❌ Copying entire appsettings.json to each environment file
   ✅ Only override what's different
   ```

### Security Best Practices

#### Development: User Secrets

```bash
# Initialize user secrets
cd src/API
dotnet user-secrets init

# Add secrets
dotnet user-secrets set "JwtSettings:Secret" "DevSecret123!"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
```

**Access in code** (automatic):
```csharp
var secret = builder.Configuration["JwtSettings:Secret"];
// Reads from user secrets in Development
```

#### Production: Environment Variables

```bash
# Linux/Azure App Service
export JWT_SECRET="ProductionSecret..."
export DATABASE_CONNECTION_STRING="Server=..."

# Docker
docker run -e JWT_SECRET="..." -e DATABASE_CONNECTION_STRING="..." bsoft:latest
```

```json
// appsettings.Production.json
{
  "JwtSettings": {
    "Secret": "${JWT_SECRET}"
  },
  "ConnectionStrings": {
    "DefaultConnection": "${DATABASE_CONNECTION_STRING}"
  }
}
```

#### Azure Key Vault (Production)

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    var keyVaultUrl = builder.Configuration["KeyVault:Url"];
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}
```

---

## 7️⃣ Complete Examples

### Example 1: Complete Configuration Setup

#### Step 1: Create appsettings Files

Create all files shown in Section 2 in `src/API/`.

#### Step 2: Create Options Classes

```csharp
// Shared.Kernel/Configuration/AppSettings.cs
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }
}

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}
```

#### Step 3: Register in Program.cs

```csharp
// src/API/Program.cs

// Register all options
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

builder.Services.Configure<UserManagementSettings>(
    builder.Configuration.GetSection("ModuleSettings:UserManagement"));
```

#### Step 4: Use in Services

```csharp
// UserManagement/Infrastructure/Services/EmailService.cs
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
            EnableSsl = _emailSettings.EnableSsl
        };

        var message = new MailMessage
        {
            From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await smtp.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To}", to);
    }
}
```

### Example 2: Environment-Specific Behavior

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Environment-specific configuration
if (builder.Environment.IsDevelopment())
{
    // Development: Use local SMTP
    builder.Services.Configure<EmailSettings>(options =>
    {
        options.SmtpServer = "localhost";
        options.SmtpPort = 1025;
        options.EnableSsl = false;
    });
    
    // Enable Swagger
    builder.Services.AddSwaggerGen();
}
else if (builder.Environment.EnvironmentName == "QA")
{
    // QA: Use QA SMTP and additional logging
    builder.Services.AddApplicationInsightsTelemetry();
}
else if (builder.Environment.IsProduction())
{
    // Production: Strict security
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
    });
}

var app = builder.Build();

// Environment-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.Run();
```

### Example 3: Configuration Validation

```csharp
// Shared.Kernel/Configuration/JwtSettings.cs
using System.ComponentModel.DataAnnotations;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = string.Empty;
    
    [Required]
    public string Issuer { get; set; } = string.Empty;
    
    [Required]
    public string Audience { get; set; } = string.Empty;
    
    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; }
}
```

```csharp
// Program.cs - Validate on startup
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Application will fail to start if validation fails
```

### Example 4: Dynamic Configuration Reload

```csharp
// Program.cs - Enable configuration reload
builder.Configuration.AddJsonFile(
    "appsettings.json",
    optional: false,
    reloadOnChange: true);  // ← Configuration reloads when file changes

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);
```

```csharp
// Service that monitors configuration changes
public class ConfigurationMonitorService : BackgroundService
{
    private readonly IOptionsMonitor<JwtSettings> _jwtSettings;
    private readonly ILogger<ConfigurationMonitorService> _logger;

    public ConfigurationMonitorService(
        IOptionsMonitor<JwtSettings> jwtSettings,
        ILogger<ConfigurationMonitorService> logger)
    {
        _jwtSettings = jwtSettings;
        _logger = logger;
        
        // Subscribe to configuration changes
        _jwtSettings.OnChange(settings =>
        {
            _logger.LogInformation("JWT settings changed: Expiration = {Minutes} minutes",
                settings.ExpirationMinutes);
        });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
```

---

## 🎯 Summary

### Configuration File Placement
✅ **Location**: ALL in `src/API/` directory  
✅ **Base**: `appsettings.json`  
✅ **Environments**: `appsettings.{Environment}.json`  
✅ **NOT in modules**: Modules are configuration-agnostic  

### Configuration Loading
✅ **Automatic**: ASP.NET Core loads based on ASPNETCORE_ENVIRONMENT  
✅ **Hierarchy**: Base → Environment → Env Variables → CLI Args  
✅ **Merge**: Later sources override earlier  

### Accessing Configuration
✅ **Pattern**: Strongly-typed options classes via IOptions<T>  
✅ **Injection**: Constructor injection in services  
✅ **Flow**: API → DI Container → Module Services  

### Module-Specific Settings
✅ **Organization**: Nested under "ModuleSettings" key  
✅ **Classes**: Each module defines its own settings class  
✅ **Registration**: Program.cs binds sections to classes  

### Best Practices
✅ **Security**: Use environment variables for secrets  
✅ **Validation**: Validate configuration on startup  
✅ **Reusability**: Strongly-typed options across modules  
✅ **Simplicity**: Single location for all configuration  

**Result**: Clean, maintainable configuration management for the entire BSOFT application!
