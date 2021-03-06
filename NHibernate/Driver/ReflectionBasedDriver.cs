using System;
using System.Data;

namespace NHibernate.Driver
{
	public abstract class ReflectionBasedDriver : DriverBase
	{
		private System.Type connectionType;
		private System.Type commandType;

		/// <summary>
		/// Initializes a new instance of <see cref="ReflectionBasedDriver" /> with
		/// type names that are loaded from the specified assembly.
		/// </summary>
		/// <param name="driverAssemblyName">Assembly to load the types from.</param>
		/// <param name="connectionTypeName">Connection type name.</param>
		/// <param name="commandTypeName">Command type name.</param>
		public ReflectionBasedDriver( string driverAssemblyName, string connectionTypeName, string commandTypeName )
		{
			// Try to get the types from an already loaded assembly
			connectionType = Util.ReflectHelper.TypeFromAssembly( connectionTypeName, driverAssemblyName );
			commandType    = Util.ReflectHelper.TypeFromAssembly( commandTypeName,    driverAssemblyName );

			if( connectionType == null || commandType == null )
			{
				throw new HibernateException(
					string.Format( "The IDbCommand and IDbConnection implementation in the assembly {0} could not be found.  "
					+ "Please ensure that the assembly {0} is in the Global Assembly Cache or in a location that NHibernate "
					+ "can use System.Type.GetType(string) to load the types from.", driverAssemblyName ) );
			}
		}

		public override System.Type ConnectionType
		{
			get { return connectionType; }
		}

		public override System.Type CommandType
		{
			get { return commandType; }
		}

		public override IDbConnection CreateConnection()
		{
			return ( IDbConnection ) Activator.CreateInstance( ConnectionType );
		}

		public override IDbCommand CreateCommand()
		{
			return ( IDbCommand ) Activator.CreateInstance( CommandType );
		}

	}
}
