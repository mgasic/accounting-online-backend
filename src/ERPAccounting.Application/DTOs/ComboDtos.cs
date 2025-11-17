namespace ERPAccounting.Application.DTOs
{
    // ══════════════════════════════════════════════════
    // SP 1: spPartnerComboStatusNabavka
    public record PartnerComboDto(
        int IdPartner,
        string NazivPartnera,
        string? Mesto,
        string? Sifra,
        int Status
    );

    // ══════════════════════════════════════════════════
    // SP 2: spOrganizacionaJedinicaCombo
    public record OrgUnitComboDto(
        int IdOrganizacionaJedinica,
        string Naziv,
        string? Mesto,
        string? Sifra
    );

    // ══════════════════════════════════════════════════
    // SP 3: spNacinOporezivanjaComboNabavka
    public record TaxationMethodComboDto(
        int IdNacinOporezivanja,
        string Opis,
        short ObracunAkciza,
        short ObracunPorez
    );

    // ══════════════════════════════════════════════════
    // SP 4: spReferentCombo
    public record ReferentComboDto(
        int IdRadnik,
        string ImeRadnika,
        string? SifraRadnika
    );

    // ══════════════════════════════════════════════════
    // SP 5: spDokumentNDCombo
    public record DocumentNDComboDto(
        int IdDokument,
        string BrojDokumenta,
        DateTime Datum,
        string NazivPartnera
    );

    // ══════════════════════════════════════════════════
    // SP 6: spPoreskaStopaCombo
    public record TaxRateComboDto(
        string IdPoreskaStopa,
        string Naziv,
        decimal ProcenatPDV
    );

    // ══════════════════════════════════════════════════
    // SP 7: spArtikalComboUlaz
    public record ArticleComboDto(
        int IdArtikal,
        string SifraArtikal,
        string NazivArtikla,
        string? JedinicaMere,
        decimal? NabavnaCena
    );

    // ══════════════════════════════════════════════════
    // SP 8: spDokumentTroskoviLista
    public record DocumentCostsListDto(
        int IdDokumentTroskovi,
        int IdDokument,
        int? IdPartner,
        string? IdVrstaDokumenta,
        string? BrojDokumenta,
        DateTime? DatumDPO,
        string? Opis
    );

    // ══════════════════════════════════════════════════
    // SP 9: spUlazniRacuniIzvedeniTroskoviCombo
    public record CostTypeComboDto(
        int IdUlazniRacuniIzvedeni,
        string Naziv,
        string? Opis
    );

    // ══════════════════════════════════════════════════
    // SP 10: spNacinDeljenjaTroskovaCombo (3 fiksne vrednosti: 1, 2, 3)
    public record CostDistributionMethodComboDto
    {
        public int Id { get; set; } // 1, 2, 3
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════
    // SP 11: spDokumentTroskoviArtikliCOMBO
    public record CostArticleComboDto(
        int IdStavkaDokumenta,
        string SifraArtikal,
        string NazivArtikla,
        decimal Kolicina
    );
}
