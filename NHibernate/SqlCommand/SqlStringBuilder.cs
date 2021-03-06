using System;
using System.Collections;

namespace NHibernate.SqlCommand
{
	/// <summary>
	/// The SqlStringBuilder is used to construct a SqlString.  The SqlString is a nonmutable
	/// class so it can't have sql parts added to it.  Instead this class should be used to
	/// generate a new SqlString.  The SqlStringBuilder is to SqlString what 
	/// the StringBuilder is to a String.  
	///  
	/// This might actually be a good use for a struct - very lightweight and gives me th
	/// modifications if I pass it to a method such as AddLock(ISqlCommand)...
	/// </summary>
	/// <remarks>
	/// This is a modifiable version - the unmodifiable version is SqlString.  This
	/// should mimic the use of a StringBuilder and just plain String.
	/// 
	/// This is different from the original version of SqlString because this does not
	/// hold the sql string in the form of "column1=@column1" instead it uses an array to
	/// build the sql statement such that 
	/// object[0] = "column1="
	/// object[1] = ref to column1 parameter
	/// 
	/// What this allows us to do is to delay the generating of the parameter for the sql
	/// until the very end - making testing dialect indifferent.  Right now all of our test
	/// to make sure the correct sql is getting built are specific to MsSql2000Dialect.
	/// </remarks>
	public class SqlStringBuilder : ISqlStringBuilder
	{
		// this holds the strings and parameters that make up the full sql statement.
		private ArrayList sqlParts;

		/// <summary>
		/// Create an empty StringBuilder with the default capacity.  
		/// </summary>
		public SqlStringBuilder() : this( 16 )
		{
		}

		/// <summary>
		/// Create a StringBuilder with a specific capacity.
		/// </summary>
		/// <param name="partsCapacity">The number of parts expected.</param>
		public SqlStringBuilder( int partsCapacity )
		{
			sqlParts = new ArrayList( partsCapacity );
		}

		/// <summary>
		/// Create a StringBuilder to modify the SqlString
		/// </summary>
		/// <param name="sqlString">The SqlString to modify.</param>
		public SqlStringBuilder( SqlString sqlString )
		{
			sqlParts = new ArrayList( sqlString.SqlParts.Length );
			foreach( object part in sqlString.SqlParts )
			{
				if (part != null) 
				{
					sqlParts.Add( part );
				}
			}
		}

		/// <summary>
		/// Adds the preformatted sql to the SqlString that is being built.
		/// </summary>
		/// <param name="sql">The string to add.</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder Add( string sql )
		{
			if ( sql != null )
			{
				sqlParts.Add( sql );
			}
			return this;
		}

		/// <summary>
		/// Adds the Parameter to the SqlString that is being built.
		/// The correct operator should be added before the Add(Parameter) is called
		/// because there will be no operator ( such as "=" ) placed between the last Add call
		/// and this Add call.
		/// </summary>
		/// <param name="parameter">The Parameter to add.</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder Add( Parameter parameter )
		{
			if ( parameter != null )
			{
				sqlParts.Add( parameter );
			}
			return this;
		}

		/// <summary>
		/// Attempts to discover what type of object this is and calls the appropriate
		/// method.
		/// </summary>
		/// <param name="part">The part to add when it is not known if it is a Parameter, String, or SqlString.</param>
		/// <returns>This SqlStringBuilder.</returns>
		/// <exception cref="ArgumentException">Thrown when the part is not a Parameter, String, or SqlString.</exception>
		public SqlStringBuilder AddObject( object part )
		{
			if ( part == null ){
				return this;
			}
			Parameter paramPart = part as Parameter;
			if( paramPart!=null )
			{
				return this.Add( paramPart ); // EARLY EXIT
			}

			string stringPart = part as String;
			if( stringPart!=null )
			{
				return this.Add( stringPart );
			}
			
			SqlString sqlPart = part as SqlString;
			if( sqlPart!=null )
			{
				return this.Add( sqlPart, null, null, null );
			}

			// remarks - we should not get to here - this is a problem with the 
			// SQL being generated.
			throw new ArgumentException( "Part was not a Parameter, String, or SqlString." );
		}

		/// <summary>
		/// Adds an existing SqlString to this SqlStringBuilder.  It does NOT add any
		/// prefix, postfix, operator, or wrap around this.  It is equivalent to just 
		/// adding a string.
		/// </summary>
		/// <param name="sqlString">The SqlString to add to this SqlStringBuilder</param>
		/// <returns>This SqlStringBuilder</returns>
		/// <remarks>This calls the overloaded Add(sqlString, null, null, null, false)</remarks>
		public SqlStringBuilder Add( SqlString sqlString )
		{
			if ( sqlString == null )
			{
				return this;
			} 
			else 
			{
				return Add( sqlString, null, null, null, false );
			}
		}


