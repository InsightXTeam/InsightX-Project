# InsightX AI — Backend Implementation Plan
**Person 1 | Authentication + Company Setup**  
**Stack: ASP.NET Core (.NET 8) · ASP.NET Core Identity · EF Core · LINQ · JWT**

---

## Overview

This plan is split into 4 phases that build on each other. Each phase must be completed and tested before the next begins, since authentication and company isolation are prerequisites for every other feature in the system.

---

## Solution Architecture Mapping

Every item in this plan maps to a specific project in the solution. Follow this table to know exactly where each file belongs.

| Plan Item | Solution Project | Path |
|---|---|---|
| `ApplicationUser`, `Company`, `Department`, `KPI`, `RefreshToken` | `InsightX.Domain` | `Entities/` |
| `ITokenService` | `InsightX.Application` | `Interfaces/` |
| `RegisterDto`, `LoginDto`, `AuthResponseDto`, `InviteUserDto`, etc. | `InsightX.Application` | `DTOs/` |
| Register / Login / Invite use case logic | `InsightX.Application` | `UseCases/` |
| `TokenService` implementation | `InsightX.Infrastructure` | `Services/` *(new folder)* |
| `AppDbContext` + EF Core fluent config | `InsightX.Infrastructure` | `Persistence/AppDbContext.cs` ✅ |
| `AuthController`, `CompanyController`, `DepartmentController`, `UserController` | `InsightX.API` | `Controllers/` |
| JWT middleware, DI registrations, role seeds | `InsightX.API` | `Program.cs` |
| `ClaimsExtensions` helper | `InsightX.Application` | `Interfaces/` or a new `Extensions/` folder |

> **Note:** `InsightX.Infrastructure/AI/` and `InsightX.Infrastructure/DocumentReaders/` are reserved for RAG/LLM work by other team members — do not add auth-related code there.

---

## Phase 1 — Domain Models & Database Schema

**Goal:** Define all entities and let EF Core + Identity generate the database.

### 1.1 — Extend IdentityUser

Create `ApplicationUser` in `InsightX.Domain/Entities/`:

```csharp
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Owner" | "Manager"
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }

    // Navigation
    public Company Company { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
```

### 1.2 — Create Remaining Entities

**`Company.cs`**
```csharp
public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ApplicationUser> Users { get; set; } = [];
    public ICollection<Department> Departments { get; set; } = [];
    public ICollection<KPI> KPIs { get; set; } = [];
}
```

**`Department.cs`**
```csharp
public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<ApplicationUser> Users { get; set; } = [];
}
```

**`KPI.cs`**
```csharp
public class KPI
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
```

**`RefreshToken.cs`**
```csharp
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
```

### 1.3 — Configure AppDbContext

