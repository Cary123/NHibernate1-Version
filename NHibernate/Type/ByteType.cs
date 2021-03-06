using System;
using System.Collections;
using System.Data;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Maps a <see cref="System.Byte"/> Property 
	/// to a <see cref="DbType.Byte"/> column.
	/// </summary>
	[Serializable]
	public class ByteType : ValueTypeType, IDiscriminatorType, IVersionType
	{
		/// <summary></summary>
		internal ByteType() : base( new ByteSqlType() )
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public override object Get( IDataReader rs, int index )
		{
			return Convert.ToByte( rs[ index ] );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public override object Get( IDataReader rs, string name )
		{
			return Convert.ToByte( rs[ name ] );
		}

		/// <summary></summary>
		public override System.Type ReturnedClass
		{
			get { return typeof( byte ); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="value"></param>
		/// <param name="index"></param>
		public override void Set( IDbCommand cmd, object value, int index )
		{
			( ( IDataParameter ) cmd.Parameters[ index ] ).Value = ( byte ) value;
		}

		/// <summary></summary>
		public override string Name
		{
			get { return "Byte"; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override string ObjectToSQLString( object value )
		{
			return value.ToString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public virtual object StringToObject( string xml )
		{
			return FromString( xml );
		}

		public override object FromStringValue( string xml )
		{
			return byte.Parse( xml );
		}

		public object Next( object current )
		{
			return ( byte ) ( ( byte ) current + ( byte )1 );
		}

		public object Seed
		{
			get { return ( byte ) 0; }
		}

		public IComparer Comparator
		{
			get { return Comparer.DefaultInvariant; }
		}
	}
}
