# 🛠️ Implementation Guide - LookupService Search Methods

## 🐞 Problem

Backend компајл грешка:

```
CS0535: 'LookupService' does not implement interface member 'ILookupService.SearchArticlesAsync(string, int)'
CS0535: 'LookupService' does not implement interface member 'ILookupService.SearchPartnersAsync(string, int)'
```

## ✅ Шта је већ урађено:

1. ✅ `ILookupService` интерфејс - додате методе `SearchPartnersAsync` и `SearchArticlesAsync`
2. ✅ `ApiRoutes` - додате константе `PartnersSearch` и `ArticlesSearch`
3. ✅ `LookupsController` - додати endpoint-и `/partners/search` и `/articles/search`
4. ❌ **`LookupService` - НИСУ имплементиране методе!** ← **OVO TREBATE URADITI!**

---

## 📝 Шта треба урадити:

### Опција 1: Користи Stored Procedure Gateway (препоручено)

**Креирај 2 нове Stored Procedures:**

```sql
-- 1. Partner Search
CREATE PROCEDURE spPartnerSearch
    @SearchTerm NVARCHAR(100),
    @Limit INT = 50
AS
BEGIN
    SELECT TOP (@Limit)
        PartnerID AS IdPartner,
        Naziv AS NazivPartnera,
        Mesto,
        Opis,
        StatusID AS IdStatus,
        NacinOporezivanjaID_Nabavka AS IdNacinOporezivanjaNabavka,
        ObracunAkciza,
        ObracunPorez,
        ReferentID AS IdReferent,
        Sifra AS SifraPartner
    FROM tblPartner
    WHERE StatusNabavka = 'Aktivan'
      AND (Sifra LIKE '%' + @SearchTerm + '%' OR Naziv LIKE '%' + @SearchTerm + '%')
    ORDER BY Naziv
END
GO

-- 2. Article Search
CREATE PROCEDURE spArticleSearch
    @SearchTerm NVARCHAR(100),
    @Limit INT = 50
AS
BEGIN
    SELECT TOP (@Limit)
        ArtikalID AS IdArtikal,
        Sifra AS SifraArtikal,
        Naziv AS NazivArtikla,
        JedinicaMere,
        PoreskaStopaID AS IdPoreskaStopa,
        ProcenatPoreza,
        Akciza,
        KoeficijentKolicine,
        ImaLot,
        OtkupnaCena,
        PoljoprivredniProizvod
    FROM tblArtikal
    WHERE StatusUlaz = 'Aktivan'
      AND (Sifra LIKE '%' + @SearchTerm + '%' OR Naziv LIKE '%' + @SearchTerm + '%')
    ORDER BY Naziv
END
GO
```

**Додај методе у `IStoredProcedureGateway`:**

```csharp
// src/ERPAccounting.Domain/Abstractions/Gateways/IStoredProcedureGateway.cs

Task<IEnumerable<PartnerLookup>> SearchPartnersAsync(string searchTerm, int limit);
Task<IEnumerable<ArticleLookup>> SearchArticlesAsync(string searchTerm, int limit);
```

**Имплементирај у `StoredProcedureGateway`:**

```csharp
// src/ERPAccounting.Infrastructure/Gateways/StoredProcedureGateway.cs

public async Task<IEnumerable<PartnerLookup>> SearchPartnersAsync(string searchTerm, int limit)
{
    var parameters = new[]{
        new SqlParameter("@SearchTerm", searchTerm),
        new SqlParameter("@Limit", limit)
    };
    
    return await ExecuteStoredProcedureAsync<PartnerLookup>(
        "spPartnerSearch",
        parameters
    );
}

public async Task<IEnumerable<ArticleLookup>> SearchArticlesAsync(string searchTerm, int limit)
{
    var parameters = new[]{
        new SqlParameter("@SearchTerm", searchTerm),
        new SqlParameter("@Limit", limit)
    };
    
    return await ExecuteStoredProcedureAsync<ArticleLookup>(
        "spArticleSearch",
        parameters
    );
}
```

**У `LookupService` замени претходну имплементацију са:**

