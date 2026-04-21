using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.slet.Application.Contracts;
using dk.gi.app.konto.satser.slet.Application.Models;
using dk.gi.app.konto.satser.slet.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.satser.slet.Tests.Unit.Application.Services
{
    [TestClass]
    public class SletSatserOrchestratorTests
    {
        [TestMethod]
        public async Task ExecuteAsync_VerifyCrm_OnlyVerifiesConnectivity()
        {
            var repository = new FakeRepository(Array.Empty<SatserRecord>());
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new SletSatserOrchestrator(repository, verifier, NullLogger<SletSatserOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new SletSatserSettings { Mode = JobExecutionMode.VerifyCrm, SatsAar = 2027 });

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
                new SatserRecord { Id = Guid.NewGuid(), StartdatoLocal = new DateTime(2027, 1, 1) },
                new SatserRecord { Id = Guid.NewGuid(), StartdatoLocal = new DateTime(2027, 2, 1) },
            });
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new SletSatserOrchestrator(repository, verifier, NullLogger<SletSatserOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new SletSatserSettings { Mode = JobExecutionMode.DryRun, SatsAar = 2027 });

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
                new SatserRecord { Id = Guid.NewGuid(), StartdatoLocal = new DateTime(2027, 1, 1) },
                new SatserRecord { Id = Guid.NewGuid(), StartdatoLocal = new DateTime(2027, 3, 1) },
                new SatserRecord { Id = Guid.NewGuid(), StartdatoLocal = new DateTime(2027, 6, 1) },
            });
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new SletSatserOrchestrator(repository, verifier, NullLogger<SletSatserOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new SletSatserSettings { Mode = JobExecutionMode.Run, SatsAar = 2027 });

            Assert.AreEqual(3, report.CandidateCount);
            Assert.AreEqual(3, report.DeletedCount);
            Assert.AreEqual(3, repository.DeleteCallCount);
        }

        private sealed class FakeRepository : ISatserRepository
        {
            private readonly IReadOnlyCollection<SatserRecord> _candidates;

            public FakeRepository(IReadOnlyCollection<SatserRecord> candidates)
            {
                _candidates = candidates;
            }

            public int DeleteCallCount { get; private set; }

            public Task<IReadOnlyCollection<SatserRecord>> GetCandidatesAsync(int satsAar, CancellationToken cancellationToken)
            {
                return Task.FromResult(_candidates);
            }

            public Task DeleteAsync(SatserRecord candidate, CancellationToken cancellationToken)
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
