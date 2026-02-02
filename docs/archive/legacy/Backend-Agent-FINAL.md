# 👨‍💻 BACKEND AGENT - FINALNA SPECIFIKACIJA v4.0

**Verzija:** 4.0 - KOMPLETNA FINALNA VERZIJA  
**Datum:** 17.11.2025  
**Projekat:** ERP Accounting Online Backend  
**Stack:** .NET 8.0 LTS + ASP.NET Core + Entity Framework Core + SQL Server  
**Repozitorijum:** https://github.com/ykliugi-beep/accounting-online-backend

---

## 📋 SVRHA DOKUMENTA

Ovaj dokument definiše **kompletnu i finalnu specifikaciju** za Backend deo ERP Accounting Online sistema. Sve detalje, pravila, standarde i najbolje prakse koje AI asistent (GitHub Copilot, ChatGPT, Claude) **MORA** slediti.

**KRITIČNO:** 
- Svi kodovi moraju biti usklađeni sa database modelom
- Sve tabele, atributi i SP-ovi moraju biti tačno mapirani
- Konkurentnost mehanizam MORA biti implementiran sa RowVersion/ETag
- Nema pretpostavki - sve je eksplicitno definisano

---

## 🏗️ CLEAN ARCHITECTURE - 4 SLOJA

```
Solution/
├── src/
│   ├── ERPAccounting.API/              # Layer 1: Presentation
│   │   ├── Controllers/
│   │   │   ├── DocumentsController.cs
│   │   │   ├── DocumentItemsController.cs    ← ETag konkurentnost
│   │   │   ├── DocumentCostsController.cs    ← ETag konkurentnost
│   │   │   └── LookupsController.cs          ← 11 SP endpointa
│   │   ├── Middleware/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── ERPAccounting.Application/      # Layer 2: Business Logic
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Validators/
│   │   └── Mapping/
│   │
│   ├── ERPAccounting.Domain/           # Layer 3: Domain Entities
│   │   └── Entities/
│   │       ├── Document.cs               ← 86 svojstava
│   │       ├── DocumentLineItem.cs       ← 65 svojstava + RowVersion
│   │       └── DocumentCostLineItem.cs   ← 14 svojstava + RowVersion
│   │
│   └── ERPAccounting.Infrastructure/   # Layer 4: Data Access
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── ModelConfiguration/
│       └── Repositories/
```

---

## 📊 DATABASE MODEL - KRITIČNO!

### DocumentLineItem Entity - 65 Atributa

```csharp
public class DocumentLineItem
{
    // PK i FK
    public int Id { get; set; }  // IDStavkaDokumenta
    public int DocumentId { get; set; }  // IDDokument (CASCADE)
    public int ArticleId { get; set; }  // IDArtikal (obavezno)
    public int? OrganizationalUnitId { get; set; }
    
    // Količine i cene - money tipovi
    public decimal Quantity { get; set; }  // CHECK <> 0
    public decimal InvoicePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal WarehousePrice { get; set; }
    public decimal DocumentDiscount { get; set; }
    public decimal ActiveMatterPercent { get; set; }
    public decimal Volume { get; set; }
    public decimal Excise { get; set; }
    public decimal QuantityCoefficient { get; set; }  // default 1
    public decimal DiscountAmount { get; set; }
    public decimal MarginAmount { get; set; }
    public decimal MarginValue { get; set; }
    
    // PDV i akciza
    public decimal TaxPercent { get; set; }
    public decimal TaxPercentMP { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxAmountWithExcise { get; set; }
    public decimal ExciseAmount { get; set; }
    public string? TaxRateId { get; set; }  // char(2)
    
    // Zavisni troškovi
    public decimal DependentCostsWithTax { get; set; }
    public decimal DependentCostsWithoutTax { get; set; }
    
    // Ukupni iznosi
    public decimal Total { get; set; }  // COMPUTED
    public decimal CurrencyPrice { get; set; }
    public decimal CurrencyTotal { get; set; }
    
    // JM i pakovanje
    public string UnitOfMeasureId { get; set; } = string.Empty;
    public int Packaging { get; set; }
    
    // Obračuni
    public bool CalculateExcise { get; set; }
    public bool CalculateTax { get; set; }
    public bool CalculateAuxiliaryTax { get; set; }
    public int? TaxationMethodId { get; set; }
    public int? StatusId { get; set; }
    
    // Masa i opis
    public decimal Weight { get; set; }
    public string? Description { get; set; }
    
    // Proizvodnja
    public double ProductionQuantity { get; set; }
    public string? ProductionUnitOfMeasureId { get; set; }
    public double ProductionQuantityCoefficient { get; set; }
    public int? MealOrderLineId { get; set; }
    public int? MealTypeId { get; set; }
    
    // Dnevna stanja
    public int DailyInventoryChangeM1 { get; set; }
    public int DailyInventoryChangeM2 { get; set; }
    public int DailyGoodsChangeM1 { get; set; }
    public int DailyGoodsChangeM2 { get; set; }
    public int DailyVPChangeM1 { get; set; }
    public int DailyVPChangeM2 { get; set; }
    
    // Dodatni rabati
    public int? BaseAccountId { get; set; }
    public decimal ActionDiscount { get; set; }
    public bool? DeliveryOfGoods { get; set; }
    public decimal Discount2 { get; set; }
    public decimal? LastPurchasePrice { get; set; }
    public decimal? AveragePrice { get; set; }
    
    // Valuta
    public int? CurrencyDays { get; set; }
    public DateTime? CurrencyDate { get; set; }
    public decimal? PriceWithoutTax { get; set; }
    
    // Oprema
    public string? MandatoryEquipment { get; set; }
    public string? SupplementaryEquipment { get; set; }
    public decimal? AveragePriceUJ { get; set; }
    public decimal? ReturnAmount { get; set; }
    public decimal? OldPrice { get; set; }
    public int? ColorId { get; set; }
    
    // ═══════════════════════════════════════════════════════════════
    // KONKURENTNOST - OBAVEZNO!
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    // ═══════════════════════════════════════════════════════════════
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
}
```

