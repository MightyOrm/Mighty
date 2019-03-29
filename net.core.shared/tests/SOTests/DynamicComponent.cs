using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MightyTests.SOTests
{
	public class DynamicComponent : DynamicObject
	{
		private static int globalId;
		private int myId;
		public override bool TryInvokeMember(
			InvokeMemberBinder binder,
			object[] args,
			out object result)
		{
			if (myId == 0) myId = ++globalId;
			result = "This is DynamicComponent #" + myId;
			return true;
		}
	}

	// SO answer - http://stackoverflow.com/a/17634595/795690
	public class AStaticComponent : IDynamicMetaObjectProvider
	{
		private static int globalId;
		private int myId;
		public string Bar()
		{
			if (myId == 0) myId = ++globalId;
			return "This is AStaticComponent #" + myId;
		}

		IDynamicMetaObjectProvider component = new DynamicComponent();

#if true
		// A cut-down version of the working version in SqlProfiler; to give (almost, see comments about trying parent object first) like-for-like replacement of the SO answer
		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			// Test the cut down working version
			//return new DelegatingMetaObject(parameter, this, nameof(component));

			// Test the (broken, since derived from SO answer) Mighty version
			// (This does what I expect, calls to the parent object follow the object, but calls to the child object get wrongly cached to the call the first child object encountered)
			return new Mighty.DelegatingMetaObject(this, component, parameter);
		}

		private class DelegatingMetaObject : DynamicMetaObject
		{
			private readonly DynamicMetaObject innerMetaObject;

			public DelegatingMetaObject(Expression expression, object outerObject, string innerFieldName)
				: base(expression, BindingRestrictions.Empty, outerObject)
			{
				FieldInfo innerField = outerObject.GetType().GetField(innerFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
				var innerObject = innerField.GetValue(outerObject);
				var innerDynamicProvider = innerObject as IDynamicMetaObjectProvider;
				innerMetaObject = innerDynamicProvider.GetMetaObject(Expression.Field(Expression.Convert(Expression, LimitType), innerField));
			}

			public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
			{
				// Whether or not to try the parent object first...
				return binder.FallbackInvokeMember(this, args, innerMetaObject.BindInvokeMember(binder, args));
				//return innerMetaObject.BindInvokeMember(binder, args);
			}
		}
#else
		// The original SO answer
		public DynamicMetaObject GetMetaObject(Expression parameter)
		{
			return new DelegatingMetaObject(component, parameter, BindingRestrictions.GetTypeRestriction(parameter, this.GetType()), this);
		}

		private class DelegatingMetaObject : DynamicMetaObject
		{
			private readonly IDynamicMetaObjectProvider innerProvider;

			public DelegatingMetaObject(IDynamicMetaObjectProvider innerProvider, Expression expr, BindingRestrictions restrictions)
				: base(expr, restrictions)
			{
				this.innerProvider = innerProvider;
			}

			public DelegatingMetaObject(IDynamicMetaObjectProvider innerProvider, Expression expr, BindingRestrictions restrictions, object value)
				: base(expr, restrictions, value)
			{
				this.innerProvider = innerProvider;
			}

			public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
			{
				// This one never tries to call the parent object first...
				var innerMetaObject = innerProvider.GetMetaObject(Expression.Constant(innerProvider));
				return innerMetaObject.BindInvokeMember(binder, args);
			}
		}
#endif
	}
}
