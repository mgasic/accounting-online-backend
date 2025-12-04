# Partners and Articles Search Fix - Implementation Documentation

## Overview

This document describes the fix implemented for server-side search functionality for Partners and Articles lookups, addressing performance issues with large datasets (6000+ partners, 11000+ articles).

## Problem Statement

### Original Implementation Issues

1. **Incorrect Column Names**: The `SearchPartnersAsync` and `SearchArticlesAsync` methods in `StoredProcedureGateway.cs` were using non-existent column names:
   - Partners: Used `PartnerID`, `Naziv`, `Sifra` instead of `IDPartner`, `NazivPartnera`, `SifraPartner`
   - Articles: Used `ArtikalID`, `Naziv`, `Sifra` instead of `IDArtikal`, `NazivArtikla`, `SifraArtikal`

2. **Missing JOIN Operations**: The queries did not include necessary JOINs:
   - Partners: Missing JOINs to `tblMesto` and `tblStatus` required for complete data
   - Articles: Missing JOIN to `tblPoreskaStopa` for tax rate information

3. **Inconsistent Data Structure**: Search results did not match the structure returned by stored procedures, causing DTO mapping failures

## Solution Implemented

### Database Schema Reference

Based on analysis of `tblSifrarnici.txt` and stored procedures in `spDocuments.txt`:

**tblPartner (Partners):**
- Primary Key: `IDPartner` (int, IDENTITY)
- Search Fields: `SifraPartner` (varchar(13)), `NazivPartnera` (varchar(255))
- Foreign Keys: `IDMesto` → `tblMesto`, `IDStatus` → `tblStatus`

**tblArtikal (Articles):**
- Primary Key: `IDArtikal` (int, IDENTITY)  
- Search Fields: `SifraArtikal` (varchar), `NazivArtikla` (varchar(255))
- Foreign Key: `IDPoreskaStopa` → `tblPoreskaStopa`
- Sort Field: `SifraSort` (varchar(255))

### Fixed Implementation

#### SearchPartnersAsync

```sql
SELECT TOP (@limit)
    p.NazivPartnera AS [NAZIV PARTNERA],
    m.NazivMesta AS MESTO,
    p.IDPartner,
    s.Opis,
    p.IDStatus,
    s.IDNacinOporezivanjaNabavka,
    s.ObracunAkciza,
    s.ObracunPorez,
    p.IDReferent,
    p.SifraPartner AS [ŠIFRA]
FROM dbo.tblPartner p
INNER JOIN dbo.tblStatus s ON p.IDStatus = s.IDStatus
LEFT OUTER JOIN dbo.tblMesto m ON p.IDMesto = m.IDMesto
WHERE (p.SifraPartner LIKE @searchTerm OR p.NazivPartnera LIKE @searchTerm)
ORDER BY p.NazivPartnera
```

**Key Features:**
- Matches exact structure of `spPartnerComboStatusNabavka`
- Uses correct column names from `tblPartner`
- Includes all required JOINs for complete data
- Column aliases match `PartnerLookup` model expectations
- Searches both code (`SifraPartner`) and name (`NazivPartnera`)

#### SearchArticlesAsync

```sql
SELECT TOP (@limit)
    a.IDArtikal,
    a.SifraArtikal AS SIFRA,
    a.NazivArtikla AS [NAZIV ARTIKLA],
    a.IDJedinicaMere AS JM,
    a.IDPoreskaStopa,
    ps.ProcenatPoreza,
    a.Akciza,
    a.KoeficijentKolicine,
    a.ImaLot,
    a.OtkupnaCena,
    a.PoljoprivredniProizvod
FROM dbo.tblArtikal a
INNER JOIN dbo.tblPoreskaStopa ps ON a.IDPoreskaStopa = ps.IDPoreskaStopa
WHERE (a.SifraArtikal LIKE @searchTerm OR a.NazivArtikla LIKE @searchTerm)
ORDER BY a.SifraSort
```

**Key Features:**
- Matches exact structure of `spArtikalComboUlaz`
- Uses correct column names from `tblArtikal`
- Includes JOIN to `tblPoreskaStopa` for tax information
- Column aliases match `ArticleLookup` model expectations
- Orders by `SifraSort` (same as stored procedure)
- Searches both code (`SifraArtikal`) and name (`NazivArtikla`)