In `InsightX.Infrastructure/Persistence/AppDbContext.cs`:

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<KPI> KPIs => Set<KPI>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Required for Identity tables

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Company)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DepartmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Department>()
            .HasOne(d => d.Company)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<KPI>()
            .HasOne(k => k.Company)
            .WithMany(c => c.KPIs)
            .HasForeignKey(k => k.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 1.4 — Register Identity & Run Migration

In `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
```

Then run:

```bash
dotnet ef migrations add InitialSchema --project InsightX.Infrastructure --startup-project InsightX.API
dotnet ef database update --project InsightX.Infrastructure --startup-project InsightX.API
```

### ✅ Phase 1 Done When
- [ ] All 5 tables exist in the database (+ all Identity tables)
- [ ] Foreign keys and cascade rules are correct
- [ ] `dotnet build` passes with no errors

---

## Phase 2 — JWT Service & Auth Endpoints

**Goal:** Implement token generation and the 4 auth endpoints.

### 2.1 — JWT Configuration

Add to `appsettings.json`:

```json
"Jwt": {
  "Key": "YOUR_SECRET_KEY_MIN_32_CHARS",
  "Issuer": "InsightX",
  "Audience": "InsightX",
  "AccessTokenExpiryMinutes": 15,
  "RefreshTokenExpiryDays": 7
}
```

### 2.2 — Token Service Interface

In `InsightX.Application/Interfaces/ITokenService.cs`:

```csharp
public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
```

### 2.3 — Token Service Implementation

In `InsightX.Infrastructure/AI/` or a new `Services/` folder:

```csharp
public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public string GenerateAccessToken(ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("CompanyId", user.CompanyId.ToString()),
            new Claim("DepartmentId", user.DepartmentId?.ToString() ?? ""),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(
            _config.GetValue<int>("Jwt:AccessTokenExpiryMinutes"));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        // Validate token ignoring expiry — used for refresh flow
        var tokenParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // <- Key: allow expired tokens here
            ValidIssuer = _config["Jwt:Issuer"],
            ValidAudience = _config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
        };

        return new JwtSecurityTokenHandler()
            .ValidateToken(token, tokenParams, out _);
    }
}
```

### 2.4 — DTOs

In `InsightX.Application/DTOs/`:

```csharp
// Auth
public record RegisterDto(string CompanyName, string OwnerName, string Email, string Password);
public record LoginDto(string Email, string Password);
public record RefreshDto(string AccessToken, string RefreshToken);
public record AuthResponseDto(string AccessToken, string RefreshToken);
```

### 2.5 — Auth Endpoints

Implement `POST /auth/register`, `/auth/login`, `/auth/refresh`, `/auth/logout` in `AuthController`:

**Register flow:**
1. Create `Company` → save with `_context.SaveChanges()`
2. Create `ApplicationUser` with `CompanyId`, `Role = "Owner"`
3. Hash password via `UserManager.CreateAsync(user, dto.Password)`
4. Generate tokens → save `RefreshToken` to DB → return `AuthResponseDto`

**Login flow:**
1. Find user by email using `UserManager.FindByEmailAsync()`
2. Verify password using `UserManager.CheckPasswordAsync()`
3. Generate tokens → save `RefreshToken` → return `AuthResponseDto`

**Refresh flow:**
1. Validate expired access token to extract `UserId`
2. Find matching `RefreshToken` in DB using LINQ:
   ```csharp
   var stored = await _context.RefreshTokens
       .FirstOrDefaultAsync(r => r.Token == dto.RefreshToken
           && r.UserId == userId
           && !r.IsRevoked
           && r.ExpiresAt > DateTime.UtcNow);
   ```
3. Revoke old token → issue new pair → return `AuthResponseDto`

**Logout flow:**
1. Extract `UserId` from current JWT claims
2. Mark all user's refresh tokens as revoked:
   ```csharp
   var tokens = await _context.RefreshTokens
       .Where(r => r.UserId == userId && !r.IsRevoked)
       .ToListAsync();
   tokens.ForEach(t => t.IsRevoked = true);
   await _context.SaveChangesAsync();
   ```

### ✅ Phase 2 Done When
- [ ] Register creates company + owner + returns tokens
- [ ] Login validates credentials + returns tokens
- [ ] Refresh issues new pair and revokes old one
- [ ] Logout revokes all tokens for user
- [ ] All tested via Swagger / Postman

---

## Phase 3 — Company, Department & KPI Endpoints

**Goal:** Build the company profile, onboarding, and department management APIs. All endpoints here **must** enforce `CompanyId` isolation via LINQ `.Where()` filters.

### 3.1 — Company Isolation Middleware / Helper

Create a helper to extract `CompanyId` from JWT claims, used across all controllers:

```csharp
public static class ClaimsExtensions
{
    public static int GetCompanyId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue("CompanyId")!);

    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier)!;
}
```

### 3.2 — Company Endpoints

**`GET /companies/me`** — Return current company profile:
```csharp
[Authorize]
public async Task<IActionResult> GetMyCompany()
{
    var companyId = User.GetCompanyId();
    var company = await _context.Companies
        .Include(c => c.Departments)
        .Include(c => c.KPIs)
        .FirstOrDefaultAsync(c => c.Id == companyId);
    return Ok(company);
}
```

**`PUT /companies/setup`** — Save KPIs and thresholds (onboarding step):
```csharp
[Authorize(Roles = "Owner")]
public async Task<IActionResult> Setup([FromBody] SetupDto dto)
{
    var companyId = User.GetCompanyId();

    // Remove old KPIs for this company, then add new ones
    var existing = await _context.KPIs
        .Where(k => k.CompanyId == companyId)
        .ToListAsync();
    _context.KPIs.RemoveRange(existing);

    var kpis = dto.KPIs.Select(k => new KPI
    {
        Name = k.Name,
        Threshold = k.Threshold,
        Unit = k.Unit,
        CompanyId = companyId
    });
    await _context.KPIs.AddRangeAsync(kpis);
    await _context.SaveChangesAsync();
    return Ok();
}
```

### 3.3 — Department Endpoints

**`POST /departments`** — Owner only, scoped to company:
```csharp
[Authorize(Roles = "Owner")]
public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto)
{
    var dept = new Department { Name = dto.Name, CompanyId = User.GetCompanyId() };
    _context.Departments.Add(dept);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetAll), new { id = dept.Id }, dept);
}
```

**`GET /departments`** — Scoped by CompanyId:
```csharp
[Authorize]
public async Task<IActionResult> GetAll()
{
    var companyId = User.GetCompanyId();
    var departments = await _context.Departments
        .Where(d => d.CompanyId == companyId)
        .ToListAsync();
    return Ok(departments);
}
```

### ✅ Phase 3 Done When
- [ ] `GET /companies/me` returns correct company with departments + KPIs
- [ ] `PUT /companies/setup` replaces KPIs correctly
- [ ] `POST /departments` is Owner-only
- [ ] `GET /departments` never returns data from another company
- [ ] Verified with two separate company accounts

---

## Phase 4 — User Management & Role Authorization

**Goal:** Implement user invitation and listing with full role-based access control.

### 4.1 — Role Seeds (Program.cs)

Seed roles on startup:

```csharp
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Owner", "Manager" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}
```

### 4.2 — User Invite Endpoint

**`POST /users/invite`** — Owner only, creates Manager under same company:

```csharp
[Authorize(Roles = "Owner")]
public async Task<IActionResult> Invite([FromBody] InviteUserDto dto)
{
    var companyId = User.GetCompanyId();

    // Ensure department belongs to same company
    var dept = await _context.Departments
        .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId && d.CompanyId == companyId);
    if (dept is null) return BadRequest("Invalid department.");

    var user = new ApplicationUser
    {
        Name = dto.Name,
        Email = dto.Email,
        UserName = dto.Email,
        Role = "Manager",
        CompanyId = companyId,
        DepartmentId = dto.DepartmentId
    };

    var result = await _userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded) return BadRequest(result.Errors);

    await _userManager.AddToRoleAsync(user, "Manager");
    return Ok();
}
```

### 4.3 — User Listing Endpoint

**`GET /users`** — Owner sees all, Manager sees own department only:

```csharp
[Authorize]
public async Task<IActionResult> GetUsers()
{
    var companyId = User.GetCompanyId();
    var role = User.FindFirstValue(ClaimTypes.Role);

    IQueryable<ApplicationUser> query = _context.Users
        .Where(u => u.CompanyId == companyId)
        .Include(u => u.Department);

    if (role == "Manager")
    {
        var deptId = int.Parse(User.FindFirstValue("DepartmentId")!);
        query = query.Where(u => u.DepartmentId == deptId);
    }

    var users = await query
        .Select(u => new { u.Id, u.Name, u.Email, u.Role, u.DepartmentId })
        .ToListAsync();

    return Ok(users);
}
```

### 4.4 — Global Authorization Policy

Register JWT authentication in `Program.cs`:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero // Remove 5-min default tolerance
        };
    });

