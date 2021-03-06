using System.Collections;
using NHibernate.Engine;
using NHibernate.Expression;
using NHibernate.Transform;

namespace NHibernate
{
	/// <summary>
	/// <p>
	/// Criteria is a simplified API for retrieving entities
	/// by composing Expression objects. This is a very
	/// convenient approach for functionality like "search" screens
	/// where there is a variable number of conditions to be placed
	/// upon the result set.
	/// </p>
	/// <p>
	/// The Session is a factory for ICriteria. 
	/// Expression instances are usually obtained via 
	/// the factory methods on Expression. eg:
	/// <code>
	/// IList cats = session.CreateCriteria(typeof(Cat)) 
	///     .Add( Expression.Like("name", "Iz%") ) 
	///     .Add( Expression.Gt( "weight", minWeight ) ) 
	///     .AddOrder( Order.Asc("age") ) 
	///     .List(); 
	/// </code> 
	/// You may navigate associations using <c>CreateAlias()</c> or
	/// <c>CreateCriteria()</c>.
	/// <code>
	/// IList cats = session.CreateCriteria(typeof(Cat))
	///		.CreateCriteria("kittens")
	///			.Add( Expression.like("name", "Iz%") )
	///			.List();
	///	</code>
	/// Hibernate's query language is much more general
	/// and should be used for non-simple cases.
	/// </p>
	/// This is an experimental API
	/// </summary>
	public interface ICriteria
	{
		// NH: Static declarations moved to CriteriaUtil class (CriteriaUtil.cs)

		/// <summary>
		/// Set a limit upon the number of objects to be retrieved
		/// </summary>
		/// <param name="maxResults"></param>
		ICriteria SetMaxResults( int maxResults );

		/// <summary>
		/// Set the first result to be retrieved
		/// </summary>
		/// <param name="firstResult"></param>
		ICriteria SetFirstResult( int firstResult );

		// SetFetchSize - not ported from H2.1

		/// <summary>
		/// Set a timeout for the underlying ADO.NET query
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		ICriteria SetTimeout( int timeout );

		/// <summary>
		/// Add an Expression to constrain the results to be retrieved.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		ICriteria Add( Expression.ICriterion expression );

		/// <summary>
		/// An an Order to the result set 
		/// </summary>
		/// <param name="order"></param>
		ICriteria AddOrder( Order order );

		/// <summary>
		/// Get the results
		/// </summary>
		/// <returns></returns>
		IList List();

		/// <summary>
		/// Convenience method to return a single instance that matches
		/// the query, or null if the query returns no results.
		/// </summary>
		/// <returns>the single result or <c>null</c></returns>
		/// <exception cref="HibernateException">
		/// If there is more than one matching result
		/// </exception>
		object UniqueResult();

		/// <summary>
		/// Specify an association fetching strategy.  Currently, only
		/// one-to-many and one-to-one associations are supported.
		/// </summary>
		/// <param name="associationPath">A dot seperated property path.</param>
		/// <param name="mode">The Fetch mode.</param>
		/// <returns></returns>
		ICriteria SetFetchMode( string associationPath, FetchMode mode );

		/// <summary>
		/// Join an association, assigning an alias to the joined entity
		/// </summary>
		/// <param name="associationPath"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		ICriteria CreateAlias( string associationPath, string alias );

		/// <summary>
		/// Create a new <see cref="ICriteria" />, "rooted" at the associated entity
		/// </summary>
		/// <param name="associationPath"></param>
		/// <returns></returns>
		ICriteria CreateCriteria( string associationPath );

		/// <summary>
		/// Create a new <see cref="ICriteria" />, "rooted" at the associated entity,
		/// assigning the given alias
		/// </summary>
		/// <param name="associationPath"></param>
		/// <param name="alias"></param>
		/// <returns></returns>
		ICriteria CreateCriteria( string associationPath, string alias );

		/// <summary>
		/// Get the persistent class that this <c>ICriteria</c> applies to
		/// </summary>
		System.Type CriteriaClass { get; }

		// NH: Deprecated methods not ported

		/// <summary>
		/// Get the persistent class that the alias refers to
		/// </summary>
		/// <param name="alias"></param>
		/// <returns></returns>
		System.Type GetCriteriaClass( string alias );

		/// <summary>
		/// Set a strategy for handling the query results. This determines the
		/// "shape" of the query result set.
		/// <seealso cref="CriteriaUtil.RootEntity"/>
		/// <seealso cref="CriteriaUtil.DistinctRootEntity"/>
		/// <seealso cref="CriteriaUtil.AliasToEntityMap"/>
		/// </summary>
		/// <param name="resultTransformer"></param>
		/// <returns></returns>
		ICriteria SetResultTransformer( IResultTransformer resultTransformer );

		/// <summary>
		/// Set the lock mode of the current entity
		/// </summary>
		/// <param name="lockMode">the lock mode</param>
		/// <returns></returns>
		ICriteria SetLockMode( LockMode lockMode );

		/// <summary>
		/// Set the lock mode of the aliased entity
		/// </summary>
		/// <param name="alias">an alias</param>
		/// <param name="lockMode">the lock mode</param>
		/// <returns></returns>
		ICriteria SetLockMode( string alias, LockMode lockMode );

		/// <summary>
		/// Enable caching of this query result set
		/// </summary>
		/// <param name="cacheable"></param>
		/// <returns></returns>
		ICriteria SetCacheable( bool cacheable );

		/// <summary>
		/// Set the name of the cache region.
		/// </summary>
		/// <param name="cacheRegion">the name of a query cache region, or <c>null</c>
		/// for the default query cache</param>
		/// <returns></returns>
		ICriteria SetCacheRegion( string cacheRegion );
	}
}