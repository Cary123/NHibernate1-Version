using NHibernate.Engine;
using NHibernate.Persister;
using NHibernate.Type;

namespace NHibernate.Collection
{
	/// <summary>
	/// Summary description for CompositeElementPropertyMapping.
	/// </summary>
	public class CompositeElementPropertyMapping : AbstractPropertyMapping
	{
		private readonly IAbstractComponentType compositeType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="elementColumns"></param>
		/// <param name="compositeType"></param>
		/// <param name="factory"></param>
		public CompositeElementPropertyMapping( string[] elementColumns, IAbstractComponentType compositeType, ISessionFactoryImplementor factory )
		{
			this.compositeType = compositeType;

			InitComponentPropertyPaths( null, compositeType, elementColumns, factory );
		}

		/// <summary></summary>
		public override IType Type
		{
			get { return compositeType; }
		}

		/// <summary></summary>
		public override string ClassName
		{
			get { return compositeType.Name; }
		}
	}
}