builder.Services.AddAuthorization();

// In pipeline:
app.UseAuthentication();
app.UseAuthorization();
```

### ✅ Phase 4 Done When
- [ ] Owner can invite Managers, Managers cannot invite
- [ ] Owner sees all users in company; Manager sees own department only
- [ ] Users from Company A can never see users from Company B
- [ ] Role seeds run on startup without errors
- [ ] All 10 endpoints pass end-to-end tests

---

## Security Checklist (applies across all phases)

| Rule | Implementation |
|---|---|
| Passwords never stored plain | `UserManager.CreateAsync(user, password)` — BCrypt handled by Identity |
| JWT carries UserId, CompanyId, DepartmentId, Role | Set in `GenerateAccessToken()` claims |
| Cross-company access blocked | Every LINQ query includes `.Where(x => x.CompanyId == companyId)` |
| Role-based access | `[Authorize(Roles = "Owner")]` on restricted endpoints |
| Access token expiry | 15 minutes, `ClockSkew = TimeSpan.Zero` |
| Refresh token expiry | 7 days, `ExpiresAt` checked in DB query |
| Revocation | `IsRevoked` flag, checked on every refresh + logout |

---

## Dependency Registration Summary (`Program.cs`)

```csharp
// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Custom services
builder.Services.AddScoped<ITokenService, TokenService>();

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...);

builder.Services.AddAuthorization();
```

---

## Endpoint Summary

| Method | Route | Auth | Role |
|---|---|---|---|
| POST | `/auth/register` | ❌ Public | — |
| POST | `/auth/login` | ❌ Public | — |
| POST | `/auth/refresh` | ❌ Public | — |
| POST | `/auth/logout` | ✅ JWT | Any |
| GET | `/companies/me` | ✅ JWT | Any |
| PUT | `/companies/setup` | ✅ JWT | Owner |
| POST | `/departments` | ✅ JWT | Owner |
| GET | `/departments` | ✅ JWT | Any |
| POST | `/users/invite` | ✅ JWT | Owner |
| GET | `/users` | ✅ JWT | Any (filtered by role) |