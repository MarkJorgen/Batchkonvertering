using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Contracts;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.beregnsatserlog.slet.Tests.Unit.Application.Services
{
    [TestClass]
    public class SletBeregnSatserLogOrchestratorTests
    {
        [TestMethod]
        public async Task ExecuteAsync_VerifyCrm_OnlyVerifiesConnectivity()
        {
            var repository = new FakeRepository(Array.Empty<BeregnSatserLogRecord>());
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new SletBeregnSatserLogOrchestrator(repository, verifier, NullLogger<SletBeregnSatserLogOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new SletBeregnSatserLogSettings { Mode = JobExecutionMode.VerifyCrm, AntalAar = 3 });

            Assert.IsTrue(report.ConnectivityVerified);
            Assert.AreEqual(0, report.CandidateCount);
            Assert.AreEqual(0, report.DeletedCount);
            Assert.AreEqual(0, repository.DeleteCallCount);
            Assert.AreEqual(1, verifier.VerifyCallCount);
        }

        [TestMethod]
        public async Task ExecuteAsync_DryRun_ReturnsCandidateCount_WithoutDeleting()
        {
            var repository = new FakeRepository(new[]
            {
                new BeregnSatserLogRecord { Id = Guid.NewGuid() },
                new BeregnSatserLogRecord { Id = Guid.NewGuid() },
            });
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new SletBeregnSatserLogOrchestrator(repository, verifier, NullLogger<SletBeregnSatserLogOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new SletBeregnSatserLogSettings { Mode = JobExecutionMode.DryRun, AntalAar = 3 });

            Assert.AreEqual(2, report.CandidateCount);
            Assert.AreEqual(0, report.DeletedCount);
            Assert.AreEqual(0, repository.DeleteCallCount);
            Assert.AreEqual(1, verifier.VerifyCallCount);
        }

        [TestMethod]
        public async Task ExecuteAsync_Run_DeletesAllCandidates()
        {
            var repository = new FakeRepository(new[]
            {
                new BeregnSatserLogRecord { Id = Guid.NewGuid() },
                new BeregnSatserLogRecord { Id = Guid.NewGuid() },
                new BeregnSatserLogRecord { Id = Guid.NewGuid() },
            });
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new SletBeregnSatserLogOrchestrator(repository, verifier, NullLogger<SletBeregnSatserLogOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new SletBeregnSatserLogSettings { Mode = JobExecutionMode.Run, AntalAar = 3 });

            Assert.AreEqual(3, report.CandidateCount);
            Assert.AreEqual(3, report.DeletedCount);
            Assert.AreEqual(3, repository.DeleteCallCount);
        }

        private sealed class FakeRepository : IBeregnSatserLogRepository
        {
            private readonly IReadOnlyCollection<BeregnSatserLogRecord> _candidates;

            public FakeRepository(IReadOnlyCollection<BeregnSatserLogRecord> candidates)
            {
                _candidates = candidates;
            }

            public int DeleteCallCount { get; private set; }

            public Task<IReadOnlyCollection<BeregnSatserLogRecord>> GetCandidatesAsync(int olderThanYears, CancellationToken cancellationToken)
            {
                return Task.FromResult(_candidates);
            }

            public Task DeleteAsync(BeregnSatserLogRecord candidate, CancellationToken cancellationToken)
            {
                DeleteCallCount += 1;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeConnectivityVerifier : IConnectivityVerifier
        {
            public int VerifyCallCount { get; private set; }

            public Task VerifyAsync(CancellationToken cancellationToken)
            {
                VerifyCallCount += 1;
                return Task.CompletedTask;
            }
        }
    }
}
