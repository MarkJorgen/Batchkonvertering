# 0001 - Første platform-slice for GI Batch

## Beslutning

Før brede jobkonverteringer etableres et fælles platform-slice, som består af:

- ny root-solution
- `Gi.Batch.Shared`
- `Gi.Batch.LegacyShim`
- første pilotjob med ny struktur
- første testprojekt
- lokal konfigurationsmodel uden krav om Azure App Configuration

## Begrundelse

Den uploadede kodebase er stærkt ensartet på tværs af standardjobs. Derfor giver et fælles platform-slice større værdi end at konvertere mange jobs individuelt uden fælles målarkitektur.

## Konsekvens

Efter denne leverance kan næste iteration fokusere på at flytte reel workflow-logik fra legacy-reference til ny struktur i pilotjobbet og derefter rulle mønstret ud til næste standardjob.