		/// <summary>
		/// Adds an existing SqlString to this SqlStringBuilder
		/// </summary>
		/// <param name="sqlString">The SqlString to add to this SqlStringBuilder</param>
		/// <param name="prefix">String to put at the beginning of the combined SqlString.</param>
		/// <param name="op">How these Statements should be junctioned "AND" or "OR"</param>
		/// <param name="postfix">String to put at the end of the combined SqlString.</param>
		/// <returns>This SqlStringBuilder</returns>
		/// <remarks>
		/// This calls the overloaded Add method with an array of SqlStrings and wrapStatment=false
		/// so it will not be wrapped with a "(" and ")"
		/// </remarks>
		public SqlStringBuilder Add( SqlString sqlString, string prefix, string op, string postfix )
		{
			return Add( new SqlString[ ] {sqlString}, prefix, op, postfix, false );
		}

		/// <summary>
		/// Adds an existing SqlString to this SqlStringBuilder
		/// </summary>
		/// <param name="sqlString">The SqlString to add to this SqlStringBuilder</param>
		/// <param name="prefix">String to put at the beginning of the combined SqlString.</param>
		/// <param name="op">How these Statements should be junctioned "AND" or "OR"</param>
		/// <param name="postfix">String to put at the end of the combined SqlString.</param>
		/// <param name="wrapStatement">Wrap each SqlStrings with "(" and ")"</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder Add( SqlString sqlString, string prefix, string op, string postfix, bool wrapStatement )
		{
			return Add( new SqlString[ ] {sqlString}, prefix, op, postfix, wrapStatement );
		}

		/// <summary>
		/// Adds existing SqlStrings to this SqlStringBuilder
		/// </summary>
		/// <param name="sqlStrings">The SqlStrings to combine.</param>
		/// <param name="prefix">String to put at the beginning of the combined SqlString.</param>
		/// <param name="op">How these SqlStrings should be junctioned "AND" or "OR"</param>
		/// <param name="postfix">String to put at the end of the combined SqlStrings.</param>
		/// <returns>This SqlStringBuilder</returns>
		/// <remarks>This calls the overloaded Add method with wrapStatement=true</remarks>
		public SqlStringBuilder Add( SqlString[ ] sqlStrings, string prefix, string op, string postfix )
		{
			return Add( sqlStrings, prefix, op, postfix, true );
		}

		/// <summary>
		/// Adds existing SqlStrings to this SqlStringBuilder
		/// </summary>
		/// <param name="sqlStrings">The SqlStrings to combine.</param>
		/// <param name="prefix">String to put at the beginning of the combined SqlStrings.</param>
		/// <param name="op">How these SqlStrings should be junctioned "AND" or "OR"</param>
		/// <param name="postfix">String to put at the end of the combined SqlStrings.</param>
		/// <param name="wrapStatement">Wrap each SqlStrings with "(" and ")"</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder Add( SqlString[ ] sqlStrings, string prefix, string op, string postfix, bool wrapStatement )
		{
			if( prefix != null )
			{
				sqlParts.Add( prefix );
			}

			bool opNeeded = false;

			foreach( SqlString sqlString in sqlStrings )
			{
				if( opNeeded )
				{
					sqlParts.Add( " " + op + " " );
				}

				opNeeded = true;

				if( wrapStatement )
				{
					sqlParts.Add( "(" );
				}
				foreach( object sqlPart in sqlString.SqlParts )
				{
					if (sqlPart != null )
					{
						sqlParts.Add( sqlPart );
					}
				}

				if( wrapStatement )
				{
					sqlParts.Add( ")" );
				}


			}

			if( postfix != null )
			{
				sqlParts.Add( postfix );
			}

			return this;
		}

		/// <summary>
		/// Gets the number of SqlParts in this SqlStringBuilder.
		/// </summary>
		/// <returns>
		/// The number of SqlParts in this SqlStringBuilder.
		/// </returns>
		public int Count
		{
			get { return sqlParts.Count; }
		}

		/// <summary>
		/// Gets or Sets the element at the index
		/// </summary>
		/// <value>Returns a string or Parameter.</value>
		/// <remarks></remarks>
		public object this[ int index ]
		{
			get { return sqlParts[ index ]; }
			set { sqlParts[ index ] = value; }
		}

		/// <summary>
		/// Insert a string containing sql into the SqlStringBuilder at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the sql should be inserted.</param>
		/// <param name="sql">The string containing sql to insert.</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder Insert( int index, string sql )
		{
			sqlParts.Insert( index, sql );
			return this;
		}

		/// <summary>
		/// Insert a Parameter into the SqlStringBuilder at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the Parameter should be inserted.</param>
		/// <param name="param">The Parameter to insert.</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder Insert( int index, Parameter param )
		{
			sqlParts.Insert( index, param );
			return this;
		}

		/// <summary>
		/// Removes the string or Parameter at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <returns>This SqlStringBuilder</returns>
		public SqlStringBuilder RemoveAt( int index )
		{
			sqlParts.RemoveAt( index );
			return this;
		}

		/// <summary>
		/// Converts the mutable SqlStringBuilder into the immutable SqlString.
		/// </summary>
		/// <returns>The SqlString that was built.</returns>
		public SqlString ToSqlString()
		{
			return new SqlString( ( object[ ] ) sqlParts.ToArray( typeof( object ) ) );
		}
	}
}