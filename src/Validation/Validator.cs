using System;
using System.Collections.Generic;

namespace Mighty.Validation
{
    /// <summary>
    /// Implement this abstract class and pass an instance of it to the constructor of <see cref="MightyOrm"/> to provide validation for your table items.
    /// Note that because almost any type of item can be passed into most Mighty commands, this validator is NOT strongly typed.
    /// If you know you care only going to pass items of type T, you can just add a cast to your validation methods.
    /// </summary>
    /// <remarks>
    /// The objects passed to the validation callbacks are whatever was passed in to Save, Insert, Delete, etc., which can always be any reasonable object
    /// or collection to contain the data needed for the task at hand, even when used with the generically typed version of <see cref="MightyOrm{T}"/>.
    /// I have tried implementing this using functions instead, like <see cref="Mighty.Mapping.SqlNamingMapper"/>, but it really doesn't work well at all,
    /// it is fiddly to use and set up, and the functions you send in can't (as far as I can make out?) have any XML comments explaining their arguments either.
    /// </remarks>
    abstract public class Validator
    {
        /// <summary>
        /// Determine whether and how to pre-validate lists of items before performing any action.
        /// Default is no pre-validation. Other options are to stop after the first item which gives any error,
        /// or to continue and collect all errors before stopping.
        /// </summary>
        /// <returns></returns>
        virtual public Prevalidation PrevalidationType { get; set; } = Prevalidation.Off;

        /// <summary>
        /// If prevalidation is enabled Mighty calls this one item at a time before any database actions are done;
        /// if any item fails, no actions are done for any item.
        /// This also called by the <see cref="MightyOrm"/>.IsValid method.
        /// The default implementation of this method directly calls the `Validate` method and so ignores the <paramref name="action"/> parameter,
        /// but your own override can change this.
        /// </summary>
        /// <param name="action">You can choose to ignore this and do the same validation for every action.</param>
        /// <param name="item">
        /// The item to validate. NB this can be whatever you pass in as input objects, and therefore is NOT restricted to items of the generic type
        /// even for generically typed <see cref="MightyOrm{T}"/>.
        /// Not necessarily a representation of the item for action: e.g. for delete only, it might be a representation of just the PK depending on how `Delete` was called.
        /// Despite this, you can write fairly stright-forward validators; have a look at the table classes in the Mighty docs examples.
        /// </param>
        /// <param name="reportError">
        /// Your code should call this function, e.g. <paramref name="reportError"/>("My error text") to add errors to the error list.
        /// You may choose to add strings, or a more complex object if you wish.
        /// NB Adding one or more errors indicates that the item fails, adding no errors indicates success.
        /// </param>
        /// <returns></returns>
        virtual public void ValidateForAction(OrmAction action, dynamic item, Action<object> reportError) { Validate(item, reportError); }

        /// <summary>
        /// Validate an object, for all action types.
        /// This is called directly by the default implementation of `ValidateForAction`, but won't automatically be called if you override that and change it.
        /// `ValidateForAction` in turn is called by <see cref="MightyOrm{T}"/>.IsValid, so IsValid will also call this unless you have overridden `ValidateForAction`.
        /// </summary>
        /// <param name="item">
        /// The item to validate. NB this can be whatever you pass in as input objects, it is not restricted to items of type T
        /// even for the generically typed <see cref="MightyOrm{T}"/>.
        /// Not necessarily a representation of the item for action: e.g. for delete only, it might be a representation of just the PK depending on how `Delete` was called.
        /// Despite this, you can write fairly stright-forward validators; have a look at the table classes in the Mighty docs examples.
        /// </param>
        /// <param name="reportError">
        /// Your code should call this function, e.g. <paramref name="reportError"/>("My error text") to add errors to the error list.
        /// You may choose to add strings, or a more complex object if you wish.
        /// NB Adding one or more errors indicates that the item fails, adding no errors indicates success.
        /// </param>
        /// <returns></returns>
        abstract public void Validate(dynamic item, Action<object> reportError);

        /// <summary>
        /// This is called one item at time, just before the processing for that specific item.
        /// <see cref="OrmAction"/> is performed iff this returns true. If false is returned, no processing
        /// is done for this item, but processing still continues for all remaining items.
        /// </summary>
        /// <param name="item">The item for which the action is about to be performed.
        /// The type of this is NOT normalised, and depends on what you pass in.</param>
        /// <param name="action">The ORM action</param>
        /// <returns></returns>
        virtual public bool ShouldPerformAction(dynamic item, OrmAction action) { return true; }

        /// <summary>
        /// This is called one item at time, after processing for that specific item.
        /// </summary>
        /// <param name="item">The item for which the action has just been performed.
        /// The type of this is NOT normalised, and depends on what you pass in.</param>
        /// <param name="action">The ORM action</param>
        /// <returns></returns>
        virtual public void HasPerformedAction(dynamic item, OrmAction action) { }
    }
}