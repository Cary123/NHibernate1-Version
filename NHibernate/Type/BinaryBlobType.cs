using System;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	///	Maps a System.Byte[] Property to an column that can store a BLOB.
	/// </summary>
	/// <remarks>
	/// This is only needed by DataProviders (SqlClient) that need to specify a Size for the
	/// IDbDataParameter.  Most DataProvider(Oralce) don't need to set the Size so a BinaryType
	/// would work just fine.
	/// </remarks>
	[Serializable]
	public class BinaryBlobType : BinaryType
	{
		/// <summary></summary>
		internal BinaryBlobType() : base( new BinaryBlobSqlType() )
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sqlType"></param>
		internal BinaryBlobType( BinarySqlType sqlType ) : base( sqlType )
		{
		}

		/// <summary></summary>
		public override string Name
		{
			get { return "BinaryBlob"; }
		}

	}
}