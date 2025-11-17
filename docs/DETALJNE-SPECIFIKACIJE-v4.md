# 🏗️ KOMPLETAN ERP ACCOUNTING SISTEM - DETALJNE SPECIFIKACIJE v4.0

**Status:** 🟢 FINALNO - SA SVIM DETALJIMA  
**Kreirano:** 16.11.2025  
**Verzija:** 4.0 - POTPUNA sa svim atributima tabela i SP  
**Projekat:** Enterprise Finance Module - Excel-like Unos Ulaznih Računa

---

## 📋 SADRŽAJ

1. [ŠIFARNICI - Sve Tabele sa Atributima](#šifarnici)
2. [TRANSAKCIONE TABELE - Sve Tabele sa Atributima](#transakcione-tabele)
3. [STORED PROCEDURE-i - Svi Ulazni/Izlazni Parametri](#stored-procedure-i)
4. [API Endpointi - Sa Mapiranjem na SP](#api-endpointi)
5. [Database View-i](#database-view-i)

---

## 💾 ŠIFARNICI - POTPUNE DEFINICIJE

### tblPartner - Partneri (Dobavljači, Kupci)

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDPartner** | int | NOT NULL | IDENTITY(1,1) | PK - Jedinstveni ID |
| **SifraPartner** | varchar(13) | NOT NULL | - | UNIQUE - Šifra partnera |
| **NazivPartnera** | varchar(255) | NOT NULL | - | Naziv kompanije |
| **Adresa** | varchar(255) | NULL | - | Fizička adresa |
| **IDMesto** | int | NOT NULL | - | FK → tblMesto |
| **PIB** | varchar(20) | NOT NULL | - | Poreski identifikacioni broj |
| **Telefon** | varchar(50) | NULL | - | Telefonski broj |
| **FAX** | varchar(50) | NULL | - | Faks broj |
| **IDReferent** | int | NULL | - | FK → tblSviRadnici |
| **Napomena** | varchar(1024) | NULL | - | Beleške |
| **Kontakt** | varchar(255) | NULL | - | Ime osobe za kontakt |
| **IDStatus** | int | NOT NULL | 1 | FK → tblStatus (DEFAULT 1 = Aktivan) |
| **IDDrzava** | int | NULL | - | FK → tblDrzava |
| **Rabat** | float | NOT NULL | 0 | Procenat rabata |
| **Kasa** | float | NOT NULL | 0 | Gotovinski rabat |
| **IDNacinPlacanja** | int | NULL | - | FK → tblNacinPlacanja |
| **IDCenovnaGrupa** | smallint | NULL | - | Grupa za određivanje cene |
| **Konto** | varchar(6) | NULL | - | Račun za kontiranje |
| **IDPartnerGlavni** | int | NULL | - | FK → tblPartner (Self-reference za matične partnere) |
| **PDVBroj** | varchar(20) | NULL | - | PDV identifikacioni broj |
| **MaticniBroj** | varchar(20) | NULL | - | Matični broj kompanije |
| **SifraSort** | varchar(255) | NULL | - | Sorta za sortiranje |
| **IDVrstaPartnera** | int | NOT NULL | - | FK → tblVrsta (Dobavljač/Kupac) |
| **Proizvodjac** | int | NULL | - | Da li je proizvodjač (0/1) |
| **BrojUgovora** | varchar(15) | NULL | - | Broj ugovora |
| **DatumUgovora** | datetime | NULL | - | Datum potpisivanja ugovora |
| **Kredit** | money | NOT NULL | 0 | Kreditni limit |
| **DatumOtvaranja** | datetime | NULL | - | Datum otvaranja konto |
| **NjihovaSifraZaNas** | varchar(20) | NULL | - | Kako nas vide kod njih |
| **BezZabrane** | int | NULL | 0 | Da li je zabranjen (0=može, 1=zabranjen) |
| **TolerancijaValute** | int | NULL | - | Tolerancija za deviznu razliku |
| **PartnerTimeStamp** | timestamp | NOT NULL | - | Konkurentnost - RowVersion |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija INSERT |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija UPDATE |
| **INDSinh** | int | NULL | 0 | Indikator sinhronizacije |
| **OdlozenoPlacanje** | bit | NULL | - | Odloženo plaćanje dozvoljeno |
| **KategorijaKupca** | varchar(1) | NULL | - | Kategorija kupca (A/B/C) |
| **StaraSifra** | varchar(50) | NULL | - | Stara šifra za kompatibilnost |

---

### tblOrganizacionaJedinica - Magacini i OJ

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDOrganizacionaJedinica** | int | NOT NULL | IDENTITY(1,1) | PK |
| **SifraOrganizacionaJedinica** | varchar(6) | NOT NULL | - | UNIQUE - Šifra magacina |
| **Naziv** | varchar(50) | NOT NULL | - | Naziv magacina/OJ |
| **Adresa** | varchar(50) | NULL | - | Adresa |
| **IDMesto** | int | NOT NULL | - | FK → tblMesto |
| **IDTeritorija** | int | NULL | - | FK → tblTeritorija |
| **IDRadnik** | int | NULL | - | FK → tblSviRadnici (Menadžer) |
| **IDVrstaCene** | smallint | NOT NULL | - | FK → tblVrstaCene (Tip cene) |
| **DatotekaZaPrijem** | varchar(255) | NULL | - | Putanja do foldera za prijem |
| **DatotekaZaSlanje** | varchar(255) | NULL | - | Putanja do foldera za slanje |
| **Email** | varchar(255) | NULL | - | Email adresa magacina |
| **IDPartner** | int | NULL | - | FK → tblPartner (Partner koji koristi magacin) |
| **AkciznoSkladiste** | bit | NOT NULL | 0 | Da li je akciznomaterno skladište |
| **Telefon** | varchar(17) | NULL | - | Telefon |
| **Napomena** | varchar(255) | NULL | - | Napomena |
| **IDVrstaMagacina** | int | NULL | - | FK → Vrsta magacina |
| **IDJedinicaMereVrsta** | int | NULL | - | FK → Vrsta JM za magacin |
| **SifraSort** | varchar(6) | NULL | - | Sorta |
| **OrganizacionaJedinicaTimeStamp** | timestamp | NULL | - | RowVersion |
| **Kasa** | varchar(50) | NULL | - | Kasa broj |
| **TelefonKasa** | varchar(17) | NULL | - | Telefon blagajne |
| **DefaultOJZaUlaz** | bit | NULL | - | Default OJ za ulazne račune |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija |
| **INDSinh** | int | NULL | 0 | Indikator |
| **IDNadredjenaOrganizacionaJedinica** | int | NULL | - | FK → Self (Nadrejena OJ) |

---

### tblSviRadnici - Radnici i Referenti

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDRadnik** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDOrganizacionaJedinica** | int | NOT NULL | - | FK → tblOrganizacionaJedinica |
| **IDRadnaJedinica** | int | NULL | - | FK → Radna jedinica |
| **SifraRadnika** | varchar(10) | NOT NULL | - | UNIQUE - Šifra |
| **ImeRadnika** | varchar(50) | NOT NULL | - | Ime |
| **PrezimeRadnika** | varchar(50) | NULL | - | Prezime |
| **ImeRoditelja** | varchar(50) | NULL | - | Ime oca |
| **DevojackoPrezime** | varchar(50) | NULL | - | Devojačko prezime |
| **Pol** | varchar(2) | NULL | - | M/Ž |
| **JMBG** | varchar(50) | NULL | - | Jedinstveni matični broj |
| **DatumRodjenja** | datetime | NULL | - | Datum rođenja |
| **IDOpstinaRodjenja** | int | NULL | - | FK |
| **IDMestoRodjenja** | int | NULL | - | FK |
| **IDOpstinaStanovanja** | int | NULL | - | FK |
| **IDMestoStanovanja** | int | NULL | - | FK |
| **IDMesnaZajednica** | int | NULL | - | FK |
| **IDUlicaStanovanja** | int | NULL | - | FK |
| **BrojStana** | varchar(50) | NULL | - | Broj stana/kuće |
| **IDStrucnaSprema** | int | NULL | - | FK |
| **IDSkola** | int | NULL | - | FK |
| **IDZanimanjeRadnika** | int | NULL | - | FK |
| **IDRadnoMesto** | int | NULL | - | FK |
| **Koeficijent** | money | NULL | - | Koeficijent |
| **LicniKoeficijent** | money | NULL | - | Lični koeficijent |
| **OznakaInvalidnosti** | varchar(10) | NULL | - | % invalidnosti |
| **BrojKnjizice** | varchar(50) | NULL | - | Broj zdravstvene knjižice |
| **BrojLicneKarte** | varchar(50) | NULL | - | Broj ličneKarte |
| **IzdataOd** | varchar(50) | NULL | - | Izdavač |
| **BrojRadneKnjizice** | varchar(50) | NULL | - | Broj radne knjižice |
| **Narodnost** | varchar(50) | NULL | - | Narodnost |
| **BracnoStanje** | varchar(50) | NULL | - | Bračno stanje |
| **KonfesijaVirman** | int | NULL | - | FK - Religija |
| **Slava** | varchar(50) | NULL | - | Slava (Srpska) |
| **DatumSlave** | datetime | NULL | - | Datum slave |
| **KrvnaGrupa** | varchar(10) | NULL | - | Krvna grupa |
| **DavalacPuta** | bit | NULL | 0 | Da li je davalac putne karte |
| **StazGodina** | int | NULL | - | Staž - Godina |
| **StazMeseci** | int | NULL | - | Staž - Mesec |
| **StazDana** | int | NULL | - | Staž - Dan |
| **StazGodinaPreduzece** | int | NULL | - | Staž u preduzeću - Godina |
| **StazMeseciPreduzece** | int | NULL | - | Staž u preduzeću - Mesec |
| **StazDanaPreduzece** | int | NULL | - | Staž u preduzeću - Dan |
| **IDBanka** | int | NULL | - | FK → Banka |
| **BrojTekucegRacuna** | varchar(50) | NULL | - | Tekući račun |
| **DatumZaposlenja** | datetime | NULL | - | Datum zaposlenja |
| **Status** | varchar(20) | NULL | - | Status (Aktivan/Neaktivan) |
| **DatumOdjave** | datetime | NULL | - | Datum odjave |
| **RazlogOdjave** | varchar(50) | NULL | - | Razlog |
| **BrojDanaGodOdmora** | int | NULL | - | Dani godišnjeg odmora |
| **Sindikat** | varchar(20) | NULL | - | Sindikalni broj |
| **Telefon** | varchar(20) | NULL | - | Telefon |
| **email** | varchar(20) | NULL | - | Email |
| **Mobilni** | varchar(20) | NULL | - | Mobilni telefon |
| **SviRadniciTimeStamp** | timestamp | NULL | - | RowVersion |
| **TrgovackiPutnik** | bit | NOT NULL | 0 | Trgovački putnik |
| **Teritorija** | varchar(20) | NULL | - | Teritorija |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija |
| **INDSinh** | int | NULL | 0 | Indikator |

---

### tblNacinOporezivanja - Načini Oporezivanja

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDNacinOporezivanja** | int | NOT NULL | - | PK |
| **Opis** | varchar(255) | NOT NULL | - | Opis (npr. "PDV - Obaveza", "PDV - Oslobođenje") |
| **Znak** | int | NOT NULL | - | Indikator (1 = Nabavka, 0 = Prodaja) |
| **ObracunAkciza** | smallint | NULL | - | Obračun akcize (0/1) |
| **ObracunPorez** | smallint | NULL | - | Obračun poreza (0/1) |
| **Napomena** | varchar(255) | NULL | - | Napomena |
| **ObracunPorezPomocni** | smallint | NULL | - | Pomoćni obračun |
| **NacinOporezivanjaTimeStamp** | timestamp | NULL | - | RowVersion |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija |
| **INDSinh** | int | NULL | 0 | Indikator |

---

### tblValuta - Valute

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDValuta** | int | NOT NULL | IDENTITY(1,1) | PK |
| **Oznaka** | char(5) | NOT NULL | - | UNIQUE - Kod valute (RSD, EUR, USD...) |
| **NazivValute** | varchar(50) | NOT NULL | - | Naziv (Dinar, Evro...) |
| **NazivZemlje** | varchar(50) | NULL | - | Država |
| **Paritet** | int | NULL | - | Paritet (koliko jedinica = 1 RSD) |
| **Prikaz** | int | NULL | -1 | Prikazati (-1=da, 0=ne) |
| **Sort** | int | NULL | 99 | Redosled |
| **ValutaTimeStamp** | timestamp | NULL | - | RowVersion |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija |
| **INDSinh** | int | NULL | 0 | Indikator |

---

### tblPoreskaStopa - PDV Stope

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDPoreskaStopa** | char(2) | NOT NULL | - | PK (01, 02, 03...) |
| **Naziv** | varchar(100) | NOT NULL | - | Naziv (npr. "Standardna 20%") |
| **ProcenatPoreza** | float | NOT NULL | 0 | Procenat poreza (20.0, 10.0, 0.0) |
| **PoreskaStopaTimeStamp** | timestamp | NULL | - | RowVersion |
| **IDPoreskaStopaAutonumber** | int | NOT NULL | IDENTITY(1,1) | Autonumber za internal |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija |
| **INDSinh** | int | NULL | 0 | Indikator |
| **HcpStopa** | int | NULL | - | HCP stopa |

---

### tblUlazniRacuniIzvedeni - Vrste Troškova

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDUlazniRacuniIzvedeni** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDUlazniRacuniOsnovni** | int | NOT NULL | - | FK → tblUlazniRacuniOsnovni |
| **Opis** | varchar(255) | NULL | - | Opis vrste troška (Transport, Carina, Osiguranje) |
| **IDSpecifikacija** | int | NOT NULL | - | FK → tblSpecifikacija |
| **TroskoviZaDokument** | bit | NOT NULL | 0 | Troškovi za dokument |
| **UlazniRacuniIzvedeniTimeStamp** | timestamp | NULL | - | RowVersion |
| **IDSinhINS** | int | NULL | 0 | Sinhronizacija |
| **IDSinhUPD** | int | NULL | 0 | Sinhronizacija |
| **INDSinh** | int | NULL | 0 | Indikator |
| **PN** | int | NULL | - | Radni broj |

---

### tblNacinDeljenjaTroskova - Načini Raspodele Troškova

| Atribut | Tip | Opis |
|---------|-----|------|
| **IDNacinDeljenjaTroskova** | int | PK |
| **Naziv** | varchar | Naziv metode |
| **OpisNacina** | varchar | Detaljno objašnjenje |

**Vrednosti:**
- 1 = Po količini stavki
- 2 = Po vrednosti stavki  
- 3 = Ručna raspodela (unos iznosa po stavki)

---

## 📊 TRANSAKCIONE TABELE - POTPUNE DEFINICIJE

### tblDokument - Glavni Dokument

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDDokument** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDVrstaDokumenta** | char(2) | NOT NULL | - | FK → tblVrstaDokumenta (UR, ND, OT...) |
| **BrojDokumenta** | varchar(30) | NOT NULL | - | Broj dokumenta |
| **BrojDokumentaINT** | int | NOT NULL | 0 | Broj kao integer za sortiranje |
| **Godina** | int | NULL | - | Godina dokumenta |
| **Datum** | datetime | NOT NULL | - | Datum evidentiranja |
| **IDPartner** | int | NULL | - | FK → tblPartner (Dobavljač/Kupac) |
| **IDOrganizacionaJedinica** | int | NOT NULL | - | FK → tblOrganizacionaJedinica (Magacin) |
| **IDInterniPartner** | int | NULL | - | FK → Partner |
| **DatumValute** | datetime | NULL | - | Datum valute |
| **DatumDPO** | datetime | NULL | - | Datum primanja obveze |
| **PartnerBrojDokumenta** | varchar(200) | NULL | - | Broj dokumenta kod partnera |
| **PartnerDatumDokumenta** | datetime | NULL | - | Datum dokumenta kod partnera |
| **IDRadnik** | int | NULL | - | FK → tblSviRadnici (Referent) |
| **IDReferentniDokument** | int | NULL | - | FK → tblDokument (Narudžbenica) |
| **Napomena** | varchar(max) | NULL | - | Beleške |
| **NapomenaSystem** | varchar(max) | NULL | - | Sistemske beleške |
| **ObradjenDokument** | bit | NOT NULL | 0 | Obrado li se (0=ne, 1=da) |
| **ProknjizenDokument** | bit | NOT NULL | 0 | Proknjiženo (0=ne, 1=da) |
| **UserName** | varchar(20) | NULL | - | Korisnik koji je kreirao |
| **UserLokacija** | varchar(30) | NULL | - | Lokacija korisnika |
| **UserDatum** | datetime | NULL | - | Datum kreiranja |
| **IDNacinPlacanja** | int | NULL | - | FK → tblNacinPlacanja |
| **IDNacinOporezivanja** | int | NULL | - | FK → tblNacinOporezivanja |
| **IDStatus** | int | NULL | - | FK → tblStatus |
| **ObracunAkciza** | smallint | NOT NULL | 0 | Obračun akcize (0=ne, 1=da) |
| **ObracunPorez** | smallint | NOT NULL | 0 | Obračun poreza (0=ne, 1=da) |
| **ObracunPorezPomocni** | smallint | NOT NULL | 0 | Pomoćni porez |
| **IDValuta** | int | NULL | - | FK → tblValuta |
| **KursValute** | money | NOT NULL | - | Kurs valute |
| **AvansIznos** | money | NOT NULL | 0 | Avanس |
| **IDModelKontiranja** | int | NULL | - | FK |
| **IDMestoIsporuke** | int | NULL | - | FK → tblMestoIsporuke |
| **TrebovanjeIDArtikal** | int | NULL | - | FK → tblArtikal |
| **TrebovanjeKolicina** | money | NOT NULL | 0 | Trebovana količina |
| **IznosPrevaranti** | money | NOT NULL | 0 | Iznos prevarante |
| **ZavisniTroskoviBezPDVa** | money | NOT NULL | 0 | Zavisni troškovi bez PDV |
| **ZavisniTroskoviPDV** | money | NOT NULL | 0 | Zavisni troškovi sa PDV |
| **IDTroskovnoMesto** | int | NULL | - | FK |
| **IDVozac** | int | NULL | - | FK → tblSviRadnici |
| **IDVozilo** | int | NULL | - | FK → tblVozilo |
| **IDLinijaProizvodnje** | int | NULL | - | FK |
| **IDSvrhaInternihRacuna** | int | NULL | - | FK |
| **UserNameK** | varchar(30) | NULL | - | Korisnik koji je proknjižio |
| **UserLokacijaK** | varchar(30) | NULL | - | Lokacija |
| **UserDatumK** | datetime | NULL | - | Datum proknjiženja |
| **Bruto** | money | NULL | - | Bruto iznos |
| **Neto** | money | NULL | - | Neto iznos |
| **GranicniPrelaz** | varchar(200) | NULL | - | Granični prelaz |
| **IDStorniranogDokumenta** | int | NULL | - | FK → tblDokument (Stornirani dokument) |
| **IDUlazniRacuniOsnovni** | int | NULL | - | FK → tblUlazniRacuniOsnovni |
| **IznosCek** | money | NOT NULL | 0 | Iznos plaćanja čekom |
| **IznosKartica** | money | NOT NULL | 0 | Iznos plaćanja karticom |
| **IznosGotovina** | money | NOT NULL | 0 | Iznos gotovinom |
| **BrojPutnogNaloga** | varchar(50) | NULL | - | Broj putnog naloga |
| **Otpremljeno** | bit | NULL | - | Otpremljeno |
| **VremeRazvoza** | varchar(50) | NULL | - | Vreme |
| **BrojDokAlt** | varchar(max) | NULL | - | Alternativni broj |
| **Napomena2** | varchar(max) | NULL | - | Dodatne beleške |
| **Napomena3** | varchar(max) | NULL | - | Dodatne beleške |
| **SinhronizovanAccess** | bit | NOT NULL | 0 | Sinhronizovano sa Access |
| **Feler** | bit | NOT NULL | 0 | Greška u dokumentu |
| **IndikatorNaknadnogOdobrenja** | varchar(1) | NULL | - | Indikator |
| **OdobrioNaknadnuIsporuku** | varchar(30) | NULL | - | Ko je odobrio |
| **ImePrezimeMetro** | varchar(50) | NULL | - | Ime |
| **BrojNarudzbenice** | varchar(50) | NULL | - | Broj narudžbenice |
| **BrojProdavnice** | varchar(50) | NULL | - | Broj prodavnice |
| **DatumNarudzbenice** | datetime | NULL | - | Datum |
| **IDTekuciRacun** | int | NULL | - | FK |
| **PozivNaBroj** | varchar(50) | NULL | - | Poziv na broj |
| **VrednostSaRacuna** | money | NULL | - | Vrednost |
| **PozivNaBroj1** | varchar(50) | NULL | - | Poziv na broj 1 |
| **DokumentTimeStamp** | timestamp | NOT NULL | - | RowVersion - KONKURENTNOST |
| **Rok** | datetime | NULL | - | Rok |
| **Kilometraza** | money | NULL | - | Kilometraža |
| **Kontakt** | varchar(50) | NULL | - | Kontakt |
| **Registracija** | varchar(50) | NULL | - | Registracija vozila |
| **IDRadnik2** | int | NULL | - | FK → tblSviRadnici |
| **DodatniRadoviIznos** | money | NULL | - | Dodatni radovi |
| **IDPartner2** | int | NULL | - | FK → tblPartner |
| **ZakljucanDokument** | bit | NULL | 0 | Zaključen (Završen) |
| **IDVrstaTroska** | int | NULL | - | FK |
| **IDPrikolica** | int | NULL | - | FK |
| **IDMesto1** | int | NULL | - | FK → tblMesto |
| **IDMesto2** | int | NULL | - | FK → tblMesto |
| **IDMerenje** | int | NULL | - | FK |

---

### tblStavkaDokumenta - Stavke (KRITIČNO)

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDStavkaDokumenta** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDDokument** | int | NOT NULL | - | FK → tblDokument (CASCADE) |
| **IDArtikal** | int | NOT NULL | - | FK → tblArtikal |
| **IDOrganizacionaJedinica** | int | NULL | - | FK → tblOrganizacionaJedinica |
| **Kolicina** | money | NOT NULL | - | Količina (CHECK Kolicina <> 0) |
| **FakturnaCena** | money | NOT NULL | 0 | Fakturna cena po JM |
| **NabavnaCena** | money | NOT NULL | 0 | Nabavna cena |
| **MagacinskaCena** | money | NOT NULL | 0 | Magacinska cena |
| **RabatDokument** | money | NOT NULL | 0 | Rabat na nivou dokumenta |
| **ProcenatAktivneMaterije** | money | NOT NULL | 0 | Procenat aktivne materije |
| **Zapremina** | money | NOT NULL | 0 | Zapremina |
| **Akciza** | money | NOT NULL | 0 | Iznos akcize po JM |
| **KoeficijentKolicine** | money | NOT NULL | 1 | Koeficijent količine |
| **Rabat** | money | NOT NULL | 0 | Rabat (iznos) |
| **Marza** | money | NOT NULL | 0 | Marža (iznos) |
| **IznosMarze** | money | NOT NULL | 0 | Iznos marže |
| **ProcenatPoreza** | money | NOT NULL | 0 | Procenat PDV |
| **ProcenatPorezaMP** | money | NOT NULL | 0 | Procenat PDV - Pomoćni |
| **IznosPDV** | money | NOT NULL | 0 | Iznos PDV |
| **IznosPDVsaAkcizom** | money | NOT NULL | 0 | Iznos PDV sa akcizom |
| **IznosAkciza** | money | NOT NULL | 0 | Iznos akcize (Kolicina × Akciza) |
| **IDPoreskaStopa** | char(2) | NULL | - | FK → tblPoreskaStopa |
| **ZavisniTroskovi** | money | NOT NULL | 0 | Zavisni troškovi sa PDV |
| **ZavisniTroskoviBezPoreza** | money | NOT NULL | 0 | Zavisni troškovi bez PDV |
| **Iznos** | money | NOT NULL | 0 | Ukupan iznos stavke |
| **ValutaCena** | money | NOT NULL | 0 | Cena u devizi |
| **ValutaIznos** | money | NOT NULL | 0 | Iznos u devizi |
| **IDJedinicaMere** | varchar(6) | NOT NULL | - | FK → tblJedinicaMere |
| **Pakovanje** | int | NOT NULL | 0 | Broj pakovanja |
| **ObracunAkciza** | smallint | NOT NULL | 0 | Obračun akcize (0/1) |
| **ObracunPorez** | smallint | NOT NULL | 0 | Obračun poreza (0/1) |
| **IDNacinOporezivanja** | int | NULL | - | FK → tblNacinOporezivanja |
| **IDStatus** | int | NULL | - | FK → tblStatus |
| **VrednostObracunPDV** | money | NULL | - | COMPUTED: PDV |
| **VrednostObracunAkciza** | money | NULL | - | COMPUTED: Akciza |
| **Masa** | money | NOT NULL | 0 | Masa |
| **Opis** | varchar(1024) | NULL | - | Detaljne beleške |
| **ProizvodnjaKolicina** | float | NOT NULL | 0 | Proizvodnja količina |
| **ProizvodnjaIDJedinicaMere** | char(6) | NULL | - | FK |
| **ProizvodnjaKoeficijentKolicine** | float | NOT NULL | 0 | Koeficijent |
| **IDObrociNarudzbinaStavka** | int | NULL | - | FK |
| **IDVrstaObroka** | int | NULL | - | FK |
| **StavkaDokumentaTimeStamp** | timestamp | NULL | - | RowVersion - KONKURENTNOST |
| **IDDnevnaStanjaMagacinskoPromeneM1** | int | NOT NULL | 0 | FK |
| **IDDnevnaStanjaMagacinskoPromeneM2** | int | NOT NULL | 0 | FK |
| **IDDnevnaStanjaRobnoPromeneM1** | int | NOT NULL | 0 | FK |
| **IDDnevnaStanjaRobnoPromeneM2** | int | NOT NULL | 0 | FK |
| **IDDnevnaStanjaVPPromeneM1** | int | NOT NULL | 0 | FK |
| **IDDnevnaStanjaVPPromeneM2** | int | NOT NULL | 0 | FK |
| **ObracunPorezPomocni** | smallint | NOT NULL | 0 | Pomoćni porez |
| **IDUlazniRacuniOsnovni** | int | NULL | - | FK |
| **RabatAkcija** | money | NOT NULL | 0 | Rabat akcija |
| **IsporukaRobe** | bit | NULL | - | Roba isporučena |
| **Rabat2** | money | NOT NULL | 0 | Drugi rabat |
| **ZadnjaNabavnaCena** | money | NULL | 0 | Poslednja nabavna cena |
| **ProsecnaCena** | money | NULL | 0 | Prosečna cena |
| **ValutaBrojDana** | int | NULL | - | Broj dana |
| **ValutaDatum** | datetime | NULL | - | Datum valute |
| **VrednostBezPDV** | money | NULL | 0 | Vrednost bez PDV |
| **ObaveznaOprema** | varchar(50) | NULL | - | Obavezna oprema |
| **DopunskaOprema** | varchar(50) | NULL | - | Dopunska oprema |
| **ProsecnaCenaOJ** | money | NULL | - | Prosečna cena OJ |
| **PovratnaNaknada** | money | NULL | 0 | Povratna naknada |
| **StaraCena** | money | NULL | - | Stara cena |
| **IDBoja** | int | NULL | - | FK → Boja |

---

### tblDokumentTroskovi - Zavisni Troškovi

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDDokumentTroskovi** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDDokument** | int | NOT NULL | - | FK → tblDokument (CASCADE) |
| **IDPartner** | int | NOT NULL | - | FK → tblPartner (Partner koji nosi trošak) |
| **IDVrstaDokumenta** | char(2) | NOT NULL | - | FK → tblVrstaDokumenta (Vrsta troška) |
| **BrojDokumenta** | varchar(max) | NOT NULL | - | Broj dokumenta troška |
| **DatumDPO** | datetime | NOT NULL | - | Datum primanja obveze |
| **DatumValute** | datetime | NULL | - | Datum valute |
| **Opis** | varchar(max) | NULL | - | Opis troška |
| **IDStatus** | int | NOT NULL | - | FK → tblStatus |
| **IDValuta** | int | NULL | - | FK → tblValuta |
| **Kurs** | money | NULL | 0 | Kurs valute |
| **DokumentTroskoviTimeStamp** | timestamp | NULL | - | RowVersion |

---

### tblDokumentTroskoviStavka - Stavke Zavisnih Troškova (KRITIČNO)

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDDokumentTroskoviStavka** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDDokumentTroskovi** | int | NOT NULL | - | FK → tblDokumentTroskovi |
| **IDNacinDeljenjaTroskova** | int | NOT NULL | - | FK → tblNacinDeljenjaTroskova (1/2/3) |
| **SveStavke** | bit | NOT NULL | 1 | Sve stavke (1=sve, 0=određene) |
| **Iznos** | money | NOT NULL | 0 | Iznos troška |
| **IDUlazniRacuniIzvedeni** | int | NOT NULL | - | FK → tblUlazniRacuniIzvedeni (Vrsta) |
| **IDStatus** | int | NOT NULL | - | FK → tblStatus |
| **ObracunPorezTroskovi** | int | NOT NULL | 0 | Obračun poreza (0/1) |
| **DodajPDVNaTroskove** | int | NOT NULL | 0 | Dodaj PDV (0/1) |
| **DokumentTroskoviStavkaTimeStamp** | timestamp | NULL | - | RowVersion - KONKURENTNOST |
| **IznosValuta** | money | NULL | 0 | Iznos u devizi |
| **Gotovina** | money | NOT NULL | 0 | Plaćeno gotovinom |
| **Kartica** | money | NOT NULL | 0 | Plaćeno karticom |
| **Virman** | money | NOT NULL | 0 | Plaćeno virmаnom |
| **Kolicina** | money | NULL | 0 | Količina (ako je po količini) |

---

### tblDokumentTroskoviStavkaPDV - PDV na Stavke Troškova

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **IDDokumentTroskoviStavkaPDV** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDDokumentTroskoviStavka** | int | NOT NULL | - | FK → tblDokumentTroskoviStavka (CASCADE) |
| **IDPoreskaStopa** | char(2) | NOT NULL | - | FK → tblPoreskaStopa |
| **IznosPDV** | money | NOT NULL | 0 | Iznos PDV |
| **DokumentTroskoviStavkaPDVTimeStamp** | timestamp | NULL | - | RowVersion |
| **UNIQUE:** (IDDokumentTroskoviStavka, IDPoreskaStopa) | - | - | - | Samo jedna PDV stopa po stavki |

---

### tblDokumentAvansPDV - PDV na Avanse

| Atribut | Tip | Mogućnost NULL | Default | Opis |
|---------|-----|---|---------|------|
| **DokumentAvansPDV** | int | NOT NULL | IDENTITY(1,1) | PK |
| **IDDokument** | int | NOT NULL | - | FK → tblDokument (CASCADE) |
| **IDPoreskaStopa** | char(2) | NOT NULL | - | FK → tblPoreskaStopa |
| **IznosPDVAvansa** | money | NOT NULL | - | Iznos PDV na avans |
| **BrojAvansa** | varchar(50) | NULL | - | Broj avansa |
| **DatumAvansa** | datetime | NULL | - | Datum avansa |
| **OsnovicaPoStopi** | money | NULL | - | Osnovica po stopi |
| **KodOslobodjenja** | varchar(50) | NULL | - | Kod oslobođenja |
| **DokumentAvansPDVTimeStamp** | timestamp | NULL | - | RowVersion |

---

## 🔌 STORED PROCEDURE-I - DETALJNO

### 1. spPartnerComboStatusNabavka

**Svrha:** Učitaj sve partnere sa statusom za nabavku

**Ulazni parametri:** Nema

**Izlazni parametri (SELECT rezultat):**
```
- NazivPartnera (varchar(255)) - Prikazano kao [NAZIV PARTNERA]
- IDPartner (int) - ID partnera
- Mesto (varchar) - Naziv mesta
- Opis (varchar) - Status
- IDStatus (int)
- IDNacinOporezivanjaNabavka (int) - Način oporezivanja
- ObracunAkciza (smallint) - Obračun akcize
- ObracunPorez (smallint) - Obračun poreza
- IDReferent (int)
- SifraPartner (varchar(13)) - Prikazano kao [ŠIFRA]
```

**SQL Logika:**
- INNER JOIN tblStatus - Samo aktivni partneri
- LEFT OUTER JOIN tblMesto - Mesto može biti NULL
- Sortira po NazivPartnera (ABC)

---

### 2. spOrganizacionaJedinicaCombo

**Svrha:** Učitaj magacine/OJ za određenu vrstu dokumenta

**Ulazni parametri:**
- `@IDVrstaDokumenta` varchar(2) = '' (UR, ND, OT...)

**Izlazni parametri (SELECT rezultat):**
```
- IDOrganizacionaJedinica (int)
- Naziv (varchar) - Prikazano kao [NAZIV MAGACINA]
  Format: "SifraOrganizacionaJedinica + ' ' + Naziv"
- Mesto (varchar) - Mesto (iz funkcije tblMesto_ID_NazivMesta)
- SifraOrganizacionaJedinica (varchar(6))
```

**SQL Logika:**
```
IF @IDVrstaDokumenta postoji u tblVrstaDokumentaOJ:
   - INNER JOIN tblVrstaDokumentaOJ
   - Filter po @IDVrstaDokumenta
   - Sortira po SifraSort (numerički)
ELSE:
   - Vrati sve magacine
   - Sortira po SifraSort
```

---

### 3. spNacinOporezivanjaComboNabavka

**Svrha:** Učitaj sve načine oporezivanja za nabavku

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDNacinOporezivanja (int)
- Opis (varchar(255))
- ObracunAkciza (smallint) - 0 ili 1
- ObracunPorez (smallint) - 0 ili 1
- ObracunPorezPomocni (smallint) - 0 ili 1
```

**SQL Logika:**
- WHERE Znak = 1 (samo za nabavku)
- TOP 100 PERCENT (za sortiranje)
- Sortira po Opis (ABC)

---

### 4. spReferentCombo

**Svrha:** Učitaj sve radnike/referente

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDRadnik (int)
- ImeRadnika (varchar) - Prikazano kao [IME I PREZIME]
  Format: Ime (može biti prazno ako je samo prezime)
- SifraRadnika (varchar(10))
```

**SQL Logika:**
- Sortira po: PrezimeRadnika + ' ' + ImeRadnika (ABC)

---

### 5. spDokumentNDCombo

**Svrha:** Učitaj sve narudžbenice (ND dokumenta)

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDDokument (int)
- BrojDokumenta (varchar(30))
- Datum (datetime)
- NazivPartnera (varchar(255))
```

**SQL Logika:**
- INNER JOIN tblPartner
- WHERE IDVrstaDokumenta = 'ND'
- Sortira po datumu ili broju

---

### 6. spPoreskaStopaCombo

**Svrha:** Učitaj sve PDV stope

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDPoreskaStopa (char(2)) - (01, 02, 03...)
- Naziv (varchar(100)) - (Standardna 20%, Umanjena 10%...)
```

**SQL Logika:**
- TOP 100 PERCENT (za sortiranje)
- Sortira po IDPoreskaStopa (numerički)

---

### 7. spArtikalComboUlaz

**Svrha:** Učitaj sve artikle sa PDV informacijom

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDArtikal (int)
- SifraArtikal (varchar) - Prikazano kao [SIFRA]
- NazivArtikla (varchar) - Prikazano kao [NAZIV ARTIKLA]
- IDJedinicaMere (varchar(6)) - Prikazano kao [JM]
- IDPoreskaStopa (char(2))
- ProcenatPoreza (float) - 20, 10, 0...
- Akciza (money) - Akciza po JM
- KoeficijentKolicine (money) - Koeficijent
- ImaLot (bit) - Da li artikal ima lot
- OtkupnaCena (money) - Otkupna cena
- PoljoprivredniProizvod (bit) - Da li je poljoprivredni proizvod
```

**SQL Logika:**
- INNER JOIN tblPoreskaStopa - Za procent PDV
- Sortira po SifraSort (alfanumerički)

---

### 8. spDokumentTroskoviLista

**Svrha:** Učitaj sve troškove i stavke troškova za dokument

**Ulazni parametri:**
- `@IDDokument` int - ID dokumenta

**Izlazni parametri:**
```
UNION dva SELECT-a:

1. Za zaglavlje troška:
   - IDDokumentTroskovi (int)
   - IDDokumentTroskoviStavka (int = NULL)
   - ListaTroškova (varchar(max))
     Format: "NazivPartnera + ' (' + IDVrstaDokumenta + ': ' + BrojDokumenta + ')'"
   - OSNOVICA (money) - SUM(Iznos)
   - PDV (money) - SUM(IznosPDV)

2. Za stavku troška:
   - IDDokumentTroskovi (int)
   - IDDokumentTroskoviStavka (int)
   - ListaTroškova (varchar(max))
     Format: "'  ' + UPPER(tblUlazniRacuniIzvedeni.Opis)"
   - OSNOVICA (money) - Iznos
   - PDV (money) - SUM(IznosPDV)
```

**SQL Logika:**
- Prvo INSERT zaglavlja troškova sa GROUP BY
- Zatim INSERT stavki sa LEFT OUTER JOIN na PDV
- SELECT rezultat ORDER BY IDDokumentTroskovi, IDDokumentTroskoviStavka

---

### 9. spUlazniRacuniIzvedeniTroskoviCombo

**Svrha:** Učitaj sve vrste troškova

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDUlazniRacuniIzvedeni (int)
- Naziv (varchar) - UPPER(tblUlazniRacuniIzvedeni.Opis)
- Opis (varchar) - tblUlazniRacuniOsnovni.Opis
- NazivSpecifikacije (varchar)
- ObracunPorez (smallint) - 0 ili 1
- IDULazniRacuniOsnovni (int)
```

**SQL Logika:**
- INNER JOIN tblUlazniRacuniOsnovni
- INNER JOIN tblSpecifikacija

---

### 10. spNacinDeljenjaTroskovaCombo

**Svrha:** Učitaj sve metode deljenja troškova

**Ulazni parametri:** Nema

**Izlazni parametri:**
```
- IDNacinDeljenjaTroskova (int) - (1, 2, 3)
- Naziv (varchar) - Po količini, Po vrednosti, Ručno
- OpisNacina (varchar) - Detaljno objašnjenje
```

**SQL Logika:**
- TOP 100 PERCENT
- Sortira po Naziv

---

### 11. spDokumentTroskoviArtikliCOMBO

**Svrha:** Učitaj sve artikle koji su u dokumentu (za raspodelu troškova)

**Ulazni parametri:**
- `@IDDokument` int - ID dokumenta

**Izlazni parametri:**
```
- IDStavkaDokumenta (int) - ID stavke
- SifraArtikal (varchar)
- NazivArtikla (varchar)
```

**SQL Logika:**
- INNER JOIN tblArtikal
- WHERE IDDokument = @IDDokument
- Sortira po SifraArtikal

---

## 📡 API ENDPOINTI - MAPIRANJE NA SP I TABELE

### Lookup Endpointi

| Endpoint | HTTP | SP | Rezultat | Koristi za |
|----------|------|----|-|-|
| `/api/v1/partners/combo` | GET | spPartnerComboStatusNabavka | Lista partnera | Combo - Dobavljač |
| `/api/v1/organizational-units/combo?docType=UR` | GET | spOrganizacionaJedinicaCombo | Lista OJ | Combo - Magacin |
| `/api/v1/taxation-methods/combo` | GET | spNacinOporezivanjaComboNabavka | Lista | Combo - Način oporezivanja |
| `/api/v1/referents/combo` | GET | spReferentCombo | Lista radnika | Combo - Referent |
| `/api/v1/reference-documents/combo?type=ND` | GET | spDokumentNDCombo | Lista ND | Combo - Narudžbenica |
| `/api/v1/tax-rates/combo` | GET | spPoreskaStopaCombo | Lista stopa | Combo - PDV stopa |
| `/api/v1/articles/combo` | GET | spArtikalComboUlaz | Lista artikala | Combo - Artikal |
| `/api/v1/cost-distribution-methods/combo` | GET | spNacinDeljenjaTroskovaCombo | Lista | Combo - Način raspodele |
| `/api/v1/cost-types/combo` | GET | spUlazniRacuniIzvedeniTroskoviCombo | Lista | Combo - Vrsta troška |
| `/api/v1/documents/{id}/cost-articles` | GET | spDokumentTroskoviArtikliCOMBO | Artikli u dokumentu | Raspodela troškova |

### Document Endpointi

| Endpoint | HTTP | Tabela | Rezultat | Opis |
|----------|------|--------|----------|------|
| `POST /api/v1/documents` | POST | tblDokument | IDDokument, DokumentTimeStamp | Kreiraj dokument |
| `GET /api/v1/documents` | GET | tblDokument | Lista sa paginacijom | Sve dokumente |
| `GET /api/v1/documents/{id}` | GET | tblDokument | Kompletan dokument | Detalji |
| `PUT /api/v1/documents/{id}` | PUT | tblDokument | OK | Update zaglavlja |
| `DELETE /api/v1/documents/{id}` | DELETE | tblDokument | OK | Obriši dokument |

### Line Items Endpointi

| Endpoint | HTTP | Tabela | Rezultat | Opis |
|----------|------|--------|----------|------|
| `POST /api/v1/documents/{id}/items` | POST | tblStavkaDokumenta | IDStavkaDokumenta, ETag | Kreiraj stavku |
| `GET /api/v1/documents/{id}/items` | GET | tblStavkaDokumenta | Lista stavki | Sve stavke |
| `GET /api/v1/documents/{id}/items/{itemId}` | GET | tblStavkaDokumenta | Stavka sa ETag | Detalji |
| `PATCH /api/v1/documents/{id}/items/{itemId}` | PATCH | tblStavkaDokumenta | OK + novi ETag | Autosave - **ETag konkurentnost** |
| `DELETE /api/v1/documents/{id}/items/{itemId}` | DELETE | tblStavkaDokumenta | OK | Soft delete |

### Costs Endpointi

| Endpoint | HTTP | Tabela | Rezultat | Opis |
|----------|------|--------|----------|------|
| `POST /api/v1/documents/{id}/costs` | POST | tblDokumentTroskovi | IDDokumentTroskovi | Kreiraj trošak |
| `GET /api/v1/documents/{id}/costs` | GET | tblDokumentTroskovi + View | Lista troškova | spDokumentTroskoviLista SP |
| `GET /api/v1/documents/{id}/costs/{costId}` | GET | tblDokumentTroskovi | Detalji | Sa stavkama |
| `POST /api/v1/documents/{id}/costs/{costId}/items` | POST | tblDokumentTroskoviStavka | IDStavka, ETag | Kreiraj stavku troška |
| `PATCH /api/v1/documents/{id}/costs/{costId}/items/{itemId}` | PATCH | tblDokumentTroskoviStavka | OK + ETag | **ETag konkurentnost** |
| `DELETE /api/v1/documents/{id}/costs/{costId}/items/{itemId}` | DELETE | tblDokumentTroskoviStavka | OK | Soft delete |
| `POST /api/v1/documents/{id}/costs/{costId}/distribute` | POST | tblDokumentTroskoviStavka | OK | Primeni raspodelu |

---

## 👀 DATABASE VIEW-I

### vwDocumentLineItemWithAudit - Za Frontend

```sql
CREATE VIEW vwDocumentLineItemWithAudit AS
SELECT 
    s.[IDStavkaDokumenta],
    s.[IDDokument],
    s.[IDArtikal],
    s.[Kolicina],
    s.[FakturnaCena],
    s.[Rabat],
    s.[Marza],
    s.[IDPoreskaStopa],
    s.[ProcenatPoreza],
    s.[IznosPDV],
    s.[Iznos],
    s.[ObracunAkciza],
    s.[ObracunPorez],
    CONVERT(NVARCHAR(100), 
        CONVERT(BINARY(8), s.[StavkaDokumentaTimeStamp]), 2) AS [ETag],
    s.[StavkaDokumentaTimeStamp] AS [RowVersion],
    ISNULL(s.[UserDatum], GETUTCDATE()) AS [UpdatedAt]
FROM [dbo].[tblStavkaDokumenta] s
WHERE s.[IsDeleted] = 0
```

### vwDocumentWithTotals - Za Zaglavlje

```sql
CREATE VIEW vwDocumentWithTotals AS
SELECT 
    d.[IDDokument],
    d.[BrojDokumenta],
    d.[Datum],
    d.[IDPartner],
    p.[NazivPartnera],
    d.[IDOrganizacionaJedinica],
    d.[ZavisniTroskoviBezPDVa],
    d.[ZavisniTroskoviPDV],
    SUM(s.[Iznos]) AS [StravaUkupno],
    SUM(s.[IznosPDV]) AS [PDVUkupno],
    d.[DokumentTimeStamp]
FROM [dbo].[tblDokument] d
LEFT JOIN [dbo].[tblPartner] p ON d.[IDPartner] = p.[IDPartner]
LEFT JOIN [dbo].[tblStavkaDokumenta] s ON d.[IDDokument] = s.[IDDokument]
GROUP BY d.[IDDokument], d.[BrojDokumenta], d.[Datum], 
         d.[IDPartner], p.[NazivPartnera], d.[IDOrganizacionaJedinica],
         d.[ZavisniTroskoviBezPDVa], d.[ZavisniTroskoviPDV], d.[DokumentTimeStamp]
```

---

## ✅ FINALNE NAPOMENE

**KRITIČNE INFORMACIJE:**

1. **RowVersion/ETag Konkurentnost:**
   - `tblStavkaDokumenta.StavkaDokumentaTimeStamp` - TIMESTAMP (RowVersion)
   - `tblDokumentTroskoviStavka.DokumentTroskoviStavkaTimeStamp` - TIMESTAMP (RowVersion)
   - Frontend koristi Base64 enkodiran RowVersion kao ETag
   - PATCH zahtev sa If-Match header-om

2. **Soft Delete:**
   - Sve stavke: `IsDeleted` bit (default 0)
   - Query uvek: `WHERE IsDeleted = 0`

3. **Stored Procedure-i:**
   - Koriste se za sve combo upite
   - Backend mapira rezultate na DTO-e
   - Frontend prima JSON, a combo vrednosti su ID/Naziv parovi

4. **Zavisni Troškovi - Raspodela:**
   - Metoda 1: Po količini stavki
   - Metoda 2: Po vrednosti stavki
   - Metoda 3: Ručna raspodela po stavki
   - Svaka stavka troška ima PDV (u tblDokumentTroskoviStavkaPDV)

---

**OVAJ DOKUMENT SADRŽI SVE PODATKE POTREBNE ZA IMPLEMENTACIJU!**

Svi atributi, tipovi podataka, default vrednosti, foreign key-i, uniqueness constraint-i i logika su eksplicitno navedeni.