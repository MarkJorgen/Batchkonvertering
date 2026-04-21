/// <remarks>
///     $Workfile:	ReadMe.txt $
///   Description:	Dette er ReadMe.txt filen som beskriver projektet.
///
///                 Nedenstående afviking sker i IndkaldBilag.Afvikling
///
///					Denne console app dk.gi.crm.app.konto.kontrol.indkaldbilag henter kontroller til intensiveret 
///                 kontrol der sendes e-boks breve og dokumenter til kontaktperson for at indhente bilag og advisering                  
///                 til huslejenævn. 
///
///					De 3 afsendte dokumenter uploades på opgave. Og kontroller flyttes til kontrol Bilagrekvireret.
///
///                 Til afsendelse af digital post OneTooX. Interface til KMD/OneTooX webservice findes i
///                 projektet webservice som er sourcekode fra KMD.
///
///   Benytter følgende GI Pakker:
///
///                 dk.gi.crm.data
///                 dk.gi.crm.konto.forretning
///                 dk.gi.crm.library
///                 dk.gi.digitalpost.servicelink.library
///                 Aspose til dokument generering
///
///                 dk.gi.app.console.template.GiNugetSrc
///                 Og flette skabeloner i biblioteket : Skabeloner 
///
///   VIGTIGT		Udover nuget pakkerne skal nedenstående resourser inkluderes i afvikende projekt
///
///                 1983_N-201-Grundejernes-Investeringsfond_1.pke (certifikat fra KMD)
///
///					Konfiguration
///
///                 <add key="WebserviceUrl" value="https://privat.doc2mail.dk/delivery/FileUploader.asmx"/>
///                 <add key="Certificat" value="1983_N-201-Grundejernes-Investeringsfond_1.pke"/>
///
///   MEGET VIGTIGT Dette program må ikke køre på dev eller tst medmindre det er en planlagt test.
///                 Fordi der sendes breve til rigtige kontaktpersoner medmindre det er en planlagt test.
///
///					Afsendelse af digital post kan slåes fra/til med app config key/value : TilladSendTilDigitalPost true/false
///
///                 Test kan laves på en kontrolsag adgangen i klassen IndkaldBilag
///                 foreach (AP_stikprve kontrol in kontroller.Where(k=>k.AP_stikprveId == "????")
///
/// YYYY MM DD INIT History
/// =============================================================================================================================
/// 2024 11 29 RCL  Template changes.
/// 2024 11 29 RCL  Not in use anymore Vejledning om kontrol af regnskaber.pdf 
/// 2024 11 15 RCL  Template changes.
/// 2024 11 13 RCL  Nuget update objecttypecode  
/// 2024 10 28 JMW  Rettet hardcoded ID på Systemuser(i BLL), samt tilføjet parameter SystemUserIdForForbindelse for samme
/// 2024 07 04 RCL  Template changes.
/// 2024 03 20 RCL  Added KontrolForbindKontaktRequest
/// 2024 02 26 RCL  Template changes.
/// 2024 02 26 RCL  kontrolårsag "Udv. pga. manglende bilag" Udvpgamanglendebilag medtages nu.
/// 2023 09 27 RCL  New build with error fix for secret address for tst/prod batch
/// 2023 04 19 RCL  We should update ap_regnskab with Ap_BlokerWEBkorrektion true according to HRL
/// 2023 01 31 RCL  Updated to NET 4.8 and converted to request/response, new console, new bll.lib etc. For tst/prod
/// 2022 10 19 RCL  Document switch in DanKontrolDokument
/// 2022 06 09 RCL  Tilføjet KontrolAssistenten igen som udtræksbar 
/// 2022 05 17 RCL  BilagLejeMaalHuslejenaevn ændret til ingen dubletter
/// 2022 05 03 RCL  Ny build - skal ikke bruges alligevel
/// 2022 03 30 RCL  Dan bilag med lejemål til huslejenævn - lagt test
/// 2022 03 29 RCL  Kode for luk aktivitet og ændring til flet fra version 2022.01.28 lagt ind igen
/// 2022 03 24 RCL  Dan bilag med lejemål til huslejenævn
/// 2022 03 14 JMW  Opdateret til CRM 9 samt Framework 4.6.2
/// 2022 01 25 RCL  Opdateret Console Template. Lagt i prod.
/// 2022 01 25 RCL  Ændret DAGSDATO til at være et flette felt og har opdateret templates
/// 2022 01 25 RCL  HUSK at ændre 9 til AP_stikprve_AP_Supplerendekontrol.KontrolAssistenten i DanKontrolDokument ved opdatering af data.
/// 2022 01 25 RCL  AP_stikprve_AP_Supplerendekontrol opdateret med KontrolAssistenten i DanKontrolDokument
/// 2021 11 29 RCL  Lukket og lagt i dev og GICRM90PRD
/// 2021 09 17 RCL  Flyttet indkaldelse af bilag dokument/flet fra GISikretService kald
/// 2021 09 06 JMW  Opdateret Console Template. (Fjernet licens filer som ikke bruges (Aspose))
/// 2021 04 14 RCL  Fjernet trim af Huslejenævnet og hardkodet Huslejenævn er fjernet fra flettebrev
/// 2021 04 09 RCL  Lukket med pakker Konto 2021.4.9.756 med data 2021.4.9.740
/// 2021 04 09 RCL  Ændringer/rettelser konto Huslejenævn
/// 2021 03 22 RCL  Afhængighed til webservice.dll fjernet med ny pakke dk.gi.digitalpost.servicelink.library 2021.2.11.820 
/// 2021 03 22 RCL  ap_regnskabstartdato ændret til ap_regnskabslutdato i indkaldbilag 
/// 2021 02 08 RCL  Første version godkendt
/// 2021 01 27 RCL  Parat til test
/// </remarks>