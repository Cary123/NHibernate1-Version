using System;
using System.Data;
using System.Text;
using NHibernate.SqlCommand;
using NHibernate.Util;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Driver
{
	/// <summary>
	/// Base class for the implementation of IDriver
	/// </summary>
	public abstract class DriverBase : IDriver
	{
		#region IDriver Members
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DriverBase));

		public abstract System.Type CommandType { get; }
		public abstract System.Type ConnectionType { get; }

		// TODO: this should be moved down to ReflectionBasedDriver, but not in 1.0.x
		public virtual IDbConnection CreateConnection()
		{
			return ( IDbConnection ) Activator.CreateInstance( ConnectionType );
		}

		// TODO: this should be moved down to ReflectionBasedDriver, but not in 1.0.x
		public virtual IDbCommand CreateCommand()
		{
			return ( IDbCommand ) Activator.CreateInstance( CommandType );
		}

		public abstract bool UseNamedPrefixInSql { get; }

		public abstract bool UseNamedPrefixInParameter { get; }

		public abstract string NamedPrefix { get; }

		public string FormatNameForSql( string parameterName )
		{
			return UseNamedPrefixInSql ? ( NamedPrefix + parameterName ) : StringHelper.SqlParameter;
		}

		public string FormatNameForSql( string tableAlias, string parameterName )
		{
			if( !UseNamedPrefixInSql )
			{
				return StringHelper.SqlParameter;
			}


			if( tableAlias != null && tableAlias.Length > 0 )
			{
				return NamedPrefix + tableAlias + parameterName;
			}
			else
			{
				return NamedPrefix + parameterName;
			}
		}

		public string FormatNameForParameter( string parameterName )
		{
			return UseNamedPrefixInParameter ? ( NamedPrefix + parameterName ) : parameterName;
		}

		public string FormatNameForParameter( string tableAlias, string parameterName )
		{
			if( !UseNamedPrefixInParameter )
			{
				return parameterName;
			}


			if( tableAlias != null && tableAlias.Length > 0 )
			{
				return NamedPrefix + tableAlias + parameterName;
			}
			else
			{
				return NamedPrefix + parameterName;
			}
		}

		public virtual bool SupportsMultipleOpenReaders
		{
			get { return true; }
		}

		public virtual bool SupportsPreparingCommands
		{
			get { return true; }
		}

		public virtual IDbCommand GenerateCommand( Dialect.Dialect dialect, SqlString sqlString )
		{
			int paramIndex = 0;
			IDbCommand cmd = this.CreateCommand();

			object envTimeout = Environment.Properties[ Environment.CommandTimeout ];
			if( envTimeout != null )
			{
				int timeout = Convert.ToInt32( envTimeout );
				if( timeout > 0 )
				{
					if( log.IsDebugEnabled )
					{
						log.Debug( string.Format( "setting ADO Command timeout to '{0}' seconds", timeout) );
					}
					try
					{
						cmd.CommandTimeout = timeout;
					}
					catch( Exception e )
					{
						if( log.IsWarnEnabled )
						{
							log.Warn( e.ToString() );
						}
					}
				}
			}

			StringBuilder builder = new StringBuilder( sqlString.SqlParts.Length*15 );
			for( int i = 0; i < sqlString.SqlParts.Length; i++ )
			{
				object part = sqlString.SqlParts[ i ];
				Parameter parameter = part as Parameter;

				if( parameter != null )
				{
					string paramName = "p" + paramIndex;
					builder.Append( this.FormatNameForSql( paramName ) );

					IDbDataParameter dbParam = GenerateParameter( cmd, paramName, parameter, dialect );

					cmd.Parameters.Add( dbParam );

					paramIndex++;
				}
				else
				{
					builder.Append( ( string ) part );
				}
			}

			cmd.CommandText = builder.ToString();

			return cmd;
		}

		public virtual IDbCommand GenerateCommand( Dialect.Dialect dialect, string sqlString )
		{
			IDbCommand cmd = this.CreateCommand();
			cmd.CommandText = sqlString;

			return cmd;
		}

		/// <summary>
		/// Generates an IDbDataParameter for the IDbCommand.  It does not add the IDbDataParameter to the IDbCommand's
		/// Parameter collection.
		/// </summary>
		/// <param name="command">The IDbCommand to use to create the IDbDataParameter.</param>
		/// <param name="name">The name to set for IDbDataParameter.Name</param>
		/// <param name="parameter">The Parameter to convert to an IDbDataParameter.</param>
		/// <param name="dialect">The Dialect to use for Default lengths if needed.</param>
		/// <returns>An IDbDataParameter ready to be added to an IDbCommand.</returns>
		/// <remarks>
		/// Drivers that require Size or Precision/Scale to be set before the IDbCommand is prepared should 
		/// override this method and use the info contained in the Parameter to set those value.  By default
		/// those values are not set, only the DbType and ParameterName are set.
		/// </remarks>
		protected virtual IDbDataParameter GenerateParameter( IDbCommand command, string name, Parameter parameter, Dialect.Dialect dialect )
		{
			if( name != null && parameter != null && parameter.SqlType == null )
			{
				throw new QueryException( String.Format( "No type assigned to parameter '{0}': be sure to set types for named parameters.", name ) );
			}
			IDbDataParameter dbParam = command.CreateParameter();
			dbParam.DbType = parameter.SqlType.DbType;

			dbParam.ParameterName = this.FormatNameForParameter( name );

			return dbParam;
		}

		#endregion
	}
}