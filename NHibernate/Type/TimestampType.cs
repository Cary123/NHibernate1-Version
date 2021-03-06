using System;
using System.Collections;
using System.Data;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// This is almost the exact same type as the DateTime except it can be used
	/// in the version column, stores it to the accuracy the Database supports, 
	/// and will default to the value of DateTime.Now if the value is null.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The value stored in the database depends on what your Data Provider is capable
	/// of storing.  So there is a possibility that the DateTime you save will not be
	/// the same DateTime you get back when you check DateTime.Equals(DateTime) because
	/// they will have their milliseconds off.
	/// </p>  
	/// <p>
	/// For example - MsSql Server 2000 is only accurate to 3.33 milliseconds.  So if 
	/// NHibernate writes a value of <c>01/01/98 23:59:59.995</c> to the Prepared Command, MsSql
	/// will store it as <c>1998-01-01 23:59:59.997</c>.
	/// </p>
	/// <p>
	/// Please review the documentation of your Database server.
	/// </p>
	/// </remarks>
	[Serializable]
	public class TimestampType : ValueTypeType, IVersionType, ILiteralType
	{
		/// <summary></summary>
		internal TimestampType() : base( new DateTimeSqlType() )
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
			return Convert.ToDateTime( rs[ index ] );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rs"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public override object Get( IDataReader rs, string name )
		{
			return Get( rs, rs.GetOrdinal( name ) );
		}

		/// <summary></summary>
		public override System.Type ReturnedClass
		{
			get { return typeof( DateTime ); }
		}

		/// <summary>
		/// Sets the value of this Type in the IDbCommand.
		/// </summary>
		/// <param name="st">The IDbCommand to add the Type's value to.</param>
		/// <param name="value">The value of the Type.</param>
		/// <param name="index">The index of the IDataParameter in the IDbCommand.</param>
		/// <remarks>
		/// No null values will be written to the IDbCommand for this Type. 
		/// </remarks>
		public override void Set( IDbCommand st, object value, int index )
		{
			IDataParameter parm = st.Parameters[ index ] as IDataParameter;

			if( !( value is DateTime ) )
			{
				parm.Value = DateTime.Now;
			}
			else
			{
				parm.Value = value;
			}
		}

		/// <summary></summary>
		public override string Name
		{
			get { return "Timestamp"; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public override string ToString( object val )
		{
			return ( ( DateTime ) val ).ToShortTimeString();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public override object FromStringValue( string xml )
		{
			return DateTime.Parse( xml );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public override bool Equals( object x, object y )
		{
			if( x == y )
			{
				return true;
			}
			if( x == null || y == null )
			{
				return false;
			}

			long xTime = ( ( DateTime ) x ).Ticks;
			long yTime = ( ( DateTime ) y ).Ticks;
			return xTime == yTime; //TODO: Fixup
		}

		/// <summary></summary>
		public override bool HasNiceEquals
		{
			get { return true; }
		}

		#region IVersionType Members

		/// <summary>
		/// 
		/// </summary>
		/// <param name="current"></param>
		/// <returns></returns>
		public object Next( object current )
		{
			return Seed;
		}

		/// <summary></summary>
		public object Seed
		{
			get { return DateTime.Now; }
		}

		public IComparer Comparator
		{
			get { return Comparer.DefaultInvariant; }
		}

		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public object StringToObject( string xml )
		{
			return DateTime.Parse( xml );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override string ObjectToSQLString( object value )
		{
			return "'" + value.ToString() + "'";
		}
	}
}