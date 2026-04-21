using System;
using System.Collections.Generic;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using dk.gi.app.konto.kontoejerLuk.Application.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.kontoejerLuk.Tests.Unit
{
    [TestClass]
    public class KontoejerLukPlannerTests
    {
        [TestMethod]
        public void PlanClosures_ReturnsClosureForOpenOwners_WhenLastAccountingDateExists()
        {
            var planner = new KontoejerLukPlanner();
            var account = new DeletedAccountRecord(Guid.NewGuid(), "41-00001", new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc));
            var owners = new List<AccountOwnerRecord> { new AccountOwnerRecord(Guid.NewGuid(), null) };

            var result = planner.PlanClosures(account, owners);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void PlanClosures_ReturnsEmpty_WhenLastAccountingDateMissing()
        {
            var planner = new KontoejerLukPlanner();
            var account = new DeletedAccountRecord(Guid.NewGuid(), "41-00001", null);
            var owners = new List<AccountOwnerRecord> { new AccountOwnerRecord(Guid.NewGuid(), null) };

            var result = planner.PlanClosures(account, owners);

            Assert.AreEqual(0, result.Count);
        }
    }
}
