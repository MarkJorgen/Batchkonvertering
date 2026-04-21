# 0019 – Remove Gi.Batch.LegacyShim project

## Decision
`Gi.Batch.LegacyShim` is removed from this pilot delivery.

## Why
The pilot no longer needs a separate legacy shim project to provide failure notification abstractions. Keeping the project would add structural noise without proven cross-job value.

## Consequence
Small transition seams remain local in the job project until the same pattern is proven on additional jobs. Only then should they move to `Gi.Batch.Shared`.
