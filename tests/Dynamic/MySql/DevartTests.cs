#if !DISABLE_DEVART
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Mighty.Dynamic.Tests.MySql.TableClasses;
using NUnit.Framework;

namespace Mighty.Dynamic.Tests.MySql
{
    [TestFixture]
    public class DevartTests
    {
        private readonly string ProviderName = "Devart.Data.MySql";

        // Massive style calls to some examples from https://www.devart.com/dotconnect/mysql/docs/Parameters.html#inoutparams
        #region Devart Examples

        /// <remarks>
        /// Demonstrates that this Devart-specific syntax is possible in Massive;
        /// although it pretty much stops looking much like Massive when used like this,
        /// since you have to do so much manually.
        /// </remarks>
        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Devart_ParameterCheck(bool explicitConnection)
        {
            var db = new SPTestsDatabase(ProviderName, explicitConnection);
            if (explicitConnection)
            {
                MightyTests.ConnectionStringUtils.CheckConnectionStringRequiredForOpenConnection(db);
            }
            dynamic result;
            using (var connection = db.OpenConnection(
                explicitConnection ?
                    MightyTests.ConnectionStringUtils.GetConnectionString(TestConstants.ReadTestConnection, ProviderName) :
                    null
                    ))
            {
                using (var command = db.CreateCommandWithParams("testproc_in_out", isProcedure: true, connection: connection))
                {
                    // uses a dynamic cast to set a provider-specific property without explicitly depending on the provider library
                    ((dynamic)command).ParameterCheck = true;
                    // Devart-specific: makes a round-trip to the database to fetch the parameter names
                    command.Prepare();
                    command.Parameters["param1"].Value = 10;
                    db.Execute(command, connection: connection);
                    result = db.ResultsAsExpando(command);
                }
            }
            Assert.AreEqual(20, result.param2);
        }
        #endregion
    }
}
#endif
