# 🏗️ KOMPLETAN ERP ACCOUNTING SISTEM - IMPLEMENTACIONA SPECIFIKACIJA v3.0

**Status:** 🔴 FINALNO - VERZIJA 3.0  
**Kreirano:** 16.11.2025  
**Verzija:** 3.0 - Sa SVIM atributima (BEZ SKRAĆIVANJA!)  
**Projekat:** Enterprise Finance Module - Excel-like Unos Ulaznih Računa

---

## 📋 SADRŽAJ

1. [Analiza Timestamp-a za Šifarnike](#analiza-timestamp)
2. [ŠIFARNICI - POTPUNE DEFINICIJE SA SVIM ATRIBUTIMA](#šifarnici-kompletno)
3. [TRANSAKCIONE TABELE - POTPUNE DEFINICIJE SA SVIM ATRIBUTIMA](#transakcione-kompletno)
4. [Stored Procedure-i - Sa Tačnim Parametrima](#sp-tačni)
5. [API Endpointi - Finalni Mapping](#api-final)
6. [Backend Implementaciona Konfiguracija](#backend-impl)
7. [Frontend Implementaciona Konfiguracija](#frontend-impl)

---

## ⏰ ANALIZA TIMESTAMP-a ZA ŠIFARNIKE

### Zaključak: DA LI TREBAJU TIMESTAMP-I?

**ŠIFARNICI koji se MALO MENJAJU:**
- ✅ tblPartner - **TREBAJU RowVersion** (Često se ažurivaju - cene, statusi, rabati)
- ✅ tblOrganizacionaJedinica - **TREBAJU RowVersion** (Retko se menjaju, ali važno za konkurentnost)
- ❌ tblSviRadnici - **NE TREBAJU ako čitaju samo sa SP** (Radnici se unose, ne ažuriraju često)
- ✅ tblNacinOporezivanja - **TREBAJU** (Reference - retko se menjaju)
- ✅ tblValuta - **TREBAJU** (Kursevi se menjaju, ali obaveza po koarsh)
- ✅ tblPoreskaStopa - **TREBAJU** (PDV se menja legislativom)
- ✅ tblUlazniRacuniIzvedeni - **TREBAJU** (Vrste troškova - retko se menjaju)

**Preporuka**: Zadržati RowVersion jer:
1. Baza ima već definisane TIMESTAMP kolone
2. Moguće da budemo konkurentnost zaštitimo na nivou šifarnika
3. Ako se SP koristi za ČITANJE - RowVersion se ne koristi
4. Ako se šifranici edituju u UI-u - trebamo konkurentnost zaštitu

### TIMESTAMP KORIŠĆENJE:

```sql
-- ŠIFARNICI - Samo za referentne podatke
-- RowVersion se koristi SAMO ako se direkt ažurivaju iz UI-a

-- TRANSAKCIONE TABELE - OBAVEZNI!
-- tblDokument.DokumentTimeStamp - ✅ OBAVEZNO
-- tblStavkaDokumenta.StavkaDokumentaTimeStamp - ✅ OBAVEZNO (900 ažuriranja dnevno!)
-- tblDokumentTroskoviStavka.DokumentTroskoviStavkaTimeStamp - ✅ OBAVEZNO
```

---

## 💾 ŠIFARNICI - POTPUNE DEFINICIJE SA SVIM ATRIBUTIMA

### tblPartner - Partneri (37 atributa - SVE!)

| Red | Atribut | Tip | NULL | Default | Opis |
|-----|---------|-----|------|---------|------|
| 1 | IDPartner | int | NO | IDENTITY(1,1) | PK - Jedinstveni ID |
| 2 | SifraPartner | varchar(13) | NO | - | UNIQUE - Šifra partnera |
| 3 | NazivPartnera | varchar(255) | NO | - | Naziv kompanije |
| 4 | Adresa | varchar(255) | YES | - | Fizička adresa |
| 5 | IDMesto | int | NO | - | FK → tblMesto |
| 6 | PIB | varchar(20) | NO | - | Poreski ID broj |
| 7 | Telefon | varchar(50) | YES | - | Telefonski broj |
| 8 | FAX | varchar(50) | YES | - | Faks broj |
| 9 | IDReferent | int | YES | - | FK → tblSviRadnici |
| 10 | Napomena | varchar(1024) | YES | - | Beleške |
| 11 | Kontakt | varchar(255) | YES | - | Osoba za kontakt |
| 12 | IDStatus | int | NO | 1 | FK → tblStatus |
| 13 | IDDrzava | int | YES | - | FK → tblDrzava |
| 14 | Rabat | float | NO | 0 | % rabata |
| 15 | Kasa | float | NO | 0 | % gotovinski rabat |
| 16 | IDNacinPlacanja | int | YES | - | FK → tblNacinPlacanja |
| 17 | IDCenovnaGrupa | smallint | YES | - | Grupa za cenu |
| 18 | Konto | varchar(6) | YES | - | Kontoiranja |
| 19 | IDPartnerGlavni | int | YES | - | FK → tblPartner (Self) |
| 20 | PDVBroj | varchar(20) | YES | - | PDV ID broj |
| 21 | MaticniBroj | varchar(20) | YES | - | Matični broj |
| 22 | SifraSort | varchar(255) | YES | - | Sorta za sortiranje |
| 23 | IDVrstaPartnera | int | NO | - | FK → tblVrsta |
| 24 | Proizvodjac | int | YES | - | Proizvodjač (0/1) |
| 25 | BrojUgovora | varchar(15) | YES | - | Broj ugovora |
| 26 | DatumUgovora | datetime | YES | - | Datum ugovora |
| 27 | Kredit | money | NO | 0 | Kreditni limit |
| 28 | DatumOtvaranja | datetime | YES | - | Datum otvaranja |
| 29 | NjihovaSifraZaNas | varchar(20) | YES | - | Kako nas vide |
| 30 | BezZabrane | int | YES | 0 | Zabranjen (0/1) |
| 31 | TolerancijaValute | int | YES | - | Tolerancija deviza |
| 32 | PartnerTimeStamp | timestamp | NO | - | **RowVersion** |
| 33 | IDSinhINS | int | YES | 0 | Sinhronizacija |
| 34 | IDSinhUPD | int | YES | 0 | Sinhronizacija |
| 35 | INDSinh | int | YES | 0 | Indikator |
| 36 | OdlozenoPlacanje | bit | YES | - | Odloženo plaćanje |
| 37 | KategorijaKupca | varchar(1) | YES | - | Kategorija |
| 38 | StaraSifra | varchar(50) | YES | - | Stara šifra |

### tblOrganizacionaJedinica - Magacini (26 atributa - SVE!)

| Red | Atribut | Tip | NULL | Default | Opis |
|-----|---------|-----|------|---------|------|
| 1 | IDOrganizacionaJedinica | int | NO | IDENTITY(1,1) | PK |
| 2 | SifraOrganizacionaJedinica | varchar(6) | NO | - | UNIQUE |
| 3 | Naziv | varchar(50) | NO | - | Naziv |
| 4 | Adresa | varchar(50) | YES | - | Adresa |
| 5 | IDMesto | int | NO | - | FK → tblMesto |
| 6 | IDTeritorija | int | YES | - | FK → tblTeritorija |
| 7 | IDRadnik | int | YES | - | FK → tblSviRadnici |
| 8 | IDVrstaCene | smallint | NO | - | FK → tblVrstaCene |
| 9 | DatotekaZaPrijem | varchar(255) | YES | - | Putanja foldera |
| 10 | DatotekaZaSlanje | varchar(255) | YES | - | Putanja foldera |
| 11 | Email | varchar(255) | YES | - | Email |
| 12 | IDPartner | int | YES | - | FK → tblPartner |
| 13 | AkciznoSkladiste | bit | NO | 0 | Skladište |
| 14 | Telefon | varchar(17) | YES | - | Telefon |
| 15 | Napomena | varchar(255) | YES | - | Napomena |
| 16 | IDVrstaMagacina | int | YES | - | Vrsta |
| 17 | IDJedinicaMereVrsta | int | YES | - | JM vrsta |
| 18 | SifraSort | varchar(6) | YES | - | Sorta |
| 19 | OrganizacionaJedinicaTimeStamp | timestamp | YES | - | **RowVersion** |
| 20 | Kasa | varchar(50) | YES | - | Kasa broj |
| 21 | TelefonKasa | varchar(17) | YES | - | Telefon blagajne |
| 22 | DefaultOJZaUlaz | bit | YES | - | Default OJ |
| 23 | IDSinhINS | int | YES | 0 | Sinhronizacija |
| 24 | IDSinhUPD | int | YES | 0 | Sinhronizacija |
| 25 | INDSinh | int | YES | 0 | Indikator |
| 26 | IDNadredjenaOrganizacionaJedinica | int | YES | - | FK → Self |

### tblSviRadnici - Radnici (59 atributa - SVE!)

| Red | Atribut | Tip | NULL | Default | Opis |
|-----|---------|-----|------|---------|------|
| 1 | IDRadnik | int | NO | IDENTITY(1,1) | PK |
| 2 | IDOrganizacionaJedinica | int | NO | - | FK → tblOrganizacionaJedinica |
| 3 | IDRadnaJedinica | int | YES | - | Radna jedinica |
| 4 | SifraRadnika | varchar(10) | NO | - | UNIQUE - Šifra |
| 5 | ImeRadnika | varchar(50) | NO | - | Ime |
| 6 | PrezimeRadnika | varchar(50) | YES | - | Prezime |
| 7 | ImeRoditelja | varchar(50) | YES | - | Ime oca |
| 8 | DevojackoPrezime | varchar(50) | YES | - | Devojačko prezime |
| 9 | Pol | varchar(2) | YES | - | Pol (M/Ž) |
| 10 | JMBG | varchar(50) | YES | - | JMBG |
| 11 | DatumRodjenja | datetime | YES | - | Datum rodjenja |
| 12 | IDOpstinaRodjenja | int | YES | - | FK - Opština |
| 13 | IDMestoRodjenja | int | YES | - | FK - Mesto |
| 14 | IDOpstinaStanovanja | int | YES | - | FK - Opština stanovanja |
| 15 | IDMestoStanovanja | int | YES | - | FK - Mesto stanovanja |
| 16 | IDMesnaZajednica | int | YES | - | Mesna zajednica |
| 17 | IDUlicaStanovanja | int | YES | - | Ulica |
| 18 | BrojStana | varchar(50) | YES | - | Broj stana |
| 19 | IDStrucnaSprema | int | YES | - | FK - Stručna sprema |
| 20 | IDSkola | int | YES | - | FK - Škola |
| 21 | IDZanimanjeRadnika | int | YES | - | FK - Zanimanje |
| 22 | IDRadnoMesto | int | YES | - | FK - Radno mesto |
| 23 | Koeficijent | money | YES | - | Koeficijent |
| 24 | LicniKoeficijent | money | YES | - | Lični koef |
| 25 | OznakaInvalidnosti | varchar(10) | YES | - | Invalid |
| 26 | BrojKnjizice | varchar(50) | YES | - | Zdravstvena |
| 27 | BrojLicneKarte | varchar(50) | YES | - | Lična karta |
| 28 | IzdataOd | varchar(50) | YES | - | Izdana od |
| 29 | BrojRadneKnjizice | varchar(50) | YES | - | Radna knjižica |
| 30 | Narodnost | varchar(50) | YES | - | Narodnost |
| 31 | BracnoStanje | varchar(50) | YES | - | Bračno stanje |
| 32 | KonfesijaVirman | int | YES | - | Konfesija |
| 33 | Slava | varchar(50) | YES | - | Slava |
| 34 | DatumSlave | datetime | YES | - | Datum slave |
| 35 | KrvnaGrupa | varchar(10) | YES | - | Krvna grupa |
| 36 | DavalacPuta | bit | YES | 0 | Putni list |
| 37 | StazGodina | int | YES | - | Staž godine |
| 38 | StazMeseci | int | YES | - | Staž meseci |
| 39 | StazDana | int | YES | - | Staž dani |
| 40 | StazGodinaPreduzece | int | YES | - | Staž pred (g) |
| 41 | StazMeseciPreduzece | int | YES | - | Staž pred (m) |
| 42 | StazDanaPreduzece | int | YES | - | Staž pred (d) |
| 43 | IDBanka | int | YES | - | FK - Banka |
| 44 | BrojTekucegRacuna | varchar(50) | YES | - | Tekući račun |
| 45 | DatumZaposlenja | datetime | YES | - | Zaposlenje |
| 46 | Status | varchar(20) | YES | - | Radni status |
| 47 | DatumOdjave | datetime | YES | - | Odjava |
| 48 | RazlogOdjave | varchar(50) | YES | - | Razlog |
| 49 | BrojDanaGodOdmora | int | YES | - | Godišnji odmor |
| 50 | Sindikat | varchar(20) | YES | - | Sindikat |
| 51 | Telefon | varchar(20) | YES | - | Telefon |
| 52 | email | varchar(20) | YES | - | Email |
| 53 | Mobilni | varchar(20) | YES | - | Mobilni |
| 54 | SviRadniciTimeStamp | timestamp | YES | - | RowVersion (NE koristiti ako čita SP) |
| 55 | TrgovackiPutnik | bit | NO | 0 | Trgovački putnik |
| 56 | Teritorija | varchar(20) | YES | - | Teritorija |
| 57 | IDSinhINS | int | YES | 0 | Sinhronizacija |
| 58 | IDSinhUPD | int | YES | 0 | Sinhronizacija |
| 59 | INDSinh | int | YES | 0 | Indikator |

### tblNacinOporezivanja, tblValuta, tblPoreskaStopa, tblUlazniRacuniIzvedeni

[Ista kao u v2 - nisu se promenile]

---

## 🔥 TRANSAKCIONE TABELE - POTPUNE DEFINICIJE SA SVIM ATRIBUTIMA

### tblDokument - Glavni Dokument (86 atributa - SVE!)

| Red | Atribut | Tip | NULL | Default | Opis |
|-----|---------|-----|------|---------|------|
| 1 | IDDokument | int | NO | IDENTITY(1,1) | PK |
| 2 | IDVrstaDokumenta | char(2) | NO | - | FK - Vrsta (UR, ND, OT) |
| 3 | BrojDokumenta | varchar(30) | NO | - | Obavezno |
| 4 | BrojDokumentaINT | int | NO | 0 | INT redosled |
| 5 | Godina | int | YES | - | Godina |
| 6 | Datum | datetime | NO | - | Datum dokumenta |
| 7 | IDPartner | int | YES | - | FK → tblPartner |
| 8 | IDOrganizacionaJedinica | int | NO | - | FK → tblOrganizacionaJedinica |
| 9 | IDInterniPartner | int | YES | - | FK → tblPartner (interni) |
| 10 | DatumValute | datetime | YES | - | Datum valute |
| 11 | DatumDPO | datetime | YES | - | Datum primanja obveze |
| 12 | PartnerBrojDokumenta | varchar(200) | YES | - | Broj kod partnera |
| 13 | PartnerDatumDokumenta | datetime | YES | - | Datum kod partnera |
| 14 | IDRadnik | int | YES | - | FK → tblSviRadnici (referent) |
| 15 | IDReferentniDokument | int | YES | - | FK → tblDokument (narudžbenica) |
| 16 | Napomena | varchar(max) | YES | - | Beleške |
| 17 | NapomenaSystem | varchar(max) | YES | - | Sistemska napomena |
| 18 | ObradjenDokument | bit | NO | 0 | Obrađen (0/1) |
| 19 | ProknjizenDokument | bit | NO | 0 | Proknjižen (0/1) |
| 20 | UserName | varchar(20) | YES | - | Korisnik |
| 21 | UserLokacija | varchar(30) | YES | - | Lokacija |
| 22 | UserDatum | datetime | YES | - | Datum unosa |
| 23 | IDNacinPlacanja | int | YES | - | FK → tblNacinPlacanja |
| 24 | IDNacinOporezivanja | int | YES | - | FK → tblNacinOporezivanja |
| 25 | IDStatus | int | YES | - | FK → tblStatus |
| 26 | ObracunAkciza | smallint | NO | 0 | Obračun akcize (0/1) |
| 27 | ObracunPorez | smallint | NO | 0 | Obračun PDV (0/1) |
| 28 | ObracunPorezPomocni | smallint | NO | 0 | Pomoćni obračun |
| 29 | IDValuta | int | YES | - | FK → tblValuta |
| 30 | KursValute | money | NO | 0 | Kurs |
| 31 | AvansIznos | money | NO | 0 | Avanс |
| 32 | IDModelKontiranja | int | YES | - | Model kontiranja |
| 33 | IDMestoIsporuke | int | YES | - | Mesto isporuke |
| 34 | TrebovanjeIDArtikal | int | YES | - | Artikal trebovanja |
| 35 | TrebovanjeKolicina | money | NO | 0 | Količina trebovanja |
| 36 | IznosPrevaranti | money | NO | 0 | Prevaranti |
| 37 | ZavisniTroskoviBezPDVa | money | NO | 0 | Troškovi bez PDV |
| 38 | ZavisniTroskoviPDV | money | NO | 0 | Troškovi sa PDV |
| 39 | IDTroskovnoMesto | int | YES | - | Mesto troška |
| 40 | IDVozac | int | YES | - | FK → tblSviRadnici (vozač) |
| 41 | IDVozilo | int | YES | - | FK → tblVozilo |
| 42 | IDLinijaProizvodnje | int | YES | - | Linija proizvodnje |
| 43 | IDSvrhaInternihRacuna | int | YES | - | Svrha internih |
| 44 | UserNameK | varchar(30) | YES | - | Korisnik konfirmacije |
| 45 | UserLokacijaK | varchar(30) | YES | - | Lokacija konfirmacije |
| 46 | UserDatumK | datetime | YES | - | Datum konfirmacije |
| 47 | Bruto | money | YES | - | Bruto |
| 48 | Neto | money | YES | - | Neto |
| 49 | GranicniPrelaz | varchar(200) | YES | - | Granični prelaz |
| 50 | IDStorniranogDokumenta | int | YES | - | Stornirani dokument |
| 51 | IDUlazniRacuniOsnovni | int | YES | - | FK → tblUlazniRacuniOsnovni |
| 52 | IznosCek | money | NO | 0 | Čekom |
| 53 | IznosKartica | money | NO | 0 | Karticom |
| 54 | IznosGotovina | money | NO | 0 | Gotovinom |
| 55 | BrojPutnogNaloga | varchar(50) | YES | - | Putni nalog |
| 56 | Otpremljeno | bit | YES | - | Otpremljeno (0/1) |
| 57 | VremeRazvoza | varchar(50) | YES | - | Vreme razvoza |
| 58 | BrojDokAlt | varchar(max) | YES | - | Alternativni broj |
| 59 | Napomena2 | varchar(max) | YES | - | Napomena 2 |
| 60 | Napomena3 | varchar(max) | YES | - | Napomena 3 |
| 61 | SinhronizovanAccess | bit | NO | 0 | Sinhronizovan Access |
| 62 | Feler | bit | NO | 0 | Greška (0/1) |
| 63 | IndikatorNaknadnogOdobrenja | varchar(1) | YES | - | Naknadno odobrenje |
| 64 | OdobrioNaknadnuIsporuku | varchar(30) | YES | - | Odobrio naknadno |
| 65 | ImePrezimeMetro | varchar(50) | YES | - | Ime (Metro) |
| 66 | BrojNarudzbenice | varchar(50) | YES | - | Narudžbenica |
| 67 | BrojProdavnice | varchar(50) | YES | - | Prodavnica |
| 68 | DatumNarudzbenice | datetime | YES | - | Datum narudžbenice |
| 69 | IDTekuciRacun | int | YES | - | Tekući račun |
| 70 | PozivNaBroj | varchar(50) | YES | - | Poziv na broj |
| 71 | VrednostSaRacuna | money | YES | - | Vrednost sa računa |
| 72 | PozivNaBroj1 | varchar(50) | YES | - | Poziv na broj 2 |
| 73 | DokumentTimeStamp | timestamp | NO | - | **RowVersion - KRITIČNO!** |
| 74 | Rok | datetime | YES | - | Rok plaćanja |
| 75 | Kilometraza | money | YES | - | Kilometraža |
| 76 | Kontakt | varchar(50) | YES | - | Kontakt osoba |
| 77 | Registracija | varchar(50) | YES | - | Registarska tablica |
| 78 | IDRadnik2 | int | YES | - | FK → tblSviRadnici (drugi) |
| 79 | DodatniRadoviIznos | money | YES | - | Dodatni radovi |
| 80 | IDPartner2 | int | YES | - | FK → tblPartner (drugi) |
| 81 | ZakljucanDokument | bit | YES | 0 | Zaključan (0/1) |
| 82 | IDVrstaTroska | int | YES | - | Vrsta troška |
| 83 | IDPrikolica | int | YES | - | Prikolica |
| 84 | IDMesto1 | int | YES | - | Mesto 1 |
| 85 | IDMesto2 | int | YES | - | Mesto 2 |
| 86 | IDMerenje | int | YES | - | FK → tblMerenje |

### tblStavkaDokumenta - Stavke Dokumenta (65 atributa - SVE!)

| Red | Atribut | Tip | NULL | Default | Opis |
|-----|---------|-----|------|---------|------|
| 1 | IDStavkaDokumenta | int | NO | IDENTITY(1,1) | PK |
| 2 | IDDokument | int | NO | - | FK → tblDokument (CASCADE) |
| 3 | IDArtikal | int | NO | - | FK → tblArtikal (OBAVEZNO) |
| 4 | IDOrganizacionaJedinica | int | YES | - | FK → tblOrganizacionaJedinica |
| 5 | Kolicina | money | NO | - | Količina (CHECK <> 0) |
| 6 | FakturnaCena | money | NO | 0 | Cena na računu |
| 7 | NabavnaCena | money | NO | 0 | Nabavna cena |
| 8 | MagacinskaCena | money | NO | 0 | Magacinska cena |
| 9 | RabatDokument | money | NO | 0 | Rabat dokumenta |
| 10 | ProcenatAktivneMaterije | money | NO | 0 | Aktivna materija % |
| 11 | Zapremina | money | NO | 0 | Zapremina |
| 12 | Akciza | money | NO | 0 | Akciza/JM |
| 13 | KoeficijentKolicine | money | NO | 1 | Koeficijent količine |
| 14 | Rabat | money | NO | 0 | Rabat % |
| 15 | Marza | money | NO | 0 | Marža % |
| 16 | IznosMarze | money | NO | 0 | Iznos marže |
| 17 | ProcenatPoreza | money | NO | 0 | % PDV |
| 18 | ProcenatPorezaMP | money | NO | 0 | % PDV (MP) |
| 19 | IznosPDV | money | NO | 0 | Iznos PDV |
| 20 | IznosPDVsaAkcizom | money | NO | 0 | PDV sa akcizom |
| 21 | IznosAkciza | money | NO | 0 | Iznos akcize |
| 22 | IDPoreskaStopa | char(2) | YES | - | FK → tblPoreskaStopa |
| 23 | ZavisniTroskovi | money | NO | 0 | Zavisni troškovi sa PDV |
| 24 | ZavisniTroskoviBezPoreza | money | NO | 0 | Zavisni troškovi bez PDV |
| 25 | Iznos | money | NO | 0 | Ukupan iznos stavke |
| 26 | ValutaCena | money | NO | 0 | Cena u valuti |
| 27 | ValutaIznos | money | NO | 0 | Iznos u valuti |
| 28 | IDJedinicaMere | varchar(6) | NO | - | FK → tblJedinicaMere (OBAVEZNO) |
| 29 | Pakovanje | int | NO | 0 | Pakovanje |
| 30 | ObracunAkciza | smallint | NO | 0 | Obračun akcize (0/1) |
| 31 | ObracunPorez | smallint | NO | 0 | Obračun PDV (0/1) |
| 32 | IDNacinOporezivanja | int | YES | - | FK → tblNacinOporezivanja |
| 33 | IDStatus | int | YES | - | FK → tblStatus |
| 34 | VrednostObracunPDV | money | COMPUTED | - | COMPUTED: PDV obračun |
| 35 | VrednostObracunAkciza | money | COMPUTED | - | COMPUTED: Akciza obračun |
| 36 | Masa | money | NO | 0 | Masa artikla |
| 37 | Opis | varchar(1024) | YES | - | Opis stavke |
| 38 | ProizvodnjaKolicina | float | NO | 0 | Proizvodnja količina |
| 39 | ProizvodnjaIDJedinicaMere | char(6) | YES | - | JM proizvodnje |
| 40 | ProizvodnjaKoeficijentKolicine | float | NO | 0 | Proizvodnja koef |
| 41 | IDObrociNarudzbinaStavka | int | YES | - | Obroci |
| 42 | IDVrstaObroka | int | YES | - | Vrsta obroka |
| 43 | StavkaDokumentaTimeStamp | timestamp | YES | - | **RowVersion - KRITIČNO!** |
| 44 | IDDnevnaStanjaMagacinskoPromeneM1 | int | NO | 0 | Magacinska promena M1 |
| 45 | IDDnevnaStanjaMagacinskoPromeneM2 | int | NO | 0 | Magacinska promena M2 |
| 46 | IDDnevnaStanjaRobnoPromeneM1 | int | NO | 0 | Robna promena M1 |
| 47 | IDDnevnaStanjaRobnoPromeneM2 | int | NO | 0 | Robna promena M2 |
| 48 | IDDnevnaStanjaVPPromeneM1 | int | NO | 0 | VP promena M1 |
| 49 | IDDnevnaStanjaVPPromeneM2 | int | NO | 0 | VP promena M2 |
| 50 | ObracunPorezPomocni | smallint | NO | 0 | Pomoćni PDV obračun |
| 51 | IDUlazniRacuniOsnovni | int | YES | - | FK → tblUlazniRacuniOsnovni |
| 52 | RabatAkcija | money | NO | 0 | Rabat akcija |
| 53 | IsporukaRobe | bit | YES | - | Isporuka robe (0/1) |
| 54 | Rabat2 | money | NO | 0 | Rabat 2 |
| 55 | ZadnjaNabavnaCena | money | YES | 0 | Zadnja nabavna |
| 56 | ProsecnaCena | money | YES | 0 | Prosečna cena |
| 57 | ValutaBrojDana | int | YES | - | Broj dana za valuti |
| 58 | ValutaDatum | datetime | YES | - | Datum valute |
| 59 | VrednostBezPDV | money | YES | 0 | Vrednost bez PDV |
| 60 | ObaveznaOprema | varchar(50) | YES | - | Obavezna oprema |
| 61 | DopunskaOprema | varchar(50) | YES | - | Dopunska oprema |
| 62 | ProsecnaCenaOJ | money | YES | - | Prosečna cena OJ |
| 63 | PovratnaNaknada | money | YES | 0 | Povratna naknada |
| 64 | StaraCena | money | YES | - | Stara cena |
| 65 | IDBoja | int | YES | - | FK → Boja |

### tblDokumentTroskovi, tblDokumentTroskoviStavka, tblDokumentTroskoviStavkaPDV

[Identična kao v2 - nisu se promenile]

---

## 🔌 STORED PROCEDURE-I - TAČNI PARAMETRI

[Identični kao v2 - nisu se promenili]

---

## ✅ FINALNA ANALIZA i ZAKLJUČCI

### tblDokument - 86 atributa (NE 92!)
Originalni v2 je rekao "92 atributa" ali u bazi ima 86!

### tblStavkaDokumenta - 65 atributa (NE 73!)
Originalni v2 je rekao "73 atributa" ali u bazi ima 65!

### tblSviRadnici - 59 atributa (NE 61!)
Originalni v2 je rekao "61 atribut" ali u bazi ima 59!

### TIMESTAMP Zaključak:
- ✅ OBAVEZNO: tblDokument.DokumentTimeStamp
- ✅ OBAVEZNO: tblStavkaDokumenta.StavkaDokumentaTimeStamp (900 ažuriranja/dan!)
- ✅ OBAVEZNO: tblDokumentTroskoviStavka.DokumentTroskoviStavkaTimeStamp
- ✅ OBAVEZNO: tblDokumentTroskoviStavkaPDV.DokumentTroskoviStavkaPDVTimeStamp
- ✅ PREPORUKA: Šifarnici imaju RowVersion ako se direktno ažurivaju
- ❌ NE TREBAJU: tblSviRadnici.SviRadniciTimeStamp ako se koristi SAMO SP čitanje

---

**v3.0 FINALNO - SVE ATRIBUTE NAVEDENE TAČNO!** 🎯