## Architecture

### Data Flow

```
┌─────────────────┐
│  Frontend       │
│  Autocomplete   │
└────────┬────────┘
         │ HTTP GET /api/v1/lookups/partners/search?query=abc&limit=50
         ▼
┌─────────────────┐
│ LookupsController│
│  (API Layer)    │
└────────┬────────┘
         │ SearchPartnersAsync()
         ▼
┌─────────────────┐
│  LookupService  │
│  (Application)  │
└────────┬────────┘
         │ SearchPartnersAsync()
         ▼
┌─────────────────────┐
│ StoredProcedureGateway│ ← FIXED HERE
│  (Infrastructure)    │
└────────┬─────────────┘
         │ SQL Query (with JOINs)
         ▼
┌─────────────────┐
│  SQL Server     │
│  Database       │
└─────────────────┘
```

### Files Modified

1. **`src/ERPAccounting.Infrastructure/Services/StoredProcedureGateway.cs`**
   - Fixed `SearchPartnersAsync()` method
   - Fixed `SearchArticlesAsync()` method
   - Both methods now use correct database schema

### Files Already Correct (No Changes Needed)

1. **`src/ERPAccounting.Domain/Lookups/LookupModels.cs`**
   - `PartnerLookup` record with correct column mappings
   - `ArticleLookup` record with correct column mappings

2. **`src/ERPAccounting.Application/Services/LookupService.cs`**
   - `SearchPartnersAsync()` implementation correct
   - `SearchArticlesAsync()` implementation correct

3. **`src/ERPAccounting.API/Controllers/LookupsController.cs`**
   - Search endpoints already implemented correctly
   - Validation logic correct

4. **`src/ERPAccounting.Common/Constants/ApiRoutes.cs`**
   - Route constants already defined correctly

## API Endpoints

### Partners Search

```http
GET /api/v1/lookups/partners/search?query={term}&limit={n}
```

**Parameters:**
- `query` (required): Search term, minimum 2 characters
- `limit` (optional): Max results, default 50, max 100

**Response:**
```json
[
  {
    "idPartner": 123,
    "nazivPartnera": "ACME d.o.o.",
    "mesto": "Beograd",
    "opis": "Aktivan",
    "idStatus": 1,
    "idNacinOporezivanjaNabavka": 1,
    "obracunAkciza": 0,
    "obracunPorez": 1,
    "idReferent": 5,
    "sifraPartner": "P001"
  }
]
```

### Articles Search

```http
GET /api/v1/lookups/articles/search?query={term}&limit={n}
```

**Parameters:**
- `query` (required): Search term, minimum 2 characters
- `limit` (optional): Max results, default 50, max 100

**Response:**
```json
[
  {
    "idArtikal": 456,
    "sifraArtikal": "ART001",
    "nazivArtikla": "Proizvod XYZ",
    "jedinicaMere": "kom",
    "idPoreskaStopa": "01",
    "procenatPoreza": 20.0,
    "akciza": 0.0,
    "koeficijentKolicine": 1.0,
    "imaLot": false,
    "otkupnaCena": 100.50,
    "poljoprivredniProizvod": false
  }
]
```

## Performance Considerations

### Database Indexes

For optimal performance, ensure the following indexes exist:

```sql
-- Partners search optimization
CREATE INDEX IX_tblPartner_SifraPartner ON tblPartner(SifraPartner);
CREATE INDEX IX_tblPartner_NazivPartnera ON tblPartner(NazivPartnera);

-- Articles search optimization  
CREATE INDEX IX_tblArtikal_SifraArtikal ON tblArtikal(SifraArtikal);
CREATE INDEX IX_tblArtikal_NazivArtikla ON tblArtikal(NazivArtikla);
CREATE INDEX IX_tblArtikal_SifraSort ON tblArtikal(SifraSort);
```

### Query Performance

- **TOP clause**: Limits results at database level
- **LIKE with prefix**: Using `%term%` for flexibility (consider `term%` for better performance if acceptable)
- **JOIN optimization**: Uses indexed foreign keys
- **Minimal data transfer**: Returns only required columns

