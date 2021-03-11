using System;

namespace Mighty.Generic.Tests.SqlServer.TableClasses
{
    public class EnumTest
    {
        public enum MyByteEnum : byte
        {
            value_3 = 3,
            value_4,
            value_5,
        }
        public enum MyShortEnum : short
        {
            value_6 = 6,
            value_7,
            value_8,
        }
        public enum MyIntEnum
        {
            value_9 = 9,
            value_a,
            value_b,
        }
        [DatabaseColumn("EnumTestID")]
        public int ID;

        public MyByteEnum ByteField;
        public MyShortEnum ShortField;
        public MyIntEnum IntField;
    }

    public class EnumTests : MightyOrm<EnumTest>
    {
        public EnumTests() : this(true)
        {
        }


        public EnumTests(bool includeSchema) :
            base(string.Format(TestConstants.WriteTestConnection, TestConstants.ProviderName), includeSchema ? "dbo.EnumTestTable" : "EnumTestTable", "ID")
        {
        }
    }
}
