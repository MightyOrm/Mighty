using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mighty.DataContracts
{
    /// <summary>
    /// Store reflected field or property, with info about whether Mighty should be reading or writing it.
    /// </summary>
    public class DataContractMemberInfo
    {
        /// <summary>
        /// The data member's parent type
        /// </summary>
        public Type DeclaringType { get; protected set; }

        /// <summary>
        /// The data member's type
        /// </summary>
        public Type MemberType { get { return Field?.FieldType ?? Property?.PropertyType; } }

        /// <summary>
        /// The field if this is wrapping <see cref="FieldInfo"/>
        /// </summary>
        private FieldInfo Field { get; }

        /// <summary>
        /// The property if this is wrapping <see cref="PropertyInfo"/>
        /// </summary>
        private PropertyInfo Property { get; }

        /// <summary>
        /// The data member which this is wrapping (always either <see cref="PropertyInfo"/> or <see cref="FieldInfo"/>)
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        /// Whether Mighty should try to read data from or write data to the database for this column
        /// </summary>
        public DataDirection DataDirection { get; protected set; }

        /// <summary>
        /// The data member name
        /// </summary>
        public string Name { get { return Field?.Name ?? Property?.Name; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="DataMember"></param>
        /// <param name="Type"></param>
        /// <param name="DataDirection"></param>
        public DataContractMemberInfo(Type Type, MemberInfo DataMember, DataDirection DataDirection)
        {
            this.DeclaringType = Type;
            this.DataDirection = DataDirection;

            Member = DataMember;
            Field = DataMember as FieldInfo;
            Property = DataMember as PropertyInfo;
        }

        /// <summary>
        /// Set the value of the reflected field or property in the specified object
        /// </summary>
        /// <param name="obj">The object to write to</param>
        /// <param name="value">The value to write</param>
        public void SetValue(object obj, object value)
        {
            if (DataDirection != 0 && (DataDirection & DataDirection.Write) == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot write to {this}, write disabled by {nameof(DatabaseColumnAttribute)} {nameof(DataDirection)} setting.");
            }

            // The middle section of this method code was intiailly added as an internal ChangeType extension method to fix
            // up the slightly weird way that the runtime refuses to convert from T to T? (which should surely be trivial?);
            // but it also as a side-effect handles:
            //  - Coercing other types (e.g. int to bool)
            // And now also handles:
            //  - Giving a reasonably useful exception message if incompatible types are set on a given field
            //    (e.g. if the field is the wrong type for the data coming back from the DB)
            // See: http://stackoverflow.com/q/18015425/

            Type t = MemberType;
            if (t == null)
            {
                // this should not happen
                throw new Exception($"Expected {DeclaringType.FullName}.{Name} to be a field or a property");
            }

            if (value != null)
            {
                // Force successful conversion from T? to T
                // Also coerces some other types such as int to byte (e.g. database has ((1)) as default value for bit)
                if (t
#if !NETFRAMEWORK
                .GetTypeInfo()
#endif
                .IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    t = Nullable.GetUnderlyingType(t);
                }
                try
                {
                    value = Convert.ChangeType(value, t);
                }
                catch (FormatException)
                {
                    throw new FormatException($"Cannot convert {value.GetType().Name} value for {DeclaringType.Name}.{Name} to {t.Name}");
                }
            }

            Field?.SetValue(obj, value);
            Property?.SetValue(obj, value);
        }

        /// <summary>
        /// Return the value of the reflected field or property in the specified object
        /// </summary>
        /// <param name="obj">The object to read from</param>
        /// <returns></returns>
        public object GetValue(object obj)
        {
            if (DataDirection != 0 && (DataDirection & DataDirection.Read) == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot read from {this}, read disabled by {nameof(DatabaseColumnAttribute)} {nameof(DataDirection)} setting.");
            }
            if (Field != null) return Field.GetValue(obj);
            return Property.GetValue(obj);
        }

        /// <summary>
        /// Create string representation of this type
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{(Field != null ? "field" : "property")} {Name} of {DeclaringType.FullName}";
        }
    }
}
