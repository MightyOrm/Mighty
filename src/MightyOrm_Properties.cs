using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using Mighty.DataContracts;
using Mighty.Interfaces;
using Mighty.Keys;
using Mighty.Mapping;
using Mighty.Plugins;
using Mighty.Profiling;
using Mighty.Validation;

// <summary>
// `MightyOrm_Propeties.cs` holds various properties which used to be in the various interface files.
// </summary>
// <remarks>
// TO DO: And now could probably go into the main `MightyOrm.cs` file.
// </remarks>
namespace Mighty
{
    public partial class MightyOrm<T> : MightyOrmAbstractInterface<T> where T : class, new()
    {
        #region Npgsql cursor dereferencing
        /// <summary>
        /// Should we auto-dereference cursors when using the Npgsql ADO.NET driver? (See Mighty documentation.)
        /// </summary>
        override public bool NpgsqlAutoDereferenceCursors { get; set; }

        /// <summary>
        /// Allows setting a global value for whether to auto-dereference cursors when using the Npgsql ADO.NET driver. (See Mighty documentation.)
        /// </summary>
        static public bool? GlobalNpgsqlAutoDereferenceCursors { get; set; }

        /// <summary>
        /// How many rows at a time should we fetch if auto-dereferencing cursors on the Npgsql ADO.NET driver? (Default value 10,000.) (See Mighty documentation.)
        /// </summary>
        override public int NpgsqlAutoDereferenceFetchSize { get; set; }

        /// <summary>
        /// Allows setting a global value for how many rows at a time to fetch if auto-dereferencing cursors on the Npgsql ADO.NET driver. (Default value 10,000.) (See Mighty documentation.)
        /// </summary>
        static public int? GlobalNpgsqlAutoDereferenceFetchSize { get; set; }
        #endregion

        #region Sql Server auto-join commands to any transaction
        /// <summary>
        /// Should we automatically enlist all commands to any transaction on any connection provided?
        /// SQL Server does not do this automatically even though other ADO.NET providers do. (Default value true.)
        /// </summary>
        override public bool SqlServerAutoEnlistCommandsToTransactions { get; set; }

        /// <summary>
        /// Allows setting a global value for whether we should automatically enlist all commands to any transaction on any connection provided?
        /// SQL Server does not do this automatically even though other ADO.NET providers do. (Default value true.)
        /// </summary>
        static public bool? GlobalSqlServerAutoEnlistCommandsToTransactions { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Connection string
        /// </summary>
        override public string ConnectionString { get; protected set; }

        /// <summary>
        /// ADO.NET provider factory
        /// </summary>
        protected internal DbProviderFactory Factory { get; protected set; }

        /// <summary>
        /// Plugin
        /// </summary>
        internal PluginBase Plugin { get; set; }

        /// <summary>
        /// Allows setting a global connection string (used by default if nothing else set; set it on untyped <see cref="MightyOrm"/> to set it everywhere).
        /// </summary>
        static public string GlobalConnectionString { get; set; }

        /// <summary>
        /// Allows setting a global provider name (used by default if nothing else set; set it on untyped <see cref="MightyOrm"/> to set it everywhere).
        /// </summary>
        static public string GlobalProviderName { get; set; }

        /// <summary>
        /// Allows setting a global validator (used by default if nothing else set; set it on untyped <see cref="MightyOrm"/> to set it everywhere).
        /// </summary>
        static public Validator GlobalValidator { get; set; }

        /// <summary>
        /// Validator
        /// </summary>
        override public Validator Validator { get; protected set; }

        /// <summary>
        /// Allows setting a global sql mapper (used by default if nothing else set; set it on untyped <see cref="MightyOrm"/> to set it everywhere).
        /// </summary>
        static public SqlNamingMapper GlobalSqlNamingMapper { get; set; }

        /// <summary>
        /// C# &lt;=&gt; SQL mapper
        /// </summary>
        override public SqlNamingMapper SqlNamingMapper { get; protected set; }

        /// <summary>
        /// Allows setting a global SQL profiler (used by default if nothing else set; set it on untyped <see cref="MightyOrm"/> to set it everywhere).
        /// </summary>
        static public DataProfiler GlobalDataProfiler { get; set; }

        /// <summary>
        /// Optional SQL profiler
        /// </summary>
        override public DataProfiler DataProfiler { get; protected set; }

        /// <summary>
        /// Table name (null if non-table-specific instance)
        /// </summary>
        override public string TableName { get; protected set; }

        /// <summary>
        /// Table owner/schema (null if not specified)
        /// </summary>
        override public string TableOwner { get; protected set; }

        /// <summary>
        /// Bare table name (without owner/schema part)
        /// </summary>
        override public string BareTableName { get; protected set; }

        /// <summary>
        /// Keys and sequence
        /// </summary>
        override public PrimaryKeyInfo PrimaryKeyInfo { get; protected set; }

#if KEY_VALUES
        /// <summary>
        /// Column from which value is retrieved by <see cref="KeyValues"/>
        /// </summary>
        override public string ValueColumn { get; protected set; }
#endif

        /// <summary>
        /// A data contract for the current item type, specified columns and case-sensitivity
        /// </summary>
        override public DataContract DataContract { get; protected set; }

        /// <summary>
        /// The default set of columns to use for queries
        /// </summary>
        override public string DefaultColumns { get; protected set; }

        /// <summary>
        /// true for generic instantiation; false if dynamically typed instantiation
        /// </summary>
        override public bool IsGeneric { get; protected set; }
#endregion
    }
}
