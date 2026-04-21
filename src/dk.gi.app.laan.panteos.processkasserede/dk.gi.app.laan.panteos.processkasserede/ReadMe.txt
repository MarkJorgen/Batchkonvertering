/// <remarks>
///     $Workfile: ReadMe.txt $
///   Description: Dette er ReadMe.txt filen som beskriver projektet.
///				   Projektet udgøres af en konsol applikation som udvælger et antal Lån entiteter og opdaterer dem med data fra AX.
///
///                Alle lån med livcyklus efter "udbetalt", men ikke indfriet skal dagligt (eller efter nærmere aftalt cyklus) 
///                opdateres med værdier fra AX svarende til den web-service, der køres onload. 
///				   Se evt. Logica JIRA GRI-550 for mulig massehentning af data - men i første omgang kan ws kaldes lån for lån. 
///
///                Test url i browser med ApiKey - Husk den virker kun gennem Panteos VPN Tunnel
///                https://dkpanteosapp01.hosted.trifork.com/Panteos.GI.Test/WebApi/api/GI/Loans?api_key=c969f8a8-b6ed-4301-b855-a14a019c7a81 
///
///     Namespace: dk.gi.crm.laan.app.ax2aplaan
///        Author: JMW
///       Created: 2010 Juni 08
///      $Modtime: 23-02-11 11:01 $
///     Copyright: © 2024 Grundejernes Investeringsfond
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2024 05 29 JMW Opdateret NuGet pakker. Rettelse: Reference til Tilstandsvurdering skal slettes ved annonymisering. Felt:ap_tilstandsvurderingid Metode:SletLaanRelateredeData Request:f.eks. ProcessAnonymiseredeRequest
/// 2023 02 06 RCL Updated to NET 4.8, new console, new bll.lib etc. For tst/prod
/// 2022 06 14 JMW Opdateret alle NuGet pakke, ingen build.
/// 2022 05 16 JMW Opdateret Assembly til Panteos
/// 2022 03 17 JMW Opgraderet til CRM 9.0 samt Framework 4.6.2
/// 2021 10 13 JMW Der var lige en fejl med upper/lower case på test(Har så osse lige opdateret console template med ny tilføjelse af CRM organisation)
/// 2021 10 13 JMW Branch blev ikke brugt, den kan overtages til noget andet
/// 2021 10 13 JMW Påbegyndt konvertering til sidst nye console template. Installeret i UDV og Test.
/// 2020 04 09 JMW Rebuild med sidst nye NuGet pakker, den kunne ikke fremsøge lån i CSV liste.
/// 2020 02 23 JMW Fjerne felter som ikke længere skal importeres:
///                Ifølge aftale med Lån fjernes felter fra import fra Panteos
///					- HRL laver en løsning, hvor vi også fjerne felterne fra forms
///					<frsteterminsdato>01-12-2020</frsteterminsdato>
///					<frsteterminsydelse>-338568.75</frsteterminsydelse>
///					<ordinrterminsydelse>26585.95</ordinrterminsydelse>
///					<afdragsfriterminsydelse>1887.69</afdragsfriterminsydelse>
///					<nsteterminsdato>01-03-2021</nsteterminsdato>
///					<nsteterminsydelse>26585.95</nsteterminsydelse>
///					<nsteterminrente>6419.33</nsteterminrente>
///					<nsteterminadmbidrag>0.00</nsteterminadmbidrag>
///					<nsteterminafdrag>20166.62</nsteterminafdrag>
///					Samt felterne
///					<skyldigpaakravsgebyr>0.00</skyldigpaakravsgebyr>
///					<skyldigmorarente>0.00</skyldigmorarente>
/// 2020 12 03 JMW Rettelse til når der ikke kommer noget retur fra panteos
/// 2020 11 24 JMW Rebuild med ny forretning, hvor der ikke længere sammenlignes på beløb hvis der ikke findes data i Panteos
/// 2020 10 27 JMW Rettet Data, der var et problem ved sletning af forbindelser. Build af data og lån forretning, samt build af denne. 
/// 2020 10 27 JMW Rettet Der kommer en exception i både udvikling, test og produktion.
/// 2020 10 20 JMW Rettet: Rettelse til det at sætte et lån til kasseret via liste. Der kan være flere status koder som er valide før status.
/// 2020 05 19 JMW Rettet: Der var en lille fejl i data. Rebuild data + forretning samt denne.
/// 2020 05 15 JMW Rettet: Lagt check ind på lån, så der kun opdateres når der faktisk er rettet noget.
/// 2020 04 27 JMW Rettet gi data, slet kontonr ved status skifte til kasseres. Nye versioner af gi.data og laan.forretning (Samt nyt namespace dk.gi og ikke længere dk.faelles)
/// 20190116   JMW        Rettelse der er behov for at vi i år og de næste par år sætter status på lån til "Kasseret" for de lån som er angivet i en liste (csv fil).
/// 20190918   JMW        Stadig fejl. Panteos leverer dato som 01-01-0001, den kan IKKE importeres i CRM fordi det er uden for CRM's valide dato'r
/// 20190809   ROB	      Opdateret assembly forretning og servicelink pga. rettelse af Panteos "Indfrielse" ap_status opdatering af CRM
/// 20190729   JMW        Rettet så der kan vælges om der forbindes til AX eller til Panteos.
/// 20190522   JMW        Flyttet kode til dk.gi.crm.data fra forretningslag. Samlet forskellige funktioner og tilpasset - rebuild og forbindelse til nye NuGet pakker. 
/// 20180221   JMW        Rettet fejl i AX ServiceLink så denne skal buildes med ny reference til lån og ServiceLink
/// 20180205   JMW        Slettet filer som var None i Build. Opdateret referencer til Forretning for at få ny AX ServiceLink ind.
/// 20170611   JMW        Flyttet alle request til forretning assembly. Dermed bliver kode filer i denne sat til "None" i build.
/// 20170608   JMW        Smårettelser, samt implementering af Serilog
/// 2017 05 16 JMW  1.1.0 Rettet Når status sættes til kasseret – så skal de tre felter i ”om ejendommen” ryddes for indhold!
/// 2017 05 09 JMW  1.1.0 Rettet så den kan kører på https også. Connection streng lagt i app.config
/// 2016 07 04 JMW  1.1.0 Konverteret til CRM 2015 
/// 2016 07 04 JMW  1.0.9 Rettet fejl, den opdaterer næsten aldrig. kun på dem som
/// 2015 01 21 JMW  1.0.8 Tilføjet UK og DK Culture, til konvertering af beløb og datoer. (Det var før implicit, men server miljøer er åbenbart ikke 100% ens)
/// 2015 01 16 JMW  1.0.8 Rettet beløbsfelter som havde et problem.
/// 2014 12 02 JMW  1.0.8 Tilpasset til CRM 2011
/// 2014 05 19 JMW  1.0.7 Uanset hvilken status GI lånet har, skal alle indbetalinger straks registreres på lånet i CRM. Det gælder alle status bl.a. Udbetalt, Holdt i hånd, inkasso, tvangsauktion og konkurs.
///                       Sag 1102843 var registreret som konkurs. Kunden indbetaler terminsydelsen pr. 1-4-2014. Det blev ikke registret i CRM. Konkursen blev ophævet 5-5-2014 og dagen efter blev indbetalingen registreret i CRM
/// 2011 02 03 JMW  1.0.5 JMW Fandt fejl i log "'AP_laan' entity doesn't contain attribute with Name = 'ap_datoformakulering'." Rettet fejl
/// 2011 02 03 JMW  1.0.4 JMW LETET-36 Status sættes til kasseret. 
///							  Dato for makulering sættes til kørselsdato. 
///							  Relation til primær kontaktperson slettes. 
///							  Alle debitorer lån slettes. 
///							  Alle poster på lånets oversigt (journal)slettes 
/// 2010 11 19 JMW  1.0.3 JMW Mindre rettelse månedlig ydelse på prioritet var forkert felt.
///                           samt hvis lånet har en udbetalingsdato og rentesats=0, så skal rentesatsen hentes fra den gældende låneberegning.
/// 2010 10 29 JMW  1.0.2 JMW DRLAAN-105 Prioriteter blev opdateret med forkeret data fordi de relevante felter i laan
///                           entitet blev fjernet før opdatering.
///                           Rettelse: kopi af data før felter slettes, samt test på hvilke felter i prioritet der skal opdateres, entitet opdateres kun hvis der er felter der skal opdateres 
/// 2010 04 12 JMW  1.0.1 JMW Oprettet Ifølge ITOS DRLAAN-80
///
/// </remarks>
-->
