using System;
using System.Collections;
using NHibernate.Collection;
using NHibernate.Persister;

namespace NHibernate.Engine
{
	/// <summary>
	/// Defines the internal contract between the <c>Session</c> and other parts of Hibernate
	/// such as implementors of <c>Type</c> or <c>ClassPersister</c>
	/// </summary>
	public interface ISessionImplementor : ISession
	{
		/// <summary>
		/// Get the pre-flush identifier of the collection
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		object GetLoadedCollectionKey( PersistentCollection collection );

		/// <summary>
		/// Get the snapshot of the pre-flush collection state
		/// </summary>
		object GetSnapshot( PersistentCollection collection );

		/// <summary>
		/// Get the <c>PersistentCollection</c> object for an array
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		ArrayHolder GetArrayHolder( object array );

		/// <summary>
		/// Register a <c>PersistentCollection</c> object for an array
		/// </summary>
		/// <param name="holder"></param>
		void AddArrayHolder( ArrayHolder holder );

		/// <summary>
		/// Initialize the collection (if not already initialized)
		/// </summary>
		/// <param name="coolection"></param>
		/// <param name="writing"></param>
		void InitializeCollection( PersistentCollection coolection, bool writing );

		/// <summary>
		/// Is this the "inverse" end of a bidirectional association?
		/// </summary>
		/// <param name="collection"></param>
		/// <returns></returns>
		bool IsInverseCollection( PersistentCollection collection );

		/// <summary>
		/// new in h2.1 and no javadoc
		/// </summary>
		/// <param name="persister"></param>
		/// <param name="id"></param>
		/// <param name="resultSetId"></param>
		/// <returns></returns>
		PersistentCollection GetLoadingCollection( ICollectionPersister persister, object id, object resultSetId );

		/// <summary>
		/// new in h2.1 and no javadoc
		/// </summary>
		void EndLoadingCollections( ICollectionPersister persister, object resultSetId );

		/// <summary>
		/// new in h2.1 and no javadoc
		/// </summary>
		void AfterLoad();

		/// <summary>
		/// new in h2.1 and no javadoc
		/// </summary>
		void BeforeLoad();

		/// <summary>
		/// new in h2.1 and no javadoc
		/// </summary>
		void InitializeNonLazyCollections();

		/// <summary>
		/// Gets the NHibernate collection wrapper from the ISession.
		/// </summary>
		/// <param name="role"></param>
		/// <param name="id"></param>
		/// <param name="owner"></param>
		/// <returns>
		/// A NHibernate wrapped collection.
		/// </returns>
		object GetCollection( string role, object id, object owner );

		/// <summary>
		/// Load an instance without checking if it was deleted. If it does not exist, throw an exception.
		/// This method may create a new proxy or return an existing proxy.
		/// </summary>
		/// <param name="persistentClass">The <see cref="System.Type"/> to load.</param>
		/// <param name="id">The identifier of the object in the database.</param>
		/// <returns>
		/// A proxy of the object or an instance of the object if the <c>persistentClass</c> does not have a proxy.
		/// </returns>
		/// <exception cref="ObjectNotFoundException">No object could be found with that <c>id</c>.</exception>
		object InternalLoad( System.Type persistentClass, object id );

		/// <summary>
		/// Load an instance without checking if it was deleted. If it does not exist, 
		/// return <c>null</c>. Do not create a proxy (but do return any existing proxy).
		/// </summary>
		/// <param name="persistentClass"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		object InternalLoadOneToOne( System.Type persistentClass, object id );

		/// <summary>
		/// Load an instance immediately. Do not return a proxy.
		/// </summary>
		/// <param name="persistentClass"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		object ImmediateLoad( System.Type persistentClass, object id );

		/// <summary>
		/// Load an instance by a unique key that is not the primary key.
		/// </summary>
		/// <param name="persistentClass"></param>
		/// <param name="uniqueKeyPropertyName"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		object LoadByUniqueKey( System.Type persistentClass, string uniqueKeyPropertyName, object id );

		/// <summary>
		/// System time before the start of the transaction
		/// </summary>
		/// <returns></returns>
		long Timestamp { get; }

		/// <summary>
		/// Get the creating SessionFactoryImplementor
		/// </summary>
		/// <returns></returns>
		ISessionFactoryImplementor Factory { get; }

		/// <summary>
		/// Get the prepared statement <c>Batcher</c> for this session
		/// </summary>
		IBatcher Batcher { get; }

		/// <summary>
		/// After actually inserting a row, record the fact that the instance exists on the database
		/// (needed for identity-column key generation)
		/// </summary>
		/// <param name="obj"></param>
		void PostInsert( object obj );

		/// <summary>
		/// After actually deleting a row, record the fact that the instance no longer exists on the
		/// database (needed for identity-column key generation)
		/// </summary>
		/// <param name="obj"></param>
		void PostDelete( object obj );

		/// <summary>
		/// After actually updating a row, record the fact that the database state has been updated.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> instance that was saved.</param>
		/// <param name="updatedState">A updated snapshot of the values in the object.</param>
		/// <param name="nextVersion">The new version to assign to the <c>obj</c>.</param>
		void PostUpdate( object obj, object[ ] updatedState, object nextVersion );

		/// <summary>
		/// Execute a <c>Find()</c> query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		IList Find( string query, QueryParameters parameters );

		/// <summary>
		/// Execute an <c>Iterate()</c> query
		/// </summary>
		/// <param name="query"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		IEnumerable Enumerable( string query, QueryParameters parameters );

		/// <summary>
		/// Execute a filter
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="filter"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		IList Filter( object collection, string filter, QueryParameters parameters );

		/// <summary>
		/// Collection from a filter
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="filter"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		IEnumerable EnumerableFilter( object collection, string filter, QueryParameters parameters );

		/// <summary>
		/// Get the <c>IClassPersister</c> for an object
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		IClassPersister GetPersister( object obj );

		/// <summary>
		/// Add an uninitialized instance of an entity class, as a placeholder to ensure object identity.
		/// Must be called before <c>PostHydrate()</c>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="obj"></param>
		/// <param name="lockMode"></param>
		void AddUninitializedEntity( Key key, object obj, LockMode lockMode );

		/// <summary>
		/// Register the "hydrated" state of an entity instance, after the first step of 2-phase loading
		/// </summary>
		/// <param name="persister"></param>
		/// <param name="id"></param>
		/// <param name="values"></param>
		/// <param name="obj"></param>
		/// <param name="lockMode"></param>
		void PostHydrate( IClassPersister persister, object id, object[ ] values, object obj, LockMode lockMode );

		/// <summary>
		/// Perform the second step of 2-phase load (ie. fully initialize the entity instance)
		/// </summary>
		/// <param name="obj"></param>
		void InitializeEntity( object obj );

		/// <summary>
		/// Get the entity instance associated with the given <c>Key</c>
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object GetEntity( Key key );

		/// <summary>
		/// Return the existing proxy associated with the given <c>Key</c>, or the second
		/// argument (the entity associated with the key) if no proxy exists.
		/// </summary>
		/// <param name="persister">The <see cref="IClassPersister"/> to see if it should be Proxied.</param>
		/// <param name="key">The <see cref="Key"/> that identifies the entity.</param>
		/// <param name="impl"></param>
		/// <returns>Returns a the Proxy for the class or the parameter impl.</returns>
		object ProxyFor( IClassPersister persister, Key key, object impl );

		/// <summary>
		/// Return the existing proxy associated with the given object. (Slower than the form above)
		/// </summary>
		/// <param name="impl"></param>
		/// <returns></returns>
		object ProxyFor( object impl );

		/// <summary>
		/// Notify the session that the transaction completed, so we no longer own the old locks.
		/// (Also we shold release cache softlocks). May be called multiple times during the transaction
		/// completion process.
		/// </summary>
		void AfterTransactionCompletion( bool successful );

		/// <summary>
		/// Return the identifier of the persistent object, or null if transient
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		object GetEntityIdentifier( object obj );

		/// <summary>
		/// Return the identifer of the persistent or transient object, or throw
		/// an exception if the instance is "unsaved"
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		object GetEntityIdentifierIfNotUnsaved( object obj );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		bool IsSaved( object obj );

		/// <summary>
		/// Instantiate the entity class, initializing with the given identifier
		/// </summary>
		/// <param name="clazz"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		object Instantiate( System.Type clazz, object id );

		/// <summary>
		/// Set the lock mode of the entity to the given lock mode
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="lockMode"></param>
		void SetLockMode( object entity, LockMode lockMode );

		/// <summary>
		/// Get the current version of the entity
		/// </summary>
		/// <param name="entity"></param>
		object GetVersion( object entity );

		/// <summary>
		/// Get the lock mode of the entity
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		LockMode GetLockMode( object entity );

		/// <summary>
		/// Get the collection orphans (entities which were removed from
		/// the collection
		/// </summary>
		/// <param name="coll"></param>
		/// <returns></returns>
		ICollection GetOrphans( PersistentCollection coll );

		/// <summary>
		/// Get a batch of uninitialized collection keys for this role
		/// </summary>
		/// <param name="collectionPersister"></param>
		/// <param name="id"></param>
		/// <param name="batchSize"></param>
		/// <returns></returns>
		object[] GetCollectionBatch( ICollectionPersister collectionPersister, object id, int batchSize );

		/// <summary>
		/// Get a batch of unloaded identifiers for this class
		/// </summary>
		/// <param name="clazz"></param>
		/// <param name="id"></param>
		/// <param name="batchSize"></param>
		/// <returns></returns>
		object[] GetClassBatch( System.Type clazz, object id, int batchSize );

		/// <summary>
		/// Register the entity as batch loadable, if enabled
		/// </summary>
		/// <param name="clazz"></param>
		/// <param name="id"></param>
		void ScheduleBatchLoad( System.Type clazz, object id );

		/// <summary>
		/// Execute an SQL Query
		/// </summary>
		/// <param name="sqlQuery"></param>
		/// <param name="aliases"></param>
		/// <param name="classes"></param>
		/// <param name="queryParameters"></param>
		/// <param name="querySpaces"></param>
		/// <returns></returns>
		IList FindBySQL( string sqlQuery, string[] aliases, System.Type[] classes, QueryParameters queryParameters, ICollection querySpaces );

		/// <summary>
		/// new in 2.1 no javadoc
		/// </summary>
		/// <param name="key"></param>
		void AddNonExist( Key key );

		/// <summary>
		/// new in 2.1 no javadoc
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="copiedAlready"></param>
		/// <returns></returns>
		object Copy( object obj, IDictionary copiedAlready );

		/// <summary>
		/// new in 2.1 no javadoc
		/// </summary>
		/// <param name="key"></param>
		/// <param name="collectionPersister"></param>
		/// <returns></returns>
		object GetCollectionOwner( object key, ICollectionPersister collectionPersister );
	}
}