using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ERPAccounting.Infrastructure.Data;
using ERPAccounting.Domain.Entities;
using ERPAccounting.Application.DTOs;

namespace ERPAccounting.API.Controllers
{
    /// <summary>
    /// Document Line Items Controller
    /// KRITIČNO: Implementira ETag konkurentnost mehanizam sa RowVersion
    /// 
    /// WORKFLOW:
    /// 1. GET /api/v1/documents/{id}/items/{itemId}
    ///    Response: ETag header sa Base64(RowVersion)
    /// 
    /// 2. Korisnik deli stavku na nekoliko sekundi
    /// 
    /// 3. PATCH /api/v1/documents/{id}/items/{itemId}
    ///    Header: If-Match: \"{BASE64_ETAG}\"  (OBAVEZNO!)
    ///    Ako RowVersion != If-Match => 409 Conflict
    ///    Ako OK => Novi ETag u response
    /// </summary>
    [ApiController]
    [Route("api/v1/documents/{documentId:int}/items")]
    [Authorize]
    public class DocumentLineItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DocumentLineItemsController> _logger;

        public DocumentLineItemsController(
            AppDbContext context,
            ILogger<DocumentLineItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════
        // GET OPERACIJE
        
        /// <summary>
        /// GET /api/v1/documents/{documentId}/items
        /// Vraća sve stavke dokumenta sa ETag
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<DocumentLineItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DocumentLineItemDto>>> GetItems(int documentId)
        {
            try
            {
                var items = await _context.DocumentLineItems
                    .Where(i => i.IDDokument == documentId && !i.IsDeleted)
                    .ToListAsync();

                var dtos = items.Select(i => new DocumentLineItemDto(
                    Id: i.IDStavkaDokumenta,
                    DocumentId: i.IDDokument,
                    ArticleId: i.IDArtikal,
                    Quantity: i.Kolicina,
                    InvoicePrice: i.FakturnaCena,
                    DiscountAmount: i.RabatDokument,
                    MarginAmount: i.Marza,
                    TaxRateId: i.IDPoreskaStopa,
                    TaxPercent: i.ProcenatPoreza,
                    TaxAmount: i.IznosPDV,
                    Total: i.Iznos,
                    CalculateExcise: i.ObracunAkciza == 1,
                    CalculateTax: i.ObracunPorez == 1,
                    Description: i.Opis,
                    ETag: Convert.ToBase64String(i.StavkaDokumentaTimeStamp),
                    CreatedAt: i.CreatedAt,
                    UpdatedAt: i.UpdatedAt,
                    CreatedBy: i.CreatedBy,
                    UpdatedBy: i.UpdatedBy
                )).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting items for document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Greška pri učitavanju stavki" });
            }
        }

        /// <summary>
        /// GET /api/v1/documents/{documentId}/items/{itemId}
        /// Vraća jednu stavku sa ETag u response header-u
        /// Response header: ETag: \"{BASE64_ROWVERSION}\"
        /// </summary>
        [HttpGet("{itemId:int}")]
        [ProducesResponseType(typeof(DocumentLineItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DocumentLineItemDto>> GetItem(int documentId, int itemId)
        {
            try
            {
                var item = await _context.DocumentLineItems
                    .FirstOrDefaultAsync(i => i.IDStavkaDokumenta == itemId && i.IDDokument == documentId && !i.IsDeleted);

                if (item == null)
                    return NotFound(new { message = "Stavka nije pronađena" });

                // ══════════════════════════════════════════════════
                // OBAVEZNO: ETag u response header-u
                var etag = Convert.ToBase64String(item.StavkaDokumentaTimeStamp);
                Response.Headers.Add("ETag", $"\"{etag}\"");
                // ══════════════════════════════════════════════════

                var dto = new DocumentLineItemDto(
                    Id: item.IDStavkaDokumenta,
                    DocumentId: item.IDDokument,
                    ArticleId: item.IDArtikal,
                    Quantity: item.Kolicina,
                    InvoicePrice: item.FakturnaCena,
                    DiscountAmount: item.RabatDokument,
                    MarginAmount: item.Marza,
                    TaxRateId: item.IDPoreskaStopa,
                    TaxPercent: item.ProcenatPoreza,
                    TaxAmount: item.IznosPDV,
                    Total: item.Iznos,
                    CalculateExcise: item.ObracunAkciza == 1,
                    CalculateTax: item.ObracunPorez == 1,
                    Description: item.Opis,
                    ETag: etag,
                    CreatedAt: item.CreatedAt,
                    UpdatedAt: item.UpdatedAt,
                    CreatedBy: item.CreatedBy,
                    UpdatedBy: item.UpdatedBy
                );

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item {ItemId}", itemId);
                return StatusCode(500, new { message = "Greška pri učitavanju stavke" });
            }
        }

        // ══════════════════════════════════════════════════
        // CREATE OPERACIJA

        /// <summary>
        /// POST /api/v1/documents/{documentId}/items
        /// Kreiraj novu stavku dokumenta
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(DocumentLineItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DocumentLineItemDto>> CreateItem(int documentId, [FromBody] CreateLineItemDto dto)
        {
            try
            {
                // Proveri dokument
                var doc = await _context.Documents.FindAsync(documentId);
                if (doc == null)
                    return NotFound(new { message = "Dokument nije pronađen" });

                // Kreiraj stavku
                var item = new DocumentLineItem
                {
                    IDDokument = documentId,
                    IDArtikal = dto.ArticleId,
                    Kolicina = dto.Quantity,
                    FakturnaCena = dto.InvoicePrice,
                    RabatDokument = dto.DiscountAmount ?? 0,
                    Marza = dto.MarginAmount ?? 0,
                    IDPoreskaStopa = dto.TaxRateId,
                    ObracunAkciza = (short)(dto.CalculateExcise ? 1 : 0),
                    ObracunPorez = (short)(dto.CalculateTax ? 1 : 0),
                    Opis = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.DocumentLineItems.Add(item);
                await _context.SaveChangesAsync();

                var etag = Convert.ToBase64String(item.StavkaDokumentaTimeStamp);
                Response.Headers.Add("ETag", $"\"{etag}\"");

                var responseDto = new DocumentLineItemDto(
                    Id: item.IDStavkaDokumenta,
                    DocumentId: item.IDDokument,
                    ArticleId: item.IDArtikal,
                    Quantity: item.Kolicina,
                    InvoicePrice: item.FakturnaCena,
                    DiscountAmount: item.RabatDokument,
                    MarginAmount: item.Marza,
                    TaxRateId: item.IDPoreskaStopa,
                    TaxPercent: 0,
                    TaxAmount: 0,
                    Total: item.Iznos,
                    CalculateExcise: item.ObracunAkciza == 1,
                    CalculateTax: item.ObracunPorez == 1,
                    Description: item.Opis,
                    ETag: etag,
                    CreatedAt: item.CreatedAt,
                    UpdatedAt: item.UpdatedAt,
                    CreatedBy: item.CreatedBy,
                    UpdatedBy: item.UpdatedBy
                );

                return CreatedAtAction(nameof(GetItem), new { documentId, itemId = item.IDStavkaDokumenta }, responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item for document {DocumentId}", documentId);
                return StatusCode(500, new { message = "Greška pri kreiranju stavke" });
            }
        }

        // ══════════════════════════════════════════════════
        // PATCH OPERACIJA - KRITIČNA ZA KONKURENTNOST!

        /// <summary>
        /// PATCH /api/v1/documents/{documentId}/items/{itemId}
        /// Ažurira stavku sa ETag konkurentnosti
        /// 
        /// OBAVEZNO: Header If-Match sa ETag vrednosti
        /// If-Match: \"{BASE64_ROWVERSION}\"
        /// 
        /// Odgovori:
        /// 200 OK - Ažuriranje uspešno, novi ETag u response
        /// 409 Conflict - RowVersion mismatch (stavka promenjena)
        /// </summary>
        [HttpPatch("{itemId:int}")]
        [ProducesResponseType(typeof(DocumentLineItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DocumentLineItemDto>> UpdateItem(
            int documentId,
            int itemId,
            [FromBody] PatchLineItemDto dto)
        {
            try
            {
                // ══════════════════════════════════════════════════
                // OBAVEZNO: Proveri If-Match header
                var ifMatch = Request.Headers["If-Match"].FirstOrDefault();
                if (string.IsNullOrEmpty(ifMatch))
                {
                    _logger.LogWarning("Missing If-Match header for PATCH");
                    return BadRequest(new { message = "Missing If-Match header (ETag required)" });
                }

                // Dekoduj Base64 ETag u RowVersion
                byte[] expectedRowVersion;
                try
                {
                    // Ukloni navodike ako postoje
                    var etagValue = ifMatch.Trim('"');
                    expectedRowVersion = Convert.FromBase64String(etagValue);
                }
                catch
                {
                    _logger.LogWarning("Invalid ETag format: {ETag}", ifMatch);
                    return BadRequest(new { message = "Invalid ETag format" });
                }
                // ══════════════════════════════════════════════════

                // Pronađi stavku
                var item = await _context.DocumentLineItems
                    .FirstOrDefaultAsync(i => i.IDStavkaDokumenta == itemId && i.IDDokument == documentId && !i.IsDeleted);

                if (item == null)
                    return NotFound(new { message = "Stavka nije pronađena" });

                // ══════════════════════════════════════════════════
                // KRITIČNO: KONKURENTNOST PROVERA!
                // Ako RowVersion ne odgovara -> 409 Conflict
                if (!item.StavkaDokumentaTimeStamp.SequenceEqual(expectedRowVersion))
                {
                    _logger.LogWarning(
                        "Concurrency conflict for item {ItemId}: expected={Expected}, actual={Actual}",
                        itemId,
                        Convert.ToBase64String(expectedRowVersion),
                        Convert.ToBase64String(item.StavkaDokumentaTimeStamp)
                    );

                    // 409 Conflict
                    return Conflict(new
                    {
                        message = "Stavka je promenjena od drugog korisnika",
                        detail = "Osvežite stranicu ili izaberite opciju 'Prepiši'",
                        currentETag = Convert.ToBase64String(item.StavkaDokumentaTimeStamp),
                        timestamp = DateTime.UtcNow
                    });
                }
                // ══════════════════════════════════════════════════

                // Ažuriramo samo polja koja su prosleđena
                if (dto.Quantity.HasValue)
                    item.Kolicina = dto.Quantity.Value;

                if (dto.InvoicePrice.HasValue)
                    item.FakturnaCena = dto.InvoicePrice.Value;

                if (dto.DiscountAmount.HasValue)
                    item.RabatDokument = dto.DiscountAmount.Value;

                if (dto.MarginAmount.HasValue)
                    item.Marza = dto.MarginAmount.Value;

                if (dto.TaxRateId != null)
                    item.IDPoreskaStopa = dto.TaxRateId;

                if (dto.CalculateExcise.HasValue)
                    item.ObracunAkciza = (short)(dto.CalculateExcise.Value ? 1 : 0);

                if (dto.CalculateTax.HasValue)
                    item.ObracunPorez = (short)(dto.CalculateTax.Value ? 1 : 0);

                if (dto.Description != null)
                    item.Opis = dto.Description;

                item.UpdatedAt = DateTime.UtcNow;

                // SaveChanges će automatski ažurirati RowVersion!
                await _context.SaveChangesAsync();

                // ══════════════════════════════════════════════════
                // OBAVEZNO: Novi ETag u response!
                var newETag = Convert.ToBase64String(item.StavkaDokumentaTimeStamp);
                Response.Headers.Add("ETag", $"\"{newETag}\"");
                // ══════════════════════════════════════════════════

                var responseDto = new DocumentLineItemDto(
                    Id: item.IDStavkaDokumenta,
                    DocumentId: item.IDDokument,
                    ArticleId: item.IDArtikal,
                    Quantity: item.Kolicina,
                    InvoicePrice: item.FakturnaCena,
                    DiscountAmount: item.RabatDokument,
                    MarginAmount: item.Marza,
                    TaxRateId: item.IDPoreskaStopa,
                    TaxPercent: item.ProcenatPoreza,
                    TaxAmount: item.IznosPDV,
                    Total: item.Iznos,
                    CalculateExcise: item.ObracunAkciza == 1,
                    CalculateTax: item.ObracunPorez == 1,
                    Description: item.Opis,
                    ETag: newETag,
                    CreatedAt: item.CreatedAt,
                    UpdatedAt: item.UpdatedAt,
                    CreatedBy: item.CreatedBy,
                    UpdatedBy: item.UpdatedBy
                );

                return Ok(responseDto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error updating item {ItemId}", itemId);
                return StatusCode(500, new { message = "Greška pri ažuriranju stavke" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item {ItemId}", itemId);
                return StatusCode(500, new { message = "Greška pri ažuriranju stavke" });
            }
        }

        // ══════════════════════════════════════════════════
        // DELETE OPERACIJA

        /// <summary>
        /// DELETE /api/v1/documents/{documentId}/items/{itemId}
        /// Obriši stavku (soft delete)
        /// </summary>
        [HttpDelete("{itemId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteItem(int documentId, int itemId)
        {
            try
            {
                var item = await _context.DocumentLineItems
                    .FirstOrDefaultAsync(i => i.IDStavkaDokumenta == itemId && i.IDDokument == documentId && !i.IsDeleted);

                if (item == null)
                    return NotFound();

                // Soft delete
                item.IsDeleted = true;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item {ItemId}", itemId);
                return StatusCode(500, new { message = "Greška pri brisanju stavke" });
            }
        }
    }
}
