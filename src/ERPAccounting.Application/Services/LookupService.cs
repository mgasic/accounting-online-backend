using ERPAccounting.Application.DTOs;
using ERPAccounting.Domain.Abstractions.Gateways;
using ERPAccounting.Domain.Lookups;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace ERPAccounting.Application.Services;

/// <summary>
/// High-level lookup orchestrator consumed by API controllers.
/// Keeps controller logic thin by retrieving data via the stored-procedure gateway and mapping results to DTOs.
/// </summary>
public class LookupService : ILookupService
{
    private const string DefaultDocumentTypeId = "UR";
    private readonly IStoredProcedureGateway _storedProcedureGateway;
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<LookupService> _logger;

    public LookupService(
        IStoredProcedureGateway storedProcedureGateway,
        IDbConnection dbConnection,
        ILogger<LookupService> logger)
    {
        _storedProcedureGateway = storedProcedureGateway;
        _dbConnection = dbConnection;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════
    // ORIGINAL METHODS (Stored Procedures)
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<PartnerComboDto>> GetPartnerComboAsync()
    {
        var partners = await _storedProcedureGateway.GetPartnerComboAsync();
        return partners.Select(MapToPartnerDto).ToList();
    }

    public async Task<List<OrgUnitComboDto>> GetOrgUnitsComboAsync(string? docTypeId = null)
    {
        var effectiveDocTypeId = string.IsNullOrWhiteSpace(docTypeId)
            ? DefaultDocumentTypeId
            : docTypeId!;

        var orgUnits = await _storedProcedureGateway.GetOrgUnitsComboAsync(effectiveDocTypeId);
        return orgUnits.Select(MapToOrgUnitDto).ToList();
    }

    public async Task<List<TaxationMethodComboDto>> GetTaxationMethodsComboAsync()
    {
        var taxationMethods = await _storedProcedureGateway.GetTaxationMethodsComboAsync();
        return taxationMethods.Select(MapToTaxationMethodDto).ToList();
    }

    public async Task<List<ReferentComboDto>> GetReferentsComboAsync()
    {
        var referents = await _storedProcedureGateway.GetReferentsComboAsync();
        return [.. referents.Select(MapToReferentDto)];
    }

    public async Task<List<DocumentNDComboDto>> GetDocumentNDComboAsync()
    {
        var documents = await _storedProcedureGateway.GetDocumentNDComboAsync();
        return [.. documents.Select(MapToDocumentNDDto)];
    }

    public async Task<List<TaxRateComboDto>> GetTaxRatesComboAsync()
    {
        var taxRates = await _storedProcedureGateway.GetTaxRatesComboAsync();
        return [.. taxRates.Select(MapToTaxRateDto)];
    }

    public async Task<List<ArticleComboDto>> GetArticlesComboAsync()
    {
        var articles = await _storedProcedureGateway.GetArticlesComboAsync();
        return [.. articles.Select(MapToArticleDto)];
    }

    public async Task<List<DocumentCostsListDto>> GetDocumentCostsListAsync(int documentId)
    {
        var costs = await _storedProcedureGateway.GetDocumentCostsListAsync(documentId);
        return [.. costs.Select(MapToDocumentCostDto)];
    }

    public async Task<List<CostTypeComboDto>> GetCostTypesComboAsync()
    {
        var costTypes = await _storedProcedureGateway.GetCostTypesComboAsync();
        return [.. costTypes.Select(MapToCostTypeDto)];
    }

    public async Task<List<CostDistributionMethodComboDto>> GetCostDistributionMethodsComboAsync()
    {
        var methods = await _storedProcedureGateway.GetCostDistributionMethodsComboAsync();
        return [.. methods.Select(MapToCostDistributionMethodDto)];
    }

    public async Task<List<CostArticleComboDto>> GetCostArticlesComboAsync(int documentId)
    {
        var articles = await _storedProcedureGateway.GetCostArticlesComboAsync(documentId);
        return [.. articles.Select(MapToCostArticleDto)];
    }

    // ═══════════════════════════════════════════════════════════════
    // NEW METHODS - Server-Side Search (Raw SQL)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Pretraga partnera po šifri ili nazivu (server-side filtering).
    /// Koristi Dapper za direktan SQL query.
    /// </summary>
    public async Task<List<PartnerComboDto>> SearchPartnersAsync(string searchTerm, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            _logger.LogWarning("SearchPartnersAsync called with invalid search term: '{SearchTerm}'", searchTerm);
            return new List<PartnerComboDto>();
        }

        // Normalize i cap limit
        var normalizedTerm = $"%{searchTerm.Trim()}%";
        var cappedLimit = Math.Min(Math.Max(limit, 1), 100);

        const string sql = @"
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
              AND (Sifra LIKE @SearchTerm OR Naziv LIKE @SearchTerm)
            ORDER BY Naziv";

        try
        {
            var results = await _dbConnection.QueryAsync<PartnerLookup>(
                sql,
                new { Limit = cappedLimit, SearchTerm = normalizedTerm });

            var dtos = results.Select(MapToPartnerDto).ToList();
            
            _logger.LogInformation(
                "Partner search: '{SearchTerm}' returned {Count} results",
                searchTerm,
                dtos.Count);

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search partners with term: '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Pretraga artikala po šifri ili nazivu (server-side filtering).
    /// Koristi Dapper za direktan SQL query.
    /// </summary>
    public async Task<List<ArticleComboDto>> SearchArticlesAsync(string searchTerm, int limit = 50)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
        {
            _logger.LogWarning("SearchArticlesAsync called with invalid search term: '{SearchTerm}'", searchTerm);
            return new List<ArticleComboDto>();
        }

        // Normalize i cap limit
        var normalizedTerm = $"%{searchTerm.Trim()}%";
        var cappedLimit = Math.Min(Math.Max(limit, 1), 100);

        const string sql = @"
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
              AND (Sifra LIKE @SearchTerm OR Naziv LIKE @SearchTerm)
            ORDER BY Naziv";

        try
        {
            var results = await _dbConnection.QueryAsync<ArticleLookup>(
                sql,
                new { Limit = cappedLimit, SearchTerm = normalizedTerm });

            var dtos = results.Select(MapToArticleDto).ToList();
            
            _logger.LogInformation(
                "Article search: '{SearchTerm}' returned {Count} results",
                searchTerm,
                dtos.Count);

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search articles with term: '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // MAPPING FUNCTIONS
    // ═══════════════════════════════════════════════════════════════

    private static PartnerComboDto MapToPartnerDto(PartnerLookup source) => new(
        source.IdPartner,
        source.NazivPartnera,
        source.Mesto,
        source.Opis,
        source.IdStatus,
        source.IdNacinOporezivanjaNabavka,
        source.ObracunAkciza,
        source.ObracunPorez,
        source.IdReferent,
        source.SifraPartner
    );

    private static OrgUnitComboDto MapToOrgUnitDto(OrgUnitLookup source) => new(
        source.IdOrganizacionaJedinica,
        source.Naziv,
        source.Mesto,
        source.Sifra
    );

    private static TaxationMethodComboDto MapToTaxationMethodDto(TaxationMethodLookup source) => new(
        source.IdNacinOporezivanja,
        source.Opis,
        source.ObracunAkciza,
        source.ObracunPorez,
        source.ObracunPorezPomocni
    );

    private static ReferentComboDto MapToReferentDto(ReferentLookup source) => new(
        source.IdRadnik,
        source.ImeRadnika,
        source.SifraRadnika
    );

    private static DocumentNDComboDto MapToDocumentNDDto(DocumentNDLookup source) => new(
        source.IdDokument,
        source.BrojDokumenta,
        source.Datum,
        source.NazivPartnera
    );

    private static TaxRateComboDto MapToTaxRateDto(TaxRateLookup source) => new(
        source.IdPoreskaStopa,
        source.Naziv
    );

    private static ArticleComboDto MapToArticleDto(ArticleLookup source) => new(
        source.IdArtikal,
        source.SifraArtikal,
        source.NazivArtikla,
        source.JedinicaMere,
        source.IdPoreskaStopa,
        source.ProcenatPoreza,
        source.Akciza,
        source.KoeficijentKolicine,
        source.ImaLot,
        source.OtkupnaCena,
        source.PoljoprivredniProizvod
    );

    private static DocumentCostsListDto MapToDocumentCostDto(DocumentCostLookup source)
    {
        var osnovica = source.Osnovica;
        var pdv = source.Pdv;

        return new(
            source.IdDokumentTroskovi,
            source.IdDokumentTroskoviStavka,
            source.ListaTroskova,
            osnovica,
            pdv
        );
    }

    private static CostTypeComboDto MapToCostTypeDto(CostTypeLookup source) => new(
        source.IdUlazniRacuniIzvedeni,
        source.Naziv,
        source.Opis,
        source.NazivSpecifikacije,
        source.ObracunPorez,
        source.IdUlazniRacuniOsnovni
    );

    private static CostDistributionMethodComboDto MapToCostDistributionMethodDto(CostDistributionMethodLookup source)
        => new()
        {
            IdNacinDeljenjaTroskova = source.IdNacinDeljenjaTroskova,
            Naziv = source.Naziv,
            OpisNacina = source.OpisNacina
        };

    private static CostArticleComboDto MapToCostArticleDto(CostArticleLookup source) => new(
        source.IdStavkaDokumenta,
        source.SifraArtikal,
        source.NazivArtikla
    );
}
