using System;
using System.Collections;
using System.Text;

namespace NHibernate.Util
{
	/// <summary></summary>
	public sealed class StringHelper
	{
		private StringHelper()
		{
			// not creatable
		}

		/// <summary></summary>
		public const char Dot = '.';
		/// <summary></summary>
		public const char Underscore = '_';
		/// <summary></summary>
		public const string CommaSpace = ", ";
		/// <summary></summary>
		public const string Comma = ",";
		/// <summary></summary>
		public const string OpenParen = "(";
		/// <summary></summary>
		public const string ClosedParen = ")";
		/// <summary></summary>
		public const char SingleQuote = '\'';

		// TODO: Semantically these belong elsewhere as they are NHibernate specific
		/// <summary></summary>
		public const string NamePrefix = ":";
		/// <summary></summary>
		public const string SqlParameter = "?";

		public static string Join( string separator, IEnumerable objects )
		{
			StringBuilder buf = new StringBuilder();
			bool first = true;

			foreach( object obj in objects )
			{
				if( !first )
				{
					buf.Append( separator );
				}

				first = false;
				buf.Append( obj );
			}

			return buf.ToString();
		}

		public static string[ ] Add( string[ ] x, string sep, string[ ] y )
		{
			string[ ] result = new string[ x.Length ];
			for( int i = 0; i < x.Length; i++ )
			{
				result[ i ] = x[ i ] + sep + y[ i ];
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="times"></param>
		/// <returns></returns>
		public static string Repeat( string str, int times )
		{
			StringBuilder buf = new StringBuilder( str.Length*times );
			for( int i = 0; i < times; i++ )
				buf.Append( str );
			return buf.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		/// <param name="placeholder"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string Replace( string template, string placeholder, string replacement )
		{
			// sometimes a null value will get passed in here -> SqlWhereStrings are a good example
			if( template == null ) return null;

			int loc = template.IndexOf( placeholder );
			if( loc < 0 )
			{
				return template;
			}
			else
			{
				return new StringBuilder( template.Substring( 0, loc ) )
					.Append( replacement )
					.Append( Replace(
						template.Substring( loc + placeholder.Length ),
						placeholder,
						replacement
						) ).ToString();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="template"></param>
		/// <param name="placeholder"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string ReplaceOnce( string template, string placeholder, string replacement )
		{
			int loc = template.IndexOf( placeholder );
			if( loc < 0 )
			{
				return template;
			}
			else
			{
				return new StringBuilder( template.Substring( 0, loc ) )
					.Append( replacement )
					.Append( template.Substring( loc + placeholder.Length ) )
					.ToString();
			}
		}

		/// <summary>
		/// Just a fa�ade for calling string.Split()
		/// We don't use our StringTokenizer because string.Split() is
		/// more efficient (but it only works when we don't want to retrieve the delimiters)
		/// </summary>
		/// <param name="separators">separators for the tokens of the list</param>
		/// <param name="list">the string that will be broken into tokens</param>
		/// <returns></returns>
		public static string[ ] Split( string separators, string list )
		{
			return list.Split( separators.ToCharArray() );
		}

		/// <summary>
		/// Splits the String using the StringTokenizer.  
		/// </summary>
		/// <param name="separators">separators for the tokens of the list</param>
		/// <param name="list">the string that will be broken into tokens</param>
		/// <param name="include">true to include the seperators in the tokens.</param>
		/// <returns></returns>
		/// <remarks>
		/// This is more powerful than Split because you have the option of including or 
		/// not including the seperators in the tokens.
		/// </remarks>
		public static string[ ] Split( string separators, string list, bool include )
		{
			StringTokenizer tokens = new StringTokenizer( list, separators, include );
			ArrayList results = new ArrayList();
			foreach( string token in tokens )
			{
				results.Add( token );
			}
			return ( string[ ] ) results.ToArray( typeof( string ) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="qualifiedName"></param>
		/// <returns></returns>
		public static string Unqualify( string qualifiedName )
		{
			return Unqualify( qualifiedName, "." );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="qualifiedName"></param>
		/// <param name="seperator"></param>
		/// <returns></returns>
		public static string Unqualify( string qualifiedName, string seperator )
		{
			return qualifiedName.Substring( qualifiedName.LastIndexOf( seperator ) + 1 );
		}

		/// <summary>
		/// Takes a fully qualified type name and returns the full name of the 
		/// Class - includes namespaces.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public static string GetFullClassname( string typeName )
		{
			return typeName.Trim().Split( ' ', ',' )[ 0 ];
		}

		/// <summary>
		/// Takes a fully qualifed type name (can include the assembly) and just returns
		/// the name of the Class.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		public static string GetClassname( string typeName )
		{
			string[ ] splitClassname = GetFullClassname( typeName ).Split( '.' );

			return splitClassname[ splitClassname.Length - 1 ];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="qualifiedName"></param>
		/// <returns></returns>
		public static string Qualifier( string qualifiedName )
		{
			int loc = qualifiedName.LastIndexOf( "." );
			if( loc < 0 )
			{
				return String.Empty;
			}
			else
			{
				return qualifiedName.Substring( 0, loc );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		public static string[ ] Suffix( string[ ] columns, string suffix )
		{
			if( suffix == null )
				return columns;
			string[ ] qualified = new string[columns.Length];
			for( int i = 0; i < columns.Length; i++ )
			{
				qualified[ i ] = Suffix( columns[ i ], suffix );
			}
			return qualified;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="suffix"></param>
		/// <returns></returns>
		public static string Suffix( string name, string suffix )
		{
			return ( suffix == null ) ?
				name :
				name + suffix;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="columns"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
		public static string[ ] Prefix( string[ ] columns, string prefix )
		{
			if( prefix == null )
				return columns;
			string[ ] qualified = new string[columns.Length];
			for( int i = 0; i < columns.Length; i++ )
			{
				qualified[ i ] = prefix + columns[ i ];
			}
			return qualified;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="qualifiedName"></param>
		/// <returns></returns>
		public static string Root( string qualifiedName )
		{
			int loc = qualifiedName.IndexOf( "." );
			return ( loc < 0 )
				? qualifiedName
				: qualifiedName.Substring( 0, loc );
		}

		/// <summary>
		/// Converts a <see cref="String"/> in the format of "true", "t", "false", or "f" to
		/// a <see cref="Boolean"/>.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <returns>
		/// The <c>value</c> converted to a <see cref="Boolean"/> .
		/// </returns>
		public static bool BooleanValue( string value )
		{
			string trimmed = value.Trim().ToLower( System.Globalization.CultureInfo.InvariantCulture );
			return trimmed.Equals( "true" ) || trimmed.Equals( "t" );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static string ToString( object[ ] array )
		{
			int len = array.Length;

			// if there is no value in the array then return no string...
			if( len == 0 ) return String.Empty;

			StringBuilder buf = new StringBuilder( len*12 );
			for( int i = 0; i < len - 1; i++ )
			{
				buf.Append( array[ i ] ).Append( StringHelper.CommaSpace );
			}
			return buf.Append( array[ len - 1 ] ).ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="placeholders"></param>
		/// <param name="replacements"></param>
		/// <returns></returns>
		public static string[ ] Multiply( string str, IEnumerator placeholders, IEnumerator replacements )
		{
			string[ ] result = new string[ ] {str};
			while( placeholders.MoveNext() )
			{
				replacements.MoveNext();
				result = Multiply( result, placeholders.Current as string, replacements.Current as string[ ] );
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="strings"></param>
		/// <param name="placeholder"></param>
		/// <param name="replacements"></param>
		/// <returns></returns>
		public static string[ ] Multiply( string[ ] strings, string placeholder, string[ ] replacements )
		{
			string[ ] results = new string[replacements.Length*strings.Length];
			int n = 0;
			for( int i = 0; i < replacements.Length; i++ )
			{
				for( int j = 0; j < strings.Length; j++ )
				{
					results[ n++ ] = ReplaceOnce( strings[ j ], placeholder, replacements[ i ] );
				}
			}
			return results;
		}

		/// <summary>
		/// Counts the unquoted instances of the character.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="character"></param>
		/// <returns></returns>
		public static int CountUnquoted( string str, char character )
		{
			if ( SingleQuote == character )
				throw new ArgumentOutOfRangeException("Unquoted count of quotes is invalid") ;

			// Impl note: takes advantage of the fact that an escaped single quote
			// embedded within a quote-block can really be handled as two seperate
			// quote-blocks for the purposes of this method...
			int count = 0;
			char[] chars = str.ToCharArray();
			int stringLength = str == null ? 0 : chars.Length;
			bool inQuote = false;
			for ( int indx = 0; indx < stringLength; indx++ ) 
			{
				if ( inQuote ) 
				{
					if ( SingleQuote == chars[indx] ) 
					{
						inQuote = false;
					}
				}
				else if ( SingleQuote == chars[indx] ) 
				{
					inQuote = true;
				}
				else if ( chars[indx] == character ) 
				{
					count++;
				}
			}
			return count;
		}

		public static bool IsNotEmpty( string str )
		{
			return str != null && str.Length > 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string Qualify( string prefix, string name )
		{
			char first = name[ 0 ];

			// Should we check for prefix == string.Empty rather than a length check?
			if ( prefix != null && prefix.Length > 0 && first != StringHelper.SingleQuote && !char.IsDigit( first ) )
			{
				return prefix + StringHelper.Dot + name;
			}
			else
			{
				return name;
			}
		}

		public static string[] Qualify( string prefix, string[] names )
		{
			// Should we check for prefix == string.Empty rather than a length check?
			if ( prefix != null && prefix.Length > 0 )
			{
				int len = names.Length;
				string[] qualified = new string[ len ];
				for ( int i = 0; i < len; i++ )
				{
					qualified[ i ] = Qualify( prefix, names[ i ] );
				}
				return qualified;
			}
			else
			{
				return names;
			}
		}

		public static int FirstIndexOfChar( string sqlString, string str, int startIndex )
		{
			return sqlString.IndexOfAny( str.ToCharArray(), startIndex );
		}

		public static string Truncate( string str, int length )
		{
			if( str.Length <= length )
			{
				return str;
			}
			else
			{
				return str.Substring( 0, length );
			}
		}
	}
}