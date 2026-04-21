using System.Collections.Generic;
using dk.gi.app.ejendom.tjekejerskifte.Application.Models;
namespace dk.gi.app.ejendom.tjekejerskifte.Application.Abstractions
{ public interface IEjendomTjekEjerskifteJobPublisher { EjendomTjekEjerskiftePublishResult Publish(IReadOnlyCollection<EjendomTjekEjerskifteCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings); } }
