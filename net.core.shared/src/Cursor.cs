namespace Mighty
{
	public class Cursor
	{
		internal object Value { get; private set; }

		public Cursor(object value = null)
		{
			Value = value;
		}
	}
}