### Expected Performance

- **Partners (6000 records)**: < 500ms response time
- **Articles (11000 records)**: < 500ms response time
- **Network payload**: ~5-10KB per request (50 results)

## Testing

### Unit Tests

Test cases should verify:

1. **Correct column names**: Verify DTO mapping succeeds
2. **JOIN correctness**: Verify all fields populated
3. **Search functionality**: Verify both code and name searches work
4. **Limit enforcement**: Verify TOP clause works
5. **Case insensitivity**: Verify LIKE search is case-insensitive

### Integration Tests

```csharp
[Fact]
public async Task SearchPartners_WithValidTerm_ReturnsMatchingPartners()
{
    // Arrange
    var searchTerm = "ACME";
    var limit = 10;

    // Act
    var result = await _gateway.SearchPartnersAsync(searchTerm, limit);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.Count <= limit);
    Assert.All(result, p => 
        Assert.True(
            p.SifraPartner.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.NazivPartnera.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        )
    );
}
```

### Manual Testing

1. **Test with Postman/curl:**
```bash
curl "http://localhost:5098/api/v1/lookups/partners/search?query=acme&limit=10"
curl "http://localhost:5098/api/v1/lookups/articles/search?query=prod&limit=10"
```

2. **Verify response structure matches DTOs**
3. **Test with production data volumes**
4. **Measure response times**

## Frontend Integration

### Required Changes

The frontend autocomplete components need to:

1. **Call search endpoints** instead of loading all data
2. **Pass query parameter** with user input
3. **Debounce input** (300ms recommended)
4. **Handle empty results** gracefully

### Example React Component Usage

```typescript
const [partners, setPartners] = useState<Partner[]>([]);
const [loading, setLoading] = useState(false);

const searchPartners = async (query: string) => {
  if (query.length < 2) {
    setPartners([]);
    return;
  }

  setLoading(true);
  try {
    const response = await api.get('/api/v1/lookups/partners/search', {
      params: { query, limit: 50 }
    });
    setPartners(response.data);
  } catch (error) {
    console.error('Search failed:', error);
    setPartners([]);
  } finally {
    setLoading(false);
  }
};

const debouncedSearch = useMemo(
  () => debounce(searchPartners, 300),
  []
);
```

## Verification Checklist

- [x] SQL queries use correct database column names
- [x] JOINs to related tables included
- [x] Column aliases match DTO property names
- [x] Query structure matches stored procedures
- [x] Search covers both code and name fields
- [x] TOP clause limits results
- [x] ORDER BY matches stored procedure behavior
- [x] Error handling implemented
- [x] Logging added for debugging
- [ ] Database indexes verified/created
- [ ] Unit tests added
- [ ] Integration tests added
- [ ] Performance testing completed
- [ ] Frontend integration completed
- [ ] End-to-end testing completed

## Rollback Plan

If issues arise:

1. **Revert Git commit**: `git revert <commit-sha>`
2. **Frontend continues using** full dataset endpoints temporarily
3. **Monitor logs** for error patterns
4. **Fix issues** in new branch
5. **Re-deploy** after verification

## Related Documentation

- `docs/api/MAPPING-VERIFICATION.md` - DTO mapping verification
- `docs/api/AUTOCOMPLETE_SEARCH_IMPLEMENTATION.md` - Original search implementation guide
- `tblSifrarnici.txt` - Database schema source file
- `spDocuments.txt` - Stored procedures source file

## Contributors

- **Backend Fix**: Implemented by AI Development Team
- **Issue Identified By**: User feedback on autocomplete performance
- **Database Schema**: Based on tblSifrarnici.txt analysis

## Change Log

### 2025-12-04 - Initial Fix

- Fixed `SearchPartnersAsync` query to use correct columns
- Fixed `SearchArticlesAsync` query to use correct columns
- Added proper JOINs to related tables
- Aligned column aliases with DTO models
- Added comprehensive documentation

---

**Status**: ✅ Ready for Testing  
**Next Step**: Frontend Integration  
**Priority**: 🔴 Critical (Blocking Production Use)
