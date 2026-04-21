# 0011 - Isolate GI references into local adapter project

## Beslutning
Pilotens direkte GI package references flyttes ud af exe-projektet og ind i et lokalt adapterprojekt.

## Begrundelse
Det gør pilotens exe-projekt og application/config-spor GI-neutralt, samtidig med at den konkrete ContactBLL-kobling bevares i et smalt lokalt seam.

## Konsekvens
- exe-projektet er fri for direkte GI NuGet-pakker
- GI er stadig til stede i piloten gennem det lokale adapterprojekt
- næste reduktionsblok kan fokusere på selve adapterprojektet eller på at bevise samme mønster i job nr. 2
