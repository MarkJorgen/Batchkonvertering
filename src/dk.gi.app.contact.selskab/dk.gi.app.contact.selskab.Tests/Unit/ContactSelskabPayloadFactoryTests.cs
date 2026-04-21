using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Infrastructure.Messaging;

namespace dk.gi.app.contact.selskab.Tests.Unit
{
    [TestClass]
    public class ContactSelskabPayloadFactoryTests
    {
        [TestMethod]
        public void Create_Builds_Expected_Action_And_ContactId()
        {
            var candidate = new ContactSelskabCandidate(Guid.Parse("11111111-2222-3333-4444-555555555555"), "12345678", 2, 0);

            string payload = ContactSelskabPayloadFactory.Create(candidate);

            StringAssert.Contains(payload, "\"Key\":\"Mode\",\"Value\":\"Contact\"");
            StringAssert.Contains(payload, "\"Key\":\"action\",\"Value\":\"opdaterkdk\"");
            StringAssert.Contains(payload, "11111111-2222-3333-4444-555555555555".ToUpperInvariant());
        }
    }
}