---

## 🔒 KONKURENTNOST - ETAG MEHANIZAM

### Controller sa ETag

```csharp
[HttpPatch("{itemId:int}")]
public async Task<ActionResult<DocumentLineItemDto>> UpdateItem(
    int documentId,
    int itemId,
    [FromBody] PatchLineItemDto dto)
{
    // 1. Proveri If-Match header - OBAVEZNO!
    var ifMatch = Request.Headers["If-Match"].FirstOrDefault();
    if (string.IsNullOrEmpty(ifMatch))
        return BadRequest(new { message = "Missing If-Match header" });

    // 2. Dekoduj Base64 ETag
    byte[] expectedRowVersion;
    try
    {
        expectedRowVersion = Convert.FromBase64String(ifMatch.Trim('"'));
    }
    catch
    {
        return BadRequest(new { message = "Invalid ETag format" });
    }

    try
    {
        var result = await _service.UpdateAsync(documentId, itemId, expectedRowVersion, dto);
        
        // OBAVEZNO: Postavi novi ETag
        Response.Headers.ETag = $"\"{result.ETag}\"";
        return Ok(result);
    }
    catch (ConflictException ex)
    {
        _logger.LogWarning("Konflikt: {Message}", ex.Message);
        return Conflict(new { message = "Stavka promenjena" });
    }
}
```

### Service sa Konkurentnosti

```csharp
public async Task<DocumentLineItemDto> UpdateAsync(
    int documentId,
    int itemId,
    byte[] expectedRowVersion,
    PatchLineItemDto dto)
{
    // 1. Validacija
    var validationResult = await _validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
        throw new ValidationException(validationResult.Errors);

    // 2. Učitaj stavku
    var item = await _repository.GetByIdAsync(itemId);
    if (item == null || item.DocumentId != documentId)
        throw new NotFoundException("Stavka nije pronađena");

    // 3. KONKURENTNOST PROVERA - KRITIČNO!
    if (!item.RowVersion.SequenceEqual(expectedRowVersion))
    {
        _logger.LogWarning("Konflikt: RowVersion mismatch za item {ItemId}", itemId);
        throw new ConflictException("Stavka je promenjena od drugog korisnika");
    }

    // 4. Update
    _mapper.Map(dto, item);
    item.UpdatedAt = DateTime.UtcNow;
    item.UpdatedBy = _currentUser.UserId;

    _repository.Update(item);
    await _repository.SaveChangesAsync();  // SQL ažurira RowVersion!

    return _mapper.Map<DocumentLineItemDto>(item);
}
```

---

## 📊 STORED PROCEDURES - 11 SP-OVA

```csharp
public class StoredProcedureService : IStoredProcedureService
{
    private readonly AppDbContext _context;

    // 1. spPartnerComboStatusNabavka
    public async Task<List<PartnerComboDto>> GetPartnerComboAsync()
    {
        return await _context.Database
            .SqlQueryRaw<PartnerComboDto>("EXECUTE spPartnerComboStatusNabavka")
            .ToListAsync();
    }

    // 2. spOrganizacionaJedinicaCombo
    public async Task<List<OrgUnitComboDto>> GetOrgUnitsComboAsync(string docTypeId)
    {
        return await _context.Database
            .SqlQueryRaw<OrgUnitComboDto>(
                "EXECUTE spOrganizacionaJedinicaCombo @IDVrstaDokumenta = {0}",
                docTypeId)
            .ToListAsync();
    }

    // ... (ostalih 9 SP-ova)
}
```

---

## 📅 FAZE IMPLEMENTACIJE

### FAZA 1: BACKEND CORE (Dani 2-5)
- [ ] Domain entiteti sa SVIM atributima
- [ ] DbContext i EF Core konfiguracija
- [ ] Repository pattern
- [ ] DTOs i AutoMapper
- [ ] FluentValidation

### FAZA 2: KONKURENTNOST (Dani 10-12) - KRITIČNO!
- [ ] PATCH sa ETag
- [ ] If-Match header validacija
- [ ] RowVersion provera
- [ ] 409 Conflict response
- [ ] Unit testovi

---

## ✅ DEFINITION OF DONE

### Za Konkurentnost Features
- [ ] RowVersion mapiran u entitetu
- [ ] ETag vraćan u Response header-u
- [ ] If-Match header obavezan u PATCH-u
- [ ] 409 Conflict response implementiran
- [ ] Unit test za mismatch scenario
- [ ] E2E test sa 2 korisnika prolazi

---

**Za kompletnu dokumentaciju pogledaj:**
- Repository-Status-Analiza.md
- ERP-SPECIFIKACIJA-FINAL.md

**BACKEND AGENT v4.0 - FINALNA VERZIJA** ✅