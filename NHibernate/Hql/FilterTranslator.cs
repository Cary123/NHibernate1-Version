using System.Collections;
using System.Runtime.CompilerServices;
using NHibernate.Engine;

namespace NHibernate.Hql
{
	/// <summary></summary>
	public class FilterTranslator : QueryTranslator
	{
		/// <summary>
		/// 
		/// </summary>
		public FilterTranslator( string queryString ) : base( queryString )
		{
		}

		/// <summary>
		/// Compile a filter. This method may be called multiple
		/// times. Subsequent invocations are no-ops.
		/// </summary>
		[MethodImpl( MethodImplOptions.Synchronized )]
		public void Compile( string collectionRole, ISessionFactoryImplementor factory, IDictionary replacements, bool scalar )
		{
			if( !Compiled )
			{
				this.factory = factory; // yick!
				AddFromAssociation( "this", collectionRole );
				base.Compile( factory, replacements, scalar );
			}
		}
	}
}