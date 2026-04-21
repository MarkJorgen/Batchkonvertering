/// <remarks>
///     $Workfile: ReadMe.txt
///   Description: Dette er ReadMe.txt filen som beskriver dk.gi projektet.
///
///                Projektet her indeholder abstrakte klasser som den øvrige del af GI's kode bygger på. Samt enkelte implementeringsklasser.
///                Disse kan ikke længere bruges til Plugin's pga bindinger til Microsoft NuGet pakker
///
///                Context
///						IContext........................:  Basis interface for en kontext klasse, - som indeholder en Trace. Alle GI's kontext klasser bygger derfor videre på denne brug af Trace/Sporing.
///
///                datacontract
///                     JsonKeyValueList................: Hjælpe klasse til brug for MSMQ og Azure Service Bus Queue
///          			SerializerWrapper...............: Serilicer til JSON
///				   extending
///					    Extending_DateTime..............: Ekstra metoder til DateTime klassen
///					    Extending_Decimal...............: Ekstra metoder til Decimal klassen
///					    Extending_String................: Ekstra metoder til String klassen
///                request
///						IRequest........................: Basis interface for et standard GI Request, - som indeholder ExecuteRequest som når den implementeres i en nedarvende klasse foretager den egentlige eksekvering af koden.
///                     IRequestValidationAttribute.....: Selve validering af request parametrene.
///                     GIRequestTraceAttribute.........: Makering af at en property skal behandles som en request attribut. Dvs. den valideres.
///                response
///                     IResponseState..................: Basis State implementering på et IResponse objekt.
///						IResponse.......................: Basis interface for et resultatset som dannes af et request. 
///                     GIResponseState.................: Sikre at IResponseState implementeres på GI Response         
///				   trace
///                     GILoggerProvider................: GI Logger baseret på Microsoft.Extensions.Logging
///
///                Her følger implementeringer
///
///                ax        Funktioner til kommunikation med Axapta økonomi
///                bc        Funktioner til kommunikation med Business Central økonomi
///                asbq      Azure Service Bus Queue: Send en Message til en Queue
///                email     Brug af Graph for kunne sende mail
///                msmq      Microsoft Message Queue (MSMQ) indsæt/flyt med mere.
///                panteos   Funktioner til kommunikation med Panteos WebServices
///
/// =============================================================================================================================
/// </remarks>
-->