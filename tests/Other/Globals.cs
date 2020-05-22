using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mighty.Generic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Globals
{
    [TestFixture]
    public class Globals
    {
        // TO DO: Should really add some tests for the defaults priorities of everything else,
        // but they are all done exactly the same way.
        [Test]
        public void NpgsqlSettings_DefaultsPriorities()
        {
            var db1 = new MightyOrm();
            var db2 = new MightyOrm<Film>();
            var db3 = new MightyOrm<Category>();

            MightyOrm.GlobalNpgsqlAutoDereferenceFetchSize = 40000;
            MightyOrm<Film>.GlobalNpgsqlAutoDereferenceFetchSize = 30000;
            var db4 = new MightyOrm();
            var db5 = new MightyOrm<Film>();
            var db6 = new MightyOrm<Category>();

            MightyOrm.GlobalNpgsqlAutoDereferenceFetchSize = null;
            var db7 = new MightyOrm<Film>();
            var db8 = new MightyOrm<Category>();

            MightyOrm<Film>.GlobalNpgsqlAutoDereferenceFetchSize = null;

            Assert.AreEqual(10000, db1.NpgsqlAutoDereferenceFetchSize);
            Assert.AreEqual(10000, db2.NpgsqlAutoDereferenceFetchSize);
            Assert.AreEqual(10000, db3.NpgsqlAutoDereferenceFetchSize);

            Assert.AreEqual(40000, db4.NpgsqlAutoDereferenceFetchSize);
            Assert.AreEqual(30000, db5.NpgsqlAutoDereferenceFetchSize);
            Assert.AreEqual(40000, db6.NpgsqlAutoDereferenceFetchSize);

            Assert.AreEqual(30000, db7.NpgsqlAutoDereferenceFetchSize);
            Assert.AreEqual(10000, db8.NpgsqlAutoDereferenceFetchSize);
        }
    }
}
