using System;
using System.Collections;

using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Expression
{
	/// <summary>
	/// The <c>Expression</c> namespace may be used by applications as a framework for building 
	/// new kinds of <see cref="ICriterion" />. However, it is intended that most applications will 
	/// simply use the built-in criterion types via the static factory methods of this class.
	/// </summary>
	public sealed class Expression
	{
		private Expression()
		{
			// can not be instantiated
		}

		/// <summary>
		/// Apply an "equal" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>An <see cref="EqExpression" />.</returns>
		public static SimpleExpression Eq( string propertyName, object value )
		{
			return new EqExpression( propertyName, value );
		}

		/// <summary>
		/// Apply a "like" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>A <see cref="LikeExpression" />.</returns>
		public static SimpleExpression Like( string propertyName, object value )
		{
			return new LikeExpression( propertyName, value );
		}

		public static SimpleExpression Like( string propertyName, string value, MatchMode matchMode )
		{
			return new LikeExpression( propertyName, value, matchMode );
		}

		public static ICriterion InsensitiveLike( string propertyName, string value, MatchMode matchMode )
		{
			return new InsensitiveLikeExpression( propertyName, value, matchMode );
		}

		/// <summary>
		/// A case-insensitive "like", similar to Postgres "ilike" operator
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>An <see cref="InsensitiveLikeExpression" />.</returns>
		public static ICriterion InsensitiveLike( string propertyName, object value )
		{
			return new InsensitiveLikeExpression( propertyName, value );
		}

		/// <summary>
		/// Apply a "greater than" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>A <see cref="GtExpression" />.</returns>
		public static SimpleExpression Gt( string propertyName, object value )
		{
			return new GtExpression( propertyName, value );
		}

		/// <summary>
		/// Apply a "less than" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>A <see cref="LtExpression" />.</returns>
		public static SimpleExpression Lt( string propertyName, object value )
		{
			return new LtExpression( propertyName, value );
		}

		/// <summary>
		/// Apply a "less than or equal" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>A <see cref="LeExpression" />.</returns>
		public static SimpleExpression Le( string propertyName, object value )
		{
			return new LeExpression( propertyName, value );
		}

		/// <summary>
		/// Apply a "greater than or equal" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="value">The value for the Property.</param>
		/// <returns>A <see cref="GtExpression" />.</returns>
		public static SimpleExpression Ge( string propertyName, object value )
		{
			return new GeExpression( propertyName, value );
		}

		/// <summary>
		/// Apply a "between" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="lo">The low value for the Property.</param>
		/// <param name="hi">The high value for the Property.</param>
		/// <returns>A <see cref="BetweenExpression" />.</returns>
		public static ICriterion Between( string propertyName, object lo, object hi )
		{
			return new BetweenExpression( propertyName, lo, hi );
		}

		/// <summary>
		/// Apply an "in" constraint to the named property 
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="values">An array of values.</param>
		/// <returns>An <see cref="InExpression" />.</returns>
		public static ICriterion In( string propertyName, object[ ] values )
		{
			return new InExpression( propertyName, values );
		}

		/// <summary>
		/// Apply an "in" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <param name="values">An ICollection of values.</param>
		/// <returns>An <see cref="InExpression" />.</returns>
		public static ICriterion In( string propertyName, ICollection values )
		{
			object[ ] ary = new object[values.Count];
			values.CopyTo( ary, 0 );
			return new InExpression( propertyName, ary );
		}

		/// <summary>
		/// Apply an "is null" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <returns>A <see cref="NullExpression" />.</returns>
		public static ICriterion IsNull( string propertyName )
		{
			return new NullExpression( propertyName );
		}

		/// <summary>
		/// Apply an "equal" constraint to two properties
		/// </summary>
		/// <param name="propertyName">The lhs Property Name</param>
		/// <param name="otherPropertyName">The rhs Property Name</param>
		/// <returns>A <see cref="EqPropertyExpression"/> .</returns>
		public static ICriterion EqProperty( string propertyName, string otherPropertyName )
		{
			return new EqPropertyExpression( propertyName, otherPropertyName );
		}

		/// <summary>
		/// Apply a "less than" constraint to two properties
		/// </summary>
		/// <param name="propertyName">The lhs Property Name</param>
		/// <param name="otherPropertyName">The rhs Property Name</param>
		/// <returns>A <see cref="LtPropertyExpression"/> .</returns>
		public static ICriterion LtProperty( string propertyName, string otherPropertyName )
		{
			return new LtPropertyExpression( propertyName, otherPropertyName );
		}

		/// <summary>
		/// Apply a "less than or equal" constraint to two properties
		/// </summary>
		/// <param name="propertyName">The lhs Property Name</param>
		/// <param name="otherPropertyName">The rhs Property Name</param>
		/// <returns>A <see cref="LePropertyExpression"/> .</returns>
		public static ICriterion LeProperty( string propertyName, string otherPropertyName )
		{
			return new LePropertyExpression( propertyName, otherPropertyName );
		}

		/// <summary>
		/// Apply an "is not null" constraint to the named property
		/// </summary>
		/// <param name="propertyName">The name of the Property in the class.</param>
		/// <returns>A <see cref="NotNullExpression" />.</returns>
		public static ICriterion IsNotNull( string propertyName )
		{
			return new NotNullExpression( propertyName );
		}


		/// <summary>
		/// Return the conjuction of two expressions
		/// </summary>
		/// <param name="lhs">The Expression to use as the Left Hand Side.</param>
		/// <param name="rhs">The Expression to use as the Right Hand Side.</param>
		/// <returns>An <see cref="AndExpression" />.</returns>
		public static ICriterion And( ICriterion lhs, ICriterion rhs )
		{
			return new AndExpression( lhs, rhs );
		}

		/// <summary>
		/// Return the disjuction of two expressions
		/// </summary>
		/// <param name="lhs">The Expression to use as the Left Hand Side.</param>
		/// <param name="rhs">The Expression to use as the Right Hand Side.</param>
		/// <returns>An <see cref="OrExpression" />.</returns>
		public static ICriterion Or( ICriterion lhs, ICriterion rhs )
		{
			return new OrExpression( lhs, rhs );
		}

		/// <summary>
		/// Return the negation of an expression
		/// </summary>
		/// <param name="expression">The Expression to negate.</param>
		/// <returns>A <see cref="NotExpression" />.</returns>
		public static ICriterion Not( ICriterion expression )
		{
			return new NotExpression( expression );
		}

		/// <summary>
		/// Apply a constraint expressed in SQL, with the given SQL parameters
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="values"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		public static ICriterion Sql( SqlString sql, object[ ] values, IType[ ] types )
		{
			return new SQLCriterion( sql, values, types );
		}

		/// <summary>
		/// Apply a constraint expressed in SQL, with the given SQL parameter
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="value"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ICriterion Sql( SqlString sql, object value, IType type )
		{
			return new SQLCriterion( sql, new object[ ] {value}, new IType[ ] {type} );
		}

		/// <summary>
		/// Apply a constraint expressed in SQL
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public static ICriterion Sql( SqlString sql )
		{
			return new SQLCriterion( sql, ArrayHelper.EmptyObjectArray, ArrayHelper.EmptyTypeArray );
		}

		/// <summary>
		/// Apply a constraint expressed in SQL
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public static ICriterion Sql( string sql )
		{
			return new SQLCriterion( new SqlString( sql ), ArrayHelper.EmptyObjectArray, ArrayHelper.EmptyTypeArray );
		}

		/// <summary>
		/// Group expressions together in a single conjunction (A and B and C...)
		/// </summary>
		public static Conjunction Conjunction()
		{
			return new Conjunction();
		}

		/// <summary>
		/// Group expressions together in a single disjunction (A or B or C...)
		/// </summary>
		public static Disjunction Disjunction()
		{
			return new Disjunction();
		}

		/// <summary>
		/// Apply an "equals" constraint to each property in the key set of a IDictionary
		/// </summary>
		/// <param name="propertyNameValues">a dictionary from property names to values</param>
		/// <returns></returns>
		public static ICriterion AllEq( IDictionary propertyNameValues )
		{
			Conjunction conj = Conjunction();

			foreach( DictionaryEntry item in propertyNameValues )
			{
				conj.Add( Eq( item.Key.ToString(), item.Value ) );
			}

			return conj;
		}
	}
}