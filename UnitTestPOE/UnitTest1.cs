using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProgPOEP1.Models;

namespace ProgPOEP1.Tests
{
    [TestClass]
    public class ClaimTests
    {
        [TestMethod]
        public void TotalAmount_CalculatesCorrectly()
        {
            var claim = new Claim { HoursWorked = 10, HourlyRate = 200 };
            Assert.AreEqual(2000, claim.TotalAmount);
        }

        [TestMethod]
        public void Status_ChangesToApproved()
        {
            var claim = new Claim { Status = "Pending" };
            claim.Status = "Approved";
            Assert.AreEqual("Approved", claim.Status);
        }

        [TestMethod]
        public void Status_ChangesToRejected()
        {
            var claim = new Claim { Status = "Pending" };
            claim.Status = "Rejected";
            Assert.AreEqual("Rejected", claim.Status);
        }

        [TestMethod]
        public void Status_ChangesToVerified()
        {
            var claim = new Claim { Status = "Pending" };
            claim.Status = "Verified";
            Assert.AreEqual("Verified", claim.Status);
        }

        [TestMethod]
        public void DocumentPath_IsNotNullOrEmpty()
        {
            var claim = new Claim { DocumentPath = "docs/claim.pdf" };
            Assert.IsFalse(string.IsNullOrEmpty(claim.DocumentPath));
        }
    }
}
