using System;

namespace NHibernate.Property
{
	/// <summary>
	/// Implementation of <see cref="IFieldNamingStrategy"/> for fields that are prefixed with
	/// an <c>_</c> and the first character in PropertyName capitalized.
	/// </summary>
	public class PascalCaseUnderscoreStrategy : IFieldNamingStrategy
	{
		#region IFieldNamingStrategy Members

		/// <summary>
		/// Converts the Property's name into a Field name by making the first character 
		/// of the <c>propertyName</c> uppercase and prefixing it with an underscore.
		/// </summary>
		/// <param name="propertyName">The name of the mapped property.</param>
		/// <returns>The name of the Field in PascalCase format prefixed with an underscore.</returns>
		public string GetFieldName( string propertyName )
		{
			return "_" + propertyName.Substring( 0, 1 ).ToUpper( System.Globalization.CultureInfo.InvariantCulture ) + propertyName.Substring( 1 );
		}

		#endregion
	}
}