```csharp
// src/ERPAccounting.Application/Services/LookupService.cs

public async Task<List<PartnerComboDto>> SearchPartnersAsync(string searchTerm, int limit = 50)
{
    if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
    {
        return new List<PartnerComboDto>();
    }

    var partners = await _storedProcedureGateway.SearchPartnersAsync(searchTerm, limit);
    return partners.Select(MapToPartnerDto).ToList();
}

public async Task<List<ArticleComboDto>> SearchArticlesAsync(string searchTerm, int limit = 50)
{
    if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
    {
        return new List<ArticleComboDto>();
    }

    var articles = await _storedProcedureGateway.SearchArticlesAsync(searchTerm, limit);
    return articles.Select(MapToArticleDto).ToList();
}
```

---

### Опција 2: Direct SQL (ако немаш Gateway pattern)

**Ако не користиш Gateway**, можеш користити `AppDbContext` и `FromSqlRaw`:

```csharp
// Dodaj dependency
private readonly AppDbContext _dbContext;

public LookupService(
    IStoredProcedureGateway storedProcedureGateway,
    AppDbContext dbContext,  // ← NEW
    ILogger<LookupService> logger)
{
    _storedProcedureGateway = storedProcedureGateway;
    _dbContext = dbContext;
    _logger = logger;
}

// Implementacija
public async Task<List<PartnerComboDto>> SearchPartnersAsync(string searchTerm, int limit = 50)
{
    if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
    {
        return new List<PartnerComboDto>();
    }

    var normalizedTerm = $"%{searchTerm}%";

    var partners = await _dbContext.Database
        .SqlQueryRaw<PartnerLookup>(
            @"SELECT TOP (@p1)
                PartnerID AS IdPartner,
                Naziv AS NazivPartnera,
                Mesto,
                Opis,
                StatusID AS IdStatus,
                NacinOporezivanjaID_Nabavka AS IdNacinOporezivanjaNabavka,
                ObracunAkciza,
                ObracunPorez,
                ReferentID AS IdReferent,
                Sifra AS SifraPartner
            FROM tblPartner
            WHERE StatusNabavka = 'Aktivan'
              AND (Sifra LIKE @p0 OR Naziv LIKE @p0)
            ORDER BY Naziv",
            normalizedTerm,
            limit
        )
        .ToListAsync();

    return partners.Select(MapToPartnerDto).ToList();
}
```

---

## 🛠️ Кораци за имплементацију:

### 1. Креирај Stored Procedures

```bash
# Отвори SQL Server Management Studio
# Повежи се на базу
# Покрени горе наведене CREATE PROCEDURE скрипте
```

### 2. Додај методе у Gateway интерфејс

```bash
cd accounting-online-backend
# Edit: src/ERPAccounting.Domain/Abstractions/Gateways/IStoredProcedureGateway.cs
# Додај: SearchPartnersAsync и SearchArticlesAsync
```

### 3. Имплементирај у Gateway

```bash
# Edit: src/ERPAccounting.Infrastructure/Gateways/StoredProcedureGateway.cs
# Имплементирај методе
```

### 4. Имплементирај у LookupService

```bash
# Edit: src/ERPAccounting.Application/Services/LookupService.cs
# Замени постојећу имплементацију (која користи Dapper) са Gateway позивима
```

### 5. Тестирај

```bash
dotnet build
dotnet run --project src/ERPAccounting.API
```

### 6. Провери Swagger

```
http://localhost:5286/swagger
```

**Потражи:**
- `GET /api/v1/lookups/partners/search?query=sim&limit=50`
- `GET /api/v1/lookups/articles/search?query=crna&limit=50`

---

## ✅ После имплементације:

1. ✅ Merguj овај Backend PR
2. ✅ Merguj Frontend PR #36
3. ✅ Restart оба сервера
4. ✅ Тестирај на `http://localhost:3000/documents/vp/ur`

---

**Тренутно стање:** LookupService.cs има Dapper имплементацију која **неће радити** јер `IDbConnection` није регистрован у DI.

**Замени са Gateway pattern имплементацијом горе!** 👆
