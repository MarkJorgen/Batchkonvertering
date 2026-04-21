using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.regnskab.slet.Application.Contracts;
using dk.gi.app.konto.regnskab.slet.Application.Models;
using dk.gi.app.konto.regnskab.slet.Application.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.regnskab.slet.Tests.Unit.Application.Services
{
    [TestClass]
    public class RegnskabSletOrchestratorTests
    {
        [TestMethod]
        public async Task ExecuteAsync_VerifyCrm_OnlyVerifiesConnectivity()
        {
            var repository = new FakeRepository(Array.Empty<KontoCandidate>());
            var publisher = new FakePublisher();
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new RegnskabSletOrchestrator(repository, publisher, verifier, NullLogger<RegnskabSletOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new RegnskabSletSettings { Mode = JobExecutionMode.VerifyCrm, ServiceBusQueueName = "crmpluginjobs", ServiceBusLabel = "KontoDiv" });

            Assert.IsTrue(report.ConnectivityVerified);
            Assert.AreEqual(0, report.SelectedAccountCount);
            Assert.AreEqual(0, report.PublishedCount);
            Assert.AreEqual(0, publisher.PublishCallCount);
            Assert.AreEqual(1, verifier.VerifyCallCount);
        }

        [TestMethod]
        public async Task ExecuteAsync_DryRun_ReturnsSelectedCount_WithoutPublishing()
        {
            var repository = new FakeRepository(new[]
            {
                new KontoCandidate { AccountId = Guid.NewGuid(), AccountNumber = "41-00001" },
                new KontoCandidate { AccountId = Guid.NewGuid(), AccountNumber = "41-00002" },
            });
            var publisher = new FakePublisher();
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new RegnskabSletOrchestrator(repository, publisher, verifier, NullLogger<RegnskabSletOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new RegnskabSletSettings { Mode = JobExecutionMode.DryRun, ServiceBusQueueName = "crmpluginjobs", ServiceBusLabel = "KontoDiv" });

            Assert.AreEqual(2, report.SelectedAccountCount);
            Assert.AreEqual(0, report.PublishedCount);
            Assert.AreEqual(0, publisher.PublishCallCount);
        }

        [TestMethod]
        public async Task ExecuteAsync_Run_PublishesAllSelectedCandidates()
        {
            var repository = new FakeRepository(new[]
            {
                new KontoCandidate { AccountId = Guid.NewGuid(), AccountNumber = "41-00003" },
                new KontoCandidate { AccountId = Guid.NewGuid(), AccountNumber = "41-00004" },
            });
            var publisher = new FakePublisher { PublishResult = 2 };
            var verifier = new FakeConnectivityVerifier();
            var orchestrator = new RegnskabSletOrchestrator(repository, publisher, verifier, NullLogger<RegnskabSletOrchestrator>.Instance);

            var report = await orchestrator.ExecuteAsync(new RegnskabSletSettings { Mode = JobExecutionMode.Run, ServiceBusQueueName = "crmpluginjobs", ServiceBusLabel = "KontoDiv" });

            Assert.AreEqual(2, report.SelectedAccountCount);
            Assert.AreEqual(2, report.PublishedCount);
            Assert.AreEqual(1, publisher.PublishCallCount);
        }

        private sealed class FakeRepository : IRegnskabSletRepository
        {
            private readonly IReadOnlyCollection<KontoCandidate> _candidates;
            public FakeRepository(IReadOnlyCollection<KontoCandidate> candidates) { _candidates = candidates; }
            public Task<IReadOnlyCollection<KontoCandidate>> GetCandidatesAsync(CancellationToken cancellationToken) => Task.FromResult(_candidates);
            public Task<ResolvedServiceBusSettings> ResolveServiceBusSettingsAsync(CancellationToken cancellationToken) => Task.FromResult(ResolvedServiceBusSettings.Empty("none"));
        }

        private sealed class FakePublisher : IRegnskabSletPublisher
        {
            public int PublishCallCount { get; private set; }
            public int PublishResult { get; set; }
            public Task<int> PublishAsync(IReadOnlyCollection<KontoCandidate> candidates, ResolvedServiceBusSettings resolvedServiceBusSettings, CancellationToken cancellationToken)
            {
                PublishCallCount += 1;
                return Task.FromResult(PublishResult == 0 ? candidates.Count : PublishResult);
            }
        }

        private sealed class FakeConnectivityVerifier : IConnectivityVerifier
        {
            public int VerifyCallCount { get; private set; }
            public Task VerifyAsync(CancellationToken cancellationToken) { VerifyCallCount += 1; return Task.CompletedTask; }
        }
    }
}
