using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using KrdLab.Lisys;
using KrdLab.Lisys.Testing;

namespace LisysTest
{
    [TestFixture]
    public class TestingTest
    {
        [Test]
        public void Test01()
        {
            double delta = 1e-3;

            IVector set1 = new Vector(12.2, 18.8, 18.2);
            IVector set2 = new Vector(26.4, 32.6, 31.3);

            double p = 1;
            double t = 0;

            // “™•ªU«‚ğ‰¼’è
            Assert.IsTrue(T.Test(set1, set2, Method.AssumedEqualityOfVariances, 0.025, out p, out t));
            Assert.AreEqual(t, 4.842, delta);

            // Welch‚ÌŒŸ’èi“™•ªU«‚ğ‰¼’è‚µ‚È‚¢j
            Assert.IsTrue(T.Test(set1, set2, Method.NotAssumedEqualityOfVariances, 0.025, out p, out t));
            Assert.AreEqual(t, 4.842, delta);
        }

        //[Test]
        //public void Test02()
        //{
        //    // ‘Î‰‚Ì‚ ‚éê‡
        //    double delta = 1e-3;

        //    // ¦–â‘è‚ ‚è
        //    IVector set1 = new Vector(269, 230, 365, 282, 295, 212, 346, 207, 308, 257);
        //    IVector set2 = new Vector(273, 213, 383, 282, 297, 213, 351, 208, 294, 238);

        //    double p = 1;
        //    double t = 0;

        //    Assert.IsFalse(T.Test(set1, set2, Method.PairedValues, 0.05, out p, out t));
        //    Assert.AreEqual(0.5245275, t, delta);
        //}
    }
}
