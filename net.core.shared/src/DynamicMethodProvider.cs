using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

using Mighty.Interfaces;

namespace Mighty
{
	/// <summary>
	/// Wrapper to provide dynamic methods (wrapper object on mighty needed as we can't do direct multiple inheritance).
	/// </summary>
	/// <returns></returns>
	/// <remarks>
	/// This is included mainly for easy compatibility with Massive.
	/// Pros: The methods this provides are quite cool...
	/// Cons: They're never visible in IntelliSense; you don't really need them; it turns out this adds overhead to
	/// *every* call to the microORM, even when the object is not stored in a dynamic.
	/// </remarks>
	internal class DynamicMethodProvider<T> : DynamicObject where T : class, new()
	{
		private MightyORM<T> Mighty;

		/// <summary>
		/// Wrap MightyORM to provide Massive-compatible dynamic methods.
		/// You can access almost all this functionality non-dynamically (and if you do, you get IntelliSense, which makes life easier).
		/// </summary>
		/// <param name="me"></param>
		internal DynamicMethodProvider(MightyORM<T> me)
		{
			Mighty = me;
		}

		/// <summary>
		/// Provides the implementation for operations that invoke a member. This method implementation tries to create queries from the methods being invoked based on the name
		/// of the invoked method.
		/// </summary>
		/// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. 
		/// For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, 
		/// binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
		/// <param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is 
		/// derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, <paramref name="args"/>[0] is equal to 100.</param>
		/// <param name="result">The result of the member invocation.</param>
		/// <returns>
		/// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific 
		/// run-time exception is thrown.)
		/// </returns>
		/// <remarks>Massive code (see CREDITS file), with added columns support (which is only possible using named arguments).</remarks>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = null;
			var info = binder.CallInfo;
			if (info.ArgumentNames.Count != args.Length)
			{
				throw new InvalidOperationException("No non-dynamic method found; use named arguments for dynamically invoked queries: this can be a field name followed by a value, 'orderby:', 'colums:', 'where:' or 'args:'");
			}

			var columns = "*";
			var orderBy = Mighty.PrimaryKeyFields;
			var wherePredicates = new List<string>();
			var nameValueArgs = new ExpandoObject();
			var nameValueDictionary = nameValueArgs.ToDictionary();
			object[] userArgs = null;
			if (info.ArgumentNames.Count > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					var name = info.ArgumentNames[i];
					switch (name.ToLowerInvariant())
					{
						case "orderby":
							orderBy = args[i].ToString();
							break;
						case "columns":
							columns = args[i].ToString();
							break;
						case "where":
							// this is an arbitrary SQL WHERE specification, so we have to wrap it in brackets to avoid operator precedence issues
							wherePredicates.Add("(" + args[i].ToString().Unthingify("WHERE") + ")");
							break;
						case "args":
							// wrap anything other than an array in an array (this is what C# params basically does anyway)
							userArgs = args[i] as object[];
							if (userArgs == null)
							{
								userArgs = new object[] { args[i] };
							}
							break;
						default:
							// treat anything else as a name-value pair
							wherePredicates.Add(string.Format("{0} = {1}", name, Mighty.Plugin.PrefixParameterName(name)));
							nameValueDictionary.Add(name, args[i]);
							break;
					}
				}
			}
			var whereClause = string.Empty;
			if (wherePredicates.Count > 0)
			{
				whereClause = " WHERE " + string.Join(" AND ", wherePredicates);
			}

			var op = binder.Name;
			var uOp = op.ToUpperInvariant();
			switch (uOp)
			{
				case "COUNT":
				case "SUM":
				case "MAX":
				case "MIN":
				case "AVG":
					result = Mighty.AggregateWithParams(string.Format("{0}({1})", uOp, columns), whereClause, inParams: nameValueArgs, args: userArgs);
					break;
				default:
					var justOne = uOp.StartsWith("FIRST") || uOp.StartsWith("LAST") || uOp.StartsWith("GET") || uOp.StartsWith("FIND") || uOp.StartsWith("SINGLE");
					// For Last only, sort by DESC on the PK (PK sort is the default)
					if (uOp.StartsWith("LAST"))
					{
						// this will be incorrect if multiple PKs are present, but note that the ORDER BY may be from a dynamic method
						// argument by this point; this could be done correctly for compound PKs, but not in general for user input (it
						// would involve parsing SQL, which we never do)
						orderBy = orderBy + " DESC";
					}
					if (justOne)
					{
						result = Mighty.SingleWithParams(whereClause, orderBy, columns, inParams: nameValueArgs, args: userArgs);
					}
					else
					{
						result = Mighty.AllWithParams(whereClause, orderBy, columns, inParams: nameValueArgs, args: userArgs);
					}
					break;
			}
			return true;
		}
	}

	// Allow dynamic methods on instances of MightyORM, implementing them via a wrapper object.
	// (We can't make MightyORM directly implement DynamicObject, since it inherits from MicroORM and C# doesn't allow multiple inheritance.)
	public partial class MightyORM<T> : IDynamicMetaObjectProvider where T : class, new()
	{
		private DynamicMethodProvider<T> DynamicObjectWrapper;

		/// <summary>
		/// Implements IDynamicMetaObjectProvider.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		/// <remarks>
		/// Inspired by http://stackoverflow.com/a/17634595/795690
		/// </remarks>
		/// <remarks>
		/// Support dynamic methods via a wrapper object (needed as we can't do direct multiple inheritance)
		/// This code is being called all the time (ALL methods calls to <see cref="IDynamicMetaObjectProvider"/> objects go through this, even
		/// when not stored in dynamic). This is the case for all <see cref="DynamicObject"/>s too (e.g. as in Massive) but you don't see it
		/// when debugging in that case, as GetMetaObject() is not user code if you inherit directly from DynamicObject.
		/// </remarks>
		public DynamicMetaObject GetMetaObject(Expression expression)
		{
			return new DelegatingMetaObject(this, DynamicObjectWrapper, expression);
		}
	}
}
