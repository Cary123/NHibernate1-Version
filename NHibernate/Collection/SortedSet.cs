using System;
using System.Collections;
using Iesi.Collections;
using NHibernate.Engine;

namespace NHibernate.Collection
{
	/// <summary>
	/// A Persistent wrapper for a <c>Iesi.Collections.ISet</c> that has
	/// Set logic to prevent duplicate elements.
	/// </summary>
	/// <remarks>
	/// This class uses the Iesi.Collections.SortedSet for the SortedSet.  
	/// </remarks>
	[Serializable]
	public class SortedSet : Set, ISet
	{
		private IComparer comparer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="persister"></param>
		/// <returns></returns>
		protected override ICollection Snapshot( ICollectionPersister persister )
		{
			SortedList clonedSet = new SortedList( comparer, internalSet.Count );
			foreach( object obj in internalSet )
			{
				object copy = persister.ElementType.DeepCopy( obj );
				clonedSet.Add( copy, copy );
			}

			return clonedSet;
		}

		/// <summary></summary>
		public IComparer Comparer
		{
			get { return comparer; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="persister"></param>
		public override void BeforeInitialize( ICollectionPersister persister )
		{
			internalSet = new Iesi.Collections.SortedSet( Comparer );
		}

		/// <summary>
		/// Constuct a new empty SortedSet that uses a IComparer to perform the sorting.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="comparer">The IComparer to user for Sorting.</param>
		internal SortedSet( ISessionImplementor session, IComparer comparer ) : base( session )
		{
			this.comparer = comparer;
		}

		/// <summary>
		/// Construct a new SortedSet initialized with the map values.
		/// </summary>
		/// <param name="session">The Session to be bound to.</param>
		/// <param name="map">The initial values.</param>
		/// <param name="comparer">The IComparer to use for Sorting.</param>
		internal SortedSet( ISessionImplementor session, ISet map, IComparer comparer )
			: base( session, new Iesi.Collections.SortedSet( map, comparer ) )
		{
			this.comparer = comparer;
		}
	}
}