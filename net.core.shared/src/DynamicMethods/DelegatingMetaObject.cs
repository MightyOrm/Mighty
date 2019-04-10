#if DYNAMIC_METHODS
using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace Mighty
{
	public class DelegatingMetaObject : DynamicMetaObject
	{
		private readonly DynamicMetaObject innerMetaObject;

		/// <summary>
		/// Create delegating meta-object (methods and properties which can't be handled by the outer object are handled by the inner object)
		/// </summary>
		/// <param name="parentObject">Your parent object, a (fake) <see cref="IDynamicMetaObjectProvider"/> which will delegate methods and properties which it can't handle to the inner object</param>
		/// <param name="innerObject">The inner object, which can be an <see cref="IDynamicMetaObjectProvider"/> (including, but not limited to <see cref="DynamicObject"/>, or just a plain object</param>
		/// <param name="parameter">Pass on from call to <see cref="GetMetaObject(Expression)"/></param>
		/// <remarks>
		/// `base` call creates a <see cref="DynamicMetaObject"/> for the non-dynamic parent.
		/// This one is using Expression.Constant and it ***does not*** quite work - if you return to a call site with a different instance of the outer
		/// object it still makes the same call (to the inner object first encountered).
		/// </remarks>
		public DelegatingMetaObject(IDynamicMetaObjectProvider parentObject, object innerObject, Expression parameter)
			: base(parameter, BindingRestrictions.Empty, parentObject)
		{
			var innerDynamicProvider = innerObject as IDynamicMetaObjectProvider;
			if (innerDynamicProvider != null)
			{
				// get invocation info for a dynamic inner object (e.g. DynamicObject)
				innerMetaObject = innerDynamicProvider.GetMetaObject(Expression.Constant(innerDynamicProvider));
			}
			else
			{
				// creates invocation info a non-dynamic inner object
				innerMetaObject = new DynamicMetaObject(Expression.Constant(innerObject), BindingRestrictions.Empty, innerObject);
			}
		}

		public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
		{
			var retval = innerMetaObject.BindInvokeMember(binder, args);

			// call any parent object non-dynamic methods before trying wrapped object
			var newretval = binder.FallbackInvokeMember(this, args, retval);

			return newretval;
		}

		public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
		{
			var retval = innerMetaObject.BindSetMember(binder, value);

			// set any parent object non-dynamic member before trying wrapped object
			var newretval = binder.FallbackSetMember(this, value, retval);

			return newretval;
		}

		public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
		{
			var retval = innerMetaObject.BindGetMember(binder);

			// get from any parent object non-dynamic member before trying wrapped object
			var newretval = binder.FallbackGetMember(this, retval);

			return newretval;
		}
	}
}
#endif