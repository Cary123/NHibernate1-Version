using System;
using System.Collections;
using System.Data;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister;
using NHibernate.SqlTypes;
using NHibernate.Util;

namespace NHibernate.Type
{
	/// <summary>
	/// PersistentCollectionType.
	/// </summary>
	[Serializable]
	public abstract class PersistentCollectionType : AbstractType, IAssociationType
	{
		private readonly string role;
		private static readonly SqlType[ ] NoSqlTypes = {};

		protected PersistentCollectionType( string role )
		{
			this.role = role;
		}

		public virtual string Role
		{
			get { return role; }
		}

		public override bool IsPersistentCollectionType
		{
			get { return true; }
		}

		public override sealed bool Equals( object x, object y )
		{
			return x == y ||
				( x is PersistentCollection && ( (PersistentCollection) x ).IsWrapper( y ) ) ||
				( y is PersistentCollection && ( (PersistentCollection) y ).IsWrapper( x ) );
		}

		public abstract PersistentCollection Instantiate( ISessionImplementor session, ICollectionPersister persister );

		public override object NullSafeGet( IDataReader rs, string name, ISessionImplementor session, object owner )
		{
			throw new AssertionFailure( "bug in PersistentCollectionType" );
		}

		public override object NullSafeGet( IDataReader rs, string[ ] name, ISessionImplementor session, object owner )
		{
			return ResolveIdentifier( Hydrate( rs, name, session, owner ), session, owner );
		}

		public override void NullSafeSet( IDbCommand cmd, object value, int index, ISessionImplementor session )
		{
		}

		public override SqlType[ ] SqlTypes( IMapping session )
		{
			return NoSqlTypes;
		}

		public override int GetColumnSpan( IMapping session )
		{
			return 0;
		}

		public override string ToString( object value, ISessionFactoryImplementor factory )
		{
			if( value == null )
			{
				return "null";
			}

			IType elemType = GetElementType( factory );
			if( NHibernateUtil.IsInitialized( value ) )
			{
				IList list = new ArrayList();
				ICollection elements = GetElementsCollection( value );
				foreach( object element in elements )
				{
					list.Add( elemType.ToString( element, factory ) );
				}
				return CollectionPrinter.ToString( list );
			}
			else 
			{
				return "uninitialized";
			}
		}

		public override object FromString( string xml )
		{
			throw new NotSupportedException();
		}


		public override object DeepCopy( object value )
		{
			return value;
		}

		public override string Name
		{
			get { return ReturnedClass.Name; }
		}


		/// <summary>
		/// Returns a reference to the elements in the collection.  
		/// </summary>
		/// <param name="collection">The object that holds the ICollection.</param>
		/// <returns>An ICollection of the Elements(classes) in the Collection.</returns>
		/// <remarks>
		/// By default the parameter <c>collection</c> is just cast to an ICollection.  Collections
		/// such as Maps and Sets should override this so that the Elements are returned - not a
		/// DictionaryEntry.
		/// </remarks>
		public virtual ICollection GetElementsCollection( object collection )
		{
			return ( ( ICollection ) collection );
		}

		public override bool IsMutable
		{
			get { return false; }
		}

		public override object Disassemble( object value, ISessionImplementor session )
		{
			return null;
		}

		public override object Assemble( object cached, ISessionImplementor session, object owner )
		{
			object id = session.GetEntityIdentifier( owner );
			if( id == null )
			{
				throw new AssertionFailure( "owner id unknown when re-assembling collection reference" );
			}
			return ResolveIdentifier( id, session, owner );
		}

		private bool IsOwnerVersioned( ISessionImplementor session )
		{
			System.Type ownerClass = session.Factory.GetCollectionPersister( role ).OwnerClass;

			return session.Factory.GetPersister( ownerClass ).IsVersioned;
		}

		public override bool IsDirty( object old, object current, ISessionImplementor session )
		{
			// collections don't dirty an unversioned parent entity

			// TODO: I don't like this implementation; it would be better if this was handled by SearchForDirtyCollections();
			return IsOwnerVersioned( session ) && base.IsDirty( old, current, session );
		}

		public override bool HasNiceEquals
		{
			get { return false; }
		}

		/// <summary>
		/// Wraps a collection from System.Collections or Iesi.Collections inside one of the 
		/// NHibernate collections.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> for the collection to be a part of.</param>
		/// <param name="collection">The unwrapped collection.</param>
		/// <returns>
		/// A subclass of <see cref="PersistentCollection"/> that wraps the non NHibernate collection.
		/// </returns>
		public abstract PersistentCollection Wrap( ISessionImplementor session, object collection );

		// Note: return true because this type is castable to IAssociationType. Not because
		// all collections are associations.
		public override bool IsAssociationType
		{
			get { return true; }
		}

		public virtual ForeignKeyType ForeignKeyType
		{
			get { return ForeignKeyType.ForeignKeyToParent; }
		}

		public override object Hydrate( IDataReader rs, string[ ] name, ISessionImplementor session, object owner )
		{
			return session.GetEntityIdentifier( owner );
		}

		public override object ResolveIdentifier( object value, ISessionImplementor session, object owner )
		{
			if( value == null )
			{
				return null;
			}
			else
			{
				return session.GetCollection( role, value, owner );
			}
		}

		public virtual bool IsArrayType
		{
			get { return false; }
		}

		public bool UsePrimaryKeyAsForeignKey
		{
			get { return true; }
		}

		public IJoinable GetJoinable( ISessionFactoryImplementor factory )
		{
			return (IJoinable) factory.GetCollectionPersister( role );
		}

		public string[] GetReferencedColumns( ISessionFactoryImplementor factory )
		{
			//I really, really don't like the fact that a Type now knows about column mappings!
			//bad seperation of concerns ... could we move this somehow to Joinable interface??
			return GetJoinable( factory ).JoinKeyColumnNames ;
		}

		public override bool IsModified(object old, object current, ISessionImplementor session)
		{
			return false;
		}

		public System.Type GetAssociatedClass( ISessionFactoryImplementor factory )
		{
			try
			{
				IQueryableCollection collectionPersister = (IQueryableCollection) factory.GetCollectionPersister( role );
				if ( !collectionPersister.ElementType.IsEntityType )
				{
					throw new MappingException( string.Format( "collection was not an association: {0}", collectionPersister.Role ) ) ;
				}
				return collectionPersister.ElementPersister.MappedClass;
			}
			catch ( InvalidCastException ice)
			{
				throw new MappingException( "collection role is not queryable " + role, ice );
			}
		}

		public override object Copy( object original, object target, ISessionImplementor session, object owner, IDictionary copiedAlready )
		{
			if ( original == null )
			{
				return null;
			}

			if ( !NHibernateUtil.IsInitialized( original ) )
			{
				return target;
			}

			IList originalCopy = new ArrayList( ( ICollection ) original );
			ICollectionPersister cp = session.Factory.GetCollectionPersister( role );

			ICollection result = target == null
				? Instantiate( session, cp )
				: ( ICollection ) target;
			Clear( result );

			foreach ( object obj in originalCopy )
			{
				Add( result, CopyElement( cp, obj, session, owner, copiedAlready ) );
			}

			return result;
		}

		public IType GetElementType( ISessionFactoryImplementor factory )
		{
			return factory.GetCollectionPersister( Role ).ElementType;
		}

		public override string ToString()
		{
			return base.ToString() + " for " + Role;
		}

		// Methods added in NH

		protected virtual void Clear( ICollection collection )
		{
			throw new NotImplementedException(
				"PersistentCollectionType.Clear was not overriden for type "
				+ GetType().FullName );
		}

		protected virtual void Add( ICollection collection, object element )
		{
			throw new NotImplementedException(
				"PersistentCollectionType.Add was not overriden for type "
				+ GetType().FullName );
		}

		protected virtual object CopyElement( ICollectionPersister persister, object element, ISessionImplementor session, object owner, IDictionary copiedAlready )
		{
			return persister.ElementType.Copy( element, null, session, owner, copiedAlready );
		}

	}
}