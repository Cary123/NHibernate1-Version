using System.Collections;

namespace NHibernate.Cache
{
	/// <summary>
	/// Implementors manage transactional access to cached data.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Transactions pass in a timestamp indicating transaction start time.
	/// </para>
	/// <para>
	/// When used to Cache Entities and Collections the <c>key</c> is the <c>id</c> of the
	/// Entity/Collection and the <c>value</c> should be set to the <see cref="Impl.CacheEntry"/> 
	/// for an Entity and the results of <see cref="Collection.PersistentCollection"/>.Disassemble for a Collection.
	/// </para>
	/// </remarks>
	public interface ICacheConcurrencyStrategy
	{
		/// <summary>
		/// Attempt to retrieve an object from the Cache
		/// </summary>
		/// <param name="key">The key (id) of the object to get out of the Cache.</param>
		/// <param name="txTimestamp">A timestamp prior to the transaction start time</param>
		/// <returns>The cached object or <c>null</c></returns>
		/// <exception cref="CacheException"></exception>
		object Get( object key, long txTimestamp );

		/// <summary>
		/// Attempt to cache an object, after loading from the database
		/// </summary>
		/// <param name="key">The key (id) of the object to put in the Cache.</param>
		/// <param name="value">The value</param>
		/// <param name="txTimestamp">A timestamp prior to the transaction start time</param>
		/// <param name="version">the version number of the object we are putting</param>
		/// <param name="versionComparer">a Comparer to be used to compare version numbers</param>
		/// <returns><c>true</c> if the object was successfully cached</returns>
		/// <exception cref="CacheException"></exception>
		bool Put( object key, object value, long txTimestamp, object version, IComparer versionComparer );

		/// <summary>
		/// We are going to attempt to update/delete the keyed object
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="version"></param>
		/// <exception cref="CacheException"></exception>
		/// <remarks>This method is used by "asynchronous" concurrency strategies.</remarks>
		ISoftLock Lock( object key, object version );

		/// <summary>
		/// Called after an item has become stale (before the transaction completes).
		/// </summary>
		/// <param name="key"></param>
		/// <exception cref="CacheException"></exception>
		/// <remarks>This method is used by "synchronous" concurrency strategies.</remarks>
		void Evict( object key );

		/// <summary>
		/// Called after an item has been updated (before the transaction completes),
		/// instead of calling Evict().
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <remarks>This method is used by "synchronous" concurrency strategies.</remarks>
		void Update( object key, object value );

		/// <summary>
		/// Called after an item has been inserted (before the transaction completes), instead of calling Evict().
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <remarks>This method is used by "synchronous" concurrency strategies.</remarks>
		void Insert( object key, object value );

		/// <summary>
		/// Called when we have finished the attempted update/delete (which may or
		/// may not have been successful), after transaction completion.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="lock">The soft lock</param>
		/// <exception cref="CacheException"></exception>
		/// <remarks>This method is used by "asynchronous" concurrency strategies.</remarks>
		void Release( object key, ISoftLock @lock );

		/// <summary>
		/// Called after an item has been updated (after the transaction completes),
		/// instead of calling Release().
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="version"></param>
		/// <param name="lock"></param>
		/// <remarks>This method is used by "asynchronous" concurrency strategies.</remarks>
		void AfterUpdate(object key, object value, object version, ISoftLock @lock );

		/// <summary>
		/// Called after an item has been inserted (after the transaction completes), instead of calling release().
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="version"></param>
		/// <remarks>This method is used by "asynchronous" concurrency strategies.</remarks>
		void AfterInsert(object key, object value, object version );

		/// <summary>
		/// Evict an item from the cache immediately (without regard for transaction isolation).
		/// </summary>
		/// <param name="key"></param>
		/// <exception cref="CacheException"></exception>
		void Remove( object key );

		/// <summary>
		/// Evict all items from the cache immediately.
		/// </summary>
		/// <exception cref="CacheException"></exception>
		void Clear();

		/// <summary>
		/// Clean up all resources.
		/// </summary>
		/// <exception cref="CacheException"></exception>
		void Destroy();

		/// <summary>
		/// Gets or sets the <see cref="ICache"/> for this strategy to use.
		/// </summary>
		/// <value>The <see cref="ICache"/> for this strategy to use.</value>
		ICache Cache { get; set; }

		/// <summary>
		/// Enable "minimal puts" mode for this cache
		/// </summary>
		/// <exception cref="CacheException"></exception>
		bool MinimalPuts { set; }
	}
}