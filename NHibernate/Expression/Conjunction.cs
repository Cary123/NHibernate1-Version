namespace NHibernate.Expression
{
	/// <summary>
	/// An <see cref="ICriterion"/> that Junctions together multiple 
	/// <see cref="ICriterion"/>s with an <c>and</c>
	/// </summary>
	public class Conjunction : Junction
	{
		/// <summary>
		/// Get the Sql operator to put between multiple <see cref="ICriterion"/>s.
		/// </summary>
		/// <value>The string "<c> and </c>"</value>
		protected override string Op
		{
			get { return " and "; }
		}
	}
}