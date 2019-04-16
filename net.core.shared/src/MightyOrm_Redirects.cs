using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.Interfaces;
using Mighty.Mapping;
using Mighty.Plugins;
using Mighty.Profiling;
using Mighty.Validation;

// <summary>
// MightyOrm_Redirects.cs holds methods in Mighty than can be very simply defined in terms of other methods.
// </summary>
namespace Mighty
{
    public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
    {
        #region Non-table specific methods
        /// <summary>
        /// Create a <see cref="DbCommand"/> ready for use with Mighty.
        /// Manually creating commands is an advanced use-case; standard Mighty methods create and dispose
        /// of required <see cref="DbCommand"/> and <see cref="DbConnection"/> objects for you.
        /// You should use one of the variants of <see cref="CreateCommand(string, object[])"/>
        /// for all commands passed in to Mighty, since on some providers this sets provider specific properties which are needed to ensure expected behaviour with Mighty.
        /// </summary>
        /// <param name="sql">The command SQL, with optional numbered parameters</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        override public DbCommand CreateCommand(string sql,
            params object[] args)
        {
            return CreateCommandWithParams(sql, args: args);
        }

        /// <summary>
        /// Create a <see cref="DbCommand"/> ready for use with Mighty.
        /// Manually creating commands is an advanced use-case; standard Mighty methods create and dispose
        /// of required <see cref="DbCommand"/> and <see cref="DbConnection"/> objects for you.
        /// You should use one of the variants of <see cref="CreateCommand(string, object[])"/>
        /// for all commands passed in to Mighty, since on some providers this sets provider specific properties which are needed to ensure expected behaviour with Mighty.
        /// </summary>
        /// <param name="sql">The command SQL, with optional numbered parameters</param>
        /// <param name="connection">The connection to use</param>
        /// <param name="args">Auto-numbered parameters for the SQL</param>
        /// <returns></returns>
        override public DbCommand CreateCommand(string sql,
            DbConnection connection,
            params object[] args)
        {
            return CreateCommandWithParams(sql, args: args);
        }
        #endregion

        #region Table specific methods
        /// <summary>
        /// Return a new item populated with defaults values which correctly reflect the defaults of the current database table, when these are present.
        /// </summary>
        /// <returns></returns>
        override public T New()
        {
            return NewFrom();
        }
        #endregion
    }
}
