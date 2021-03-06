using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Iesi.Collections;
using log4net;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Persister;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Hql
{
	/// <summary> 
	/// An instance of <c>QueryTranslator</c> translates a Hibernate query string to SQL.
	/// </summary>
	public class QueryTranslator : Loader.Loader
	{
		private readonly string queryString;

		private readonly IDictionary typeMap = new SequencedHashMap();
		private readonly IDictionary collections = new SequencedHashMap();
		private IList returnedTypes = new ArrayList();
		private readonly IList fromTypes = new ArrayList();
		private readonly IList scalarTypes = new ArrayList();
		private readonly IDictionary namedParameters = new Hashtable();
		private readonly IDictionary aliasNames = new Hashtable();
		private readonly IDictionary oneToOneOwnerNames = new Hashtable();
		private readonly ISet crossJoins = new HashedSet();
		private readonly IDictionary decoratedPropertyMappings = new Hashtable();

		private readonly IList scalarSelectTokens = new ArrayList(); // contains a List of strings
		private readonly IList whereTokens = new ArrayList(); // contains a List of strings containing Sql or SqlStrings
		private readonly IList havingTokens = new ArrayList();
		private readonly IDictionary joins = new SequencedHashMap();
		private readonly IList orderByTokens = new ArrayList();
		private readonly IList groupByTokens = new ArrayList();
		private readonly ISet querySpaces = new HashedSet();
		private readonly ISet entitiesToFetch = new HashedSet();

		private IQueryable[ ] persisters;
		private int[] owners;
		private string[ ] names;
		private bool[ ] includeInSelect;
		private int selectLength;
		private IType[ ] returnTypes;
		private IType[ ] actualReturnTypes;
		private string[ ][ ] scalarColumnNames;
		internal ISessionFactoryImplementor factory;
		private IDictionary tokenReplacements;
		private int nameCount = 0;
		private int parameterCount = 0;
		private bool distinct = false;
		private bool compiled; 
		private SqlString sqlString;
		private System.Type holderClass;
		private ConstructorInfo holderConstructor;
		private bool hasScalars;
		private bool shallowQuery;
		private QueryTranslator superQuery;
		private IQueryableCollection collectionPersister;

		private int collectionOwnerColumn = -1;
		private string collectionOwnerName;
		private string fetchName;

		private string[ ] suffixes;

		private static readonly ILog log = LogManager.GetLogger( typeof( QueryTranslator ) );

		/// <summary> 
		/// Construct a query translator
		/// </summary>
		/// <param name="queryString"></param>
		public QueryTranslator( string queryString )
		{
			this.queryString = queryString;
		}

		/// <summary>
		/// Compile a subquery
		/// </summary>
		/// <param name="superquery"></param>
		protected internal void Compile( QueryTranslator superquery )
		{
			this.factory = superquery.factory;
			this.tokenReplacements = superquery.tokenReplacements;
			this.superQuery = superquery;
			this.shallowQuery = true;

			Compile( );
		}

		/// <summary>
		/// Compile a "normal" query. This method may be called multiple
		/// times. Subsequent invocations are no-ops.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="replacements"></param>
		/// <param name="scalar"></param>
		[MethodImpl( MethodImplOptions.Synchronized )]
		public void Compile( ISessionFactoryImplementor factory, IDictionary replacements, bool scalar )
		{
			if( !Compiled )
			{
				this.factory = factory;
				this.tokenReplacements = replacements;
				this.shallowQuery = scalar;

				Compile( );
			}
		}

		/// <summary> 
		/// Compile the query (generate the SQL).
		/// </summary>
		protected void Compile( )
		{
			log.Debug( "compiling query" );
			try
			{
				ParserHelper.Parse(
					new PreprocessingParser( tokenReplacements ),
					queryString,
					ParserHelper.HqlSeparators,
					this );
				RenderSql();
			}
			catch( QueryException qe )
			{
				qe.QueryString = queryString;
				throw;
			}
			catch( MappingException )
			{
				throw;
			}
			catch( Exception e )
			{
				log.Debug( "unexpected query compilation problem", e );
				QueryException qe = new QueryException( "Incorrect query syntax", e );
				qe.QueryString = queryString;
				throw qe;
			}

			PostInstantiate();

			compiled = true;
		}

		public new object LoadSingleRow( IDataReader resultSet, ISessionImplementor session, QueryParameters queryParameters, bool returnProxies )
		{
			return base.LoadSingleRow( resultSet, session, queryParameters, returnProxies );
		}

		/// <summary>
		/// Persisters for the return values of a <c>Find</c> style query
		/// </summary>
		/// <remarks>
		/// The <c>Persisters</c> stored by QueryTranslator have to be <see cref="IQueryable"/>.  The
		/// <c>setter</c> will attempt to cast the <c>ILoadable</c> array passed in into an 
		/// <c>IQueryable</c> array.
		/// </remarks>
		protected override ILoadable[ ] Persisters
		{
			get { return persisters; }
			set { persisters = ( IQueryable[ ] ) value; }
		}

		/// <summary>
		///Types of the return values of an <c>Enumerate()</c> style query.
		///Return an array of <see cref="IType" />s.
		/// </summary>
		public virtual IType[ ] ReturnTypes
		{
			get { return returnTypes; }
		}

		public virtual string[ ][ ] ScalarColumnNames
		{
			get { return scalarColumnNames; }
		}

		private void LogQuery( string hql, string sql )
		{
			if( log.IsDebugEnabled )
			{
				log.Debug( "HQL: " + hql );
				log.Debug( "SQL: " + sql );
			}
		}

		internal void SetAliasName( string alias, string name )
		{
			aliasNames.Add( alias, name );
		}

		internal string GetAliasName( String alias )
		{
			String name = ( String ) aliasNames[ alias ];
			if( name == null )
			{
				if( superQuery != null )
				{
					name = superQuery.GetAliasName( alias );
				}
				else
				{
					name = alias;
				}
			}
			return name;
		}

		internal string Unalias( string path )
		{
			string alias = StringHelper.Root( path );
			string name = GetAliasName( alias );
			if( name != null )
			{
				return name + path.Substring( alias.Length );
			}
			else
			{
				return path;
			}
		}

		public void AddEntityToFetch( string name, string oneToOneOwnerName )
		{
			AddEntityToFetch( name );
			if ( oneToOneOwnerName != null )
			{
				oneToOneOwnerNames.Add( name, oneToOneOwnerName );
			}
		}

		public void AddEntityToFetch( string name )
		{
			entitiesToFetch.Add( name );
		}

		/// <summary></summary>
		protected internal override SqlString SqlString
		{
			// this needs internal access because the WhereParser needs to be able to "get" it.
			get { return sqlString; }
			set
			{
				throw new NotSupportedException( "SqlString can not be set by class outside of QueryTranslator" );
			}
		}

		private int NextCount()
		{
			return ( superQuery == null ) ? nameCount++ : superQuery.nameCount++;
		}

		internal string CreateNameFor( System.Type type )
		{
			return GenerateAlias( type.Name, NextCount() );
		}

		internal string CreateNameForCollection( string role )
		{
			return GenerateAlias( role, NextCount() );
		}

		internal System.Type GetType( string name )
		{
			System.Type type = ( System.Type ) typeMap[ name ];
			if( type == null && superQuery != null )
			{
				type = superQuery.GetType( name );
			}
			return type;
		}

		internal string GetRole( string name )
		{
			string role = ( string ) collections[ name ];
			if( role == null && superQuery != null )
			{
				role = superQuery.GetRole( name );
			}
			return role;
		}

		internal bool IsName( string name )
		{
			return aliasNames.Contains( name ) ||
				typeMap.Contains( name ) ||
				collections.Contains( name ) ||
				( superQuery != null && superQuery.IsName( name ) );
		}

		public IPropertyMapping GetPropertyMapping( string name )
		{
			IPropertyMapping decorator = GetDecoratedPropertyMapping( name );
			if ( decorator != null )
			{
				return decorator; 
			}

			System.Type type = GetType( name );
			if ( type == null )
			{
				string role = GetRole( name );
				if ( role == null )
				{
					throw new QueryException( string.Format( "alias not found: {0}", name ) );
				}
				return GetCollectionPersister( role );
			}
			else
			{
				IQueryable persister = GetPersister( type );
				if ( persister == null )
				{
					throw new QueryException( string.Format( "persistent class not found: {0}", type.Name ) );
				}
				return persister;
			}
		}

		public IPropertyMapping GetDecoratedPropertyMapping( string name )
		{
			return (IPropertyMapping) decoratedPropertyMappings[name];
		}

		public void DecoratePropertyMapping( string name, IPropertyMapping mapping )
		{
			decoratedPropertyMappings.Add( name, mapping );
		}

		internal IQueryable GetPersisterForName( string name )
		{
			System.Type type = GetType( name );
			IQueryable persister = GetPersister( type );
			if ( persister == null )
			{
				throw new QueryException( "persistent class not found: " + type.Name );
			}

			return persister;
		}

		internal IQueryable GetPersisterUsingImports( string className )
		{
			// Slightly altered from H2.1 to avoid needlessly throwing
			// and catching a MappingException.
			return ( IQueryable ) factory.GetPersister(
				factory.GetImportedClassName( className ),
				false );
		}

		internal IQueryable GetPersister( System.Type clazz )
		{
			try
			{
				return ( IQueryable ) factory.GetPersister( clazz );
			}
			catch( Exception )
			{
				throw new QueryException( "persistent class not found: " + clazz.Name );
			}
		}

		internal IQueryableCollection GetCollectionPersister( string role )
		{
			try
			{
				return (IQueryableCollection) factory.GetCollectionPersister( role );
			}
			catch( InvalidCastException )
			{
				throw new QueryException( string.Format( "collection role is not queryable: {0}", role ) );
			}
			catch( Exception )
			{
				throw new QueryException( string.Format( "collection role not found: {0}", role ) );
			}
		}

		internal void AddType( string name, System.Type type )
		{
			typeMap.Add( name, type );
		}

		internal void AddCollection( string name, string role )
		{
			collections.Add( name, role );
		}

		internal void AddFrom( string name, System.Type type, JoinFragment join )
		{
			AddType( name, type );
			AddFrom( name, join );
		}

		internal void AddFromCollection( string name, string collectionRole, JoinFragment join )
		{
			//register collection role
			AddCollection( name, collectionRole );
			AddJoin( name, join );
		}

		internal void AddFrom( string name, JoinFragment join )
		{
			fromTypes.Add( name );
			AddJoin( name, join );
		}

		internal void AddFromClass( string name, IQueryable classPersister )
		{
			JoinFragment ojf = CreateJoinFragment( false );
			ojf.AddCrossJoin( classPersister.TableName, name );
			crossJoins.Add( name );
			AddFrom( name, classPersister.MappedClass, ojf );
		}

		internal void AddSelectClass( string name )
		{
			returnedTypes.Add( name );
		}

		internal void AddSelectScalar( IType type )
		{
			scalarTypes.Add( type );
		}

		internal void AppendWhereToken( string token )
		{
			whereTokens.Add( token );
		}

		internal void AppendWhereToken( SqlString token )
		{
			whereTokens.Add( token );
		}

		internal void AppendHavingToken( string token )
		{
			havingTokens.Add( token );
		}

		internal void AppendOrderByToken( string token )
		{
			orderByTokens.Add( token );
		}

		internal void AppendGroupByToken( string token )
		{
			groupByTokens.Add( token );
		}

		internal void AppendScalarSelectToken( string token )
		{
			scalarSelectTokens.Add( token );
		}

		internal void AppendScalarSelectTokens( string[ ] tokens )
		{
			scalarSelectTokens.Add( tokens );
		}

		internal void AddJoin( string name, JoinFragment newjoin )
		{
			JoinFragment oldjoin = ( JoinFragment ) joins[ name ];
			if( oldjoin == null )
			{
				joins.Add( name, newjoin );
			}
			else
			{
				oldjoin.AddCondition( newjoin.ToWhereFragmentString );
				//TODO: HACKS with ToString()
				if( oldjoin.ToFromFragmentString.ToString().IndexOf( newjoin.ToFromFragmentString.Trim().ToString() ) < 0 )
				{
					throw new AssertionFailure( "bug in query parser: " + queryString );
					//TODO: what about the toFromFragmentString() ????
				}
			}
		}

		internal void AddNamedParameter( string name )
		{
			if( superQuery != null )
			{
				superQuery.AddNamedParameter( name );
			}

			// want the param index to start at 0 instead of 1
			//int loc = ++parameterCount;
			int loc = parameterCount++;
			object o = namedParameters[ name ];
			if( o == null )
			{
				namedParameters.Add( name, loc );
			}
			else if( o is int )
			{
				ArrayList list = new ArrayList( 4 );
				list.Add( o );
				list.Add( loc );
				namedParameters[ name ] = list;
			}
			else
			{
				( ( ArrayList ) o ).Add( loc );
			}
		}

		internal int[ ] GetNamedParameterLocs( string name )
		{
			object o = namedParameters[ name ];
			if( o == null )
			{
				QueryException qe = new QueryException( "Named parameter does not appear in Query: " + name );
				qe.QueryString = queryString;
				throw qe;
			}
			if( o is int )
			{
				return new int[ ] {( ( int ) o )};
			}
			else
			{
				return ArrayHelper.ToIntArray( ( ArrayList ) o );
			}
		}

		public static string ScalarName( int x, int y )
		{
			return new StringBuilder()
				.Append( 'x' )
				.Append( x )
				.Append( StringHelper.Underscore )
				.Append( y )
				.Append( StringHelper.Underscore )
				.ToString();
		}

		private void RenderSql()
		{
			int rtsize;
			if( returnedTypes.Count == 0 && scalarTypes.Count == 0 )
			{
				//ie no select clause in HQL
				returnedTypes = fromTypes;
				rtsize = returnedTypes.Count;
			}
			else
			{
				rtsize = returnedTypes.Count;
				foreach( string entityName in entitiesToFetch )
				{
					returnedTypes.Add( entityName );
				}
			}

			int size = returnedTypes.Count;
			persisters = new IQueryable[size];
			names = new string[size];
			owners = new int[size];
			suffixes = new string[size];
			includeInSelect = new bool[size];
			for( int i = 0; i < size; i++ )
			{
				string name = ( string ) returnedTypes[ i ];
				//if ( !IsName(name) ) throw new QueryException("unknown type: " + name);
				persisters[ i ] = GetPersisterForName( name );
				suffixes[ i ] = ( size == 1 ) ? String.Empty : i.ToString() + StringHelper.Underscore;
				names[ i ] = name;
				includeInSelect[ i ] = !entitiesToFetch.Contains( name );
				if( includeInSelect[ i ] )
				{
					selectLength++;
				}
				if( name.Equals( collectionOwnerName ) )
				{
					collectionOwnerColumn = i;
				}
				string oneToOneOwner = (string) oneToOneOwnerNames[ name ];
				owners[ i ] = oneToOneOwner == null ? -1 : returnedTypes.IndexOf( oneToOneOwner );
			}

			if ( ArrayHelper.IsAllNegative( owners ) )
			{
				owners = null;
			}

			string scalarSelect = RenderScalarSelect(); //Must be done here because of side-effect! yuck...

			int scalarSize = scalarTypes.Count;
			hasScalars = scalarTypes.Count != rtsize;

			returnTypes = new IType[scalarSize];
			for( int i = 0; i < scalarSize; i++ )
			{
				returnTypes[ i ] = ( IType ) scalarTypes[ i ];
			}

			QuerySelect sql = new QuerySelect( factory.Dialect );
			sql.Distinct = distinct;

			if( !shallowQuery )
			{
				RenderIdentifierSelect( sql );
				RenderPropertiesSelect( sql );
			}

			if( CollectionPersister != null )
			{
				sql.AddSelectFragmentString( collectionPersister.SelectFragment( fetchName ) );
			}
			if( hasScalars || shallowQuery )
			{
				sql.AddSelectFragmentString( scalarSelect );
			}

			// TODO: for some dialects it would be appropriate to add the renderOrderByPropertiesSelect() to other select strings
			MergeJoins( sql.JoinFragment );

			sql.SetWhereTokens( whereTokens );

			sql.SetGroupByTokens( groupByTokens );
			sql.SetHavingTokens( havingTokens );
			sql.SetOrderByTokens( orderByTokens );

			if( collectionPersister != null && collectionPersister.HasOrdering )
			{
				sql.AddOrderBy( collectionPersister.GetSQLOrderByString( fetchName ) );
			}

			scalarColumnNames = GenerateColumnNames( returnTypes, factory );

			// initialize the set of queried identifer spaces (ie. tables)
			foreach( string name in collections.Values )
			{
				IQueryableCollection p = GetCollectionPersister( name );
				AddQuerySpace( p.CollectionSpace );
			}
			foreach( string name in typeMap.Keys )
			{
				IQueryable p = GetPersisterForName( name );
				object[] spaces = p.PropertySpaces;
				for ( int i=0; i < spaces.Length; i++ )
				{
					AddQuerySpace( spaces[ i ] );
				}
			}

			sqlString = sql.ToQuerySqlString();

			try
			{
				if( holderClass != null )
				{
					holderConstructor = ReflectHelper.GetConstructor( holderClass, returnTypes );
				}
			}
			catch( Exception nsme )
			{
				throw new QueryException( "could not find constructor for: " + holderClass.Name, nsme );
			}

			if ( hasScalars )
			{
				actualReturnTypes = returnTypes;
			}
			else
			{
				actualReturnTypes = new IType[ selectLength ];
				int j = 0;
				for( int i = 0; i < persisters.Length; i++ )
				{
					if ( includeInSelect[ i ] )
					{
						actualReturnTypes[ j++ ] = NHibernateUtil.Entity( persisters[ i ].MappedClass ) ;
					}
				}
			}
		}

		private void RenderIdentifierSelect( QuerySelect sql )
		{
			int size = returnedTypes.Count;

			for( int k = 0; k < size; k++ )
			{
				string name = ( string ) returnedTypes[ k ];
				string suffix = size == 1 ? String.Empty : k.ToString() + StringHelper.Underscore;
				sql.AddSelectFragmentString( persisters[ k ].IdentifierSelectFragment( name, suffix ) );
			}
		}

		private void RenderPropertiesSelect( QuerySelect sql )
		{
			int size = returnedTypes.Count;
			for( int k = 0; k < size; k++ )
			{
				string suffix = ( size == 1 ) ? String.Empty : k.ToString() + StringHelper.Underscore;
				string name = ( string ) returnedTypes[ k ];
				sql.AddSelectFragmentString( persisters[ k ].PropertySelectFragment( name, suffix ) );
			}
		}

		/// <summary> 
		/// WARNING: side-effecty
		/// </summary>
		private string RenderScalarSelect()
		{
			bool isSubselect = superQuery != null;

			StringBuilder buf = new StringBuilder( 20 );

			if( scalarTypes.Count == 0 )
			{
				//ie. no select clause
				int size = returnedTypes.Count;
				for( int k = 0; k < size; k++ )
				{
					scalarTypes.Add( NHibernateUtil.Entity( persisters[ k ].MappedClass ) );

					string[ ] names = persisters[ k ].IdentifierColumnNames;
					for( int i = 0; i < names.Length; i++ )
					{
						buf.Append( returnedTypes[ k ] ).Append( StringHelper.Dot ).Append( names[ i ] );
						if( !isSubselect )
						{
							buf.Append( " as " ).Append( ScalarName( k, i ) );
						}
						if( i != names.Length - 1 || k != size - 1 )
						{
							buf.Append( StringHelper.CommaSpace );
						}
					}
				}
			}
			else
			{
				//there _was_ a select clause
				int c = 0;
				bool nolast = false; //real hacky...
				int parenCount = 0;  // used to count the nesting of parentheses
				foreach( object next in scalarSelectTokens )
				{
					if( next is string )
					{
						string token = ( string ) next;
						string lc = token.ToLower( System.Globalization.CultureInfo.InvariantCulture );

						if( StringHelper.OpenParen.Equals( token ) )
						{
							parenCount++;
						}
						else if( StringHelper.ClosedParen.Equals( token ) )
						{
							parenCount--;
						}
						else if( lc.Equals( StringHelper.CommaSpace ) )
						{
							if( nolast )
							{
								nolast = false;
							}
							else
							{
								if( !isSubselect && parenCount == 0 )
								{
									buf.Append( " as " ).Append( ScalarName( c++, 0 ) );
								}
							}
						}

						buf.Append( token );
						if( lc.Equals( "distinct" ) || lc.Equals( "all" ) )
						{
							buf.Append( ' ' );
						}
					}
					else
					{
						nolast = true;
						string[ ] tokens = ( string[ ] ) next;
						for( int i = 0; i < tokens.Length; i++ )
						{
							buf.Append( tokens[ i ] );
							if( !isSubselect )
							{
								buf.Append( " as " ).Append( ScalarName( c, i ) );
							}
							if( i != tokens.Length - 1 )
							{
								buf.Append( StringHelper.CommaSpace );
							}
						}
						c++;
					}
				}
				if( !isSubselect && !nolast )
				{
					buf.Append( " as " ).Append( ScalarName( c++, 0 ) );
				}
			}

			return buf.ToString();
		}

		private void MergeJoins( JoinFragment ojf )
		{
			foreach( DictionaryEntry de in joins )
			{
				string name = (string)de.Key;
				JoinFragment join = (JoinFragment) de.Value;

				if ( typeMap.Contains( name ) ) 
				{
					IQueryable p = GetPersisterForName( name );
					bool includeSubclasses = returnedTypes.Contains( name )
						&& !IsShallowQuery;

					bool isCrossJoin = crossJoins.Contains( name );
					ojf.AddFragment( join );
					ojf.AddJoins(
						p.FromJoinFragment( name, isCrossJoin, includeSubclasses ),
						p.QueryWhereFragment( name, isCrossJoin, includeSubclasses )
						);

				}
				else if ( collections.Contains( name ) ) 
				{
					ojf.AddFragment(join);
				}
				else 
				{
					//name from a super query (a bit inelegant that it shows up here)
				}
			}
		}

		public ISet QuerySpaces
		{
			get { return querySpaces; }
		}

		/// <summary>
		/// Is this query called by Scroll() or Iterate()?
		/// </summary>
		/// <value>true if it is, false if it is called by find() or list()</value>
		public bool IsShallowQuery
		{
			get { return shallowQuery; }
		}

		internal void AddQuerySpace( object table )
		{
			querySpaces.Add( table );
			if( superQuery != null )
			{
				superQuery.AddQuerySpace( table );
			}
		}

		internal bool Distinct
		{
			set { distinct = value; }
		}

		/// <summary></summary>
		public bool IsSubquery
		{
			get { return superQuery != null; }
		}

		protected override ICollectionPersister CollectionPersister
		{
			get { return collectionPersister; }
		}

		public void SetCollectionToFetch( string role, string name, string ownerName, string entityName )
		{
			if( fetchName != null )
			{
				throw new QueryException( "cannot fetch multiple collections in one query" );
			}

			fetchName = name;
			collectionPersister = GetCollectionPersister( role );
			collectionOwnerName = ownerName;
			if ( collectionPersister.ElementType.IsEntityType )
			{
				AddEntityToFetch( entityName );
			}
		}

		protected override string[ ] Suffixes
		{
			get { return suffixes; }
			set { suffixes = value; }
		}

		/// <remarks>Used for collection filters</remarks>
		protected void AddFromAssociation( string elementName, string collectionRole )
		{
			//q.addCollection(collectionName, collectionRole);
			IType collectionElementType = GetCollectionPersister( collectionRole ).ElementType;
			if( !collectionElementType.IsEntityType )
			{
				throw new QueryException( "collection of values in filter: " + elementName );
			}

			IQueryableCollection persister = GetCollectionPersister( collectionRole );
			string[ ] keyColumnNames = persister.KeyColumnNames;
			//if (keyColumnNames.Length!=1) throw new QueryException("composite-key collecion in filter: " + collectionRole);

			string collectionName;
			JoinFragment join = CreateJoinFragment( false );
			collectionName = persister.IsOneToMany ? elementName : CreateNameForCollection( collectionRole );
			join.AddCrossJoin( persister.TableName, collectionName );
			if( !persister.IsOneToMany )
			{
				//many-to-many
				AddCollection( collectionName, collectionRole );

				IQueryable p = (IQueryable) persister.ElementPersister;
				string[ ] idColumnNames = p.IdentifierColumnNames;
				string[ ] eltColumnNames = persister.ElementColumnNames;
				join.AddJoin(
					p.TableName,
					elementName,
					StringHelper.Qualify( collectionName, eltColumnNames ),
					idColumnNames,
					JoinType.InnerJoin );
			}
			join.AddCondition( collectionName, keyColumnNames, " = ", persister.KeyType, Factory );
			if( persister.HasWhere )
			{
				join.AddCondition( persister.GetSQLWhereString( collectionName ) );
			}
			EntityType elmType = (EntityType) collectionElementType;
			AddFrom( elementName, elmType.AssociatedClass, join );
		}

		private IDictionary pathAliases = new Hashtable();
		private IDictionary pathJoins = new Hashtable();

		internal string GetPathAlias( string path )
		{
			return ( string ) pathAliases[ path ];
		}

		internal JoinFragment GetPathJoin( string path )
		{
			return ( JoinFragment ) pathJoins[ path ];
		}

		internal void AddPathAliasAndJoin( string path, string alias, JoinFragment join )
		{
			pathAliases.Add( path, alias );
			pathJoins.Add( path, join.Copy() );
		}

		protected override int BindNamedParameters( IDbCommand ps, IDictionary namedParams, int start, ISessionImplementor session )
		{
			if( namedParams != null )
			{
				// assumes that types are all of span 1
				int result = 0;
				foreach( DictionaryEntry e in namedParams )
				{
					string name = ( string ) e.Key;
					TypedValue typedval = ( TypedValue ) e.Value;
					int[ ] locs = GetNamedParameterLocs( name );
					for( int i = 0; i < locs.Length; i++ )
					{
						// Hack: parametercollection starts at 0
						//typedval.Type.NullSafeSet(ps, typedval.Value, Impl.AdoHack.ParameterPos(locs[i] + start), session);
						typedval.Type.NullSafeSet( ps, typedval.Value, ( locs[ i ] + start ), session );
						// end-of Hack
					}
					result += locs.Length;
				}
				return result;
			}
			else
			{
				return 0;
			}
		}

		public IList List( ISessionImplementor session, QueryParameters queryParameters )
		{
			LogQuery( queryString, sqlString.ToString() );

			// NH: added this call to initialize parameter types in SqlString
			// so that it gets cached and looked up properly in the call to List
			PopulateSqlString( queryParameters );

			return List( session, queryParameters, QuerySpaces, actualReturnTypes );
		}

		public IEnumerable GetEnumerable( QueryParameters parameters, ISessionImplementor session )
		{
			LogQuery( queryString, sqlString.ToString() );
			
			// NH: added this call to initialize parameter types in SqlString
			PopulateSqlString( parameters );

			SqlString sqlWithLock = ApplyLocks( SqlString, parameters.LockModes, session.Factory.Dialect );

			IDbCommand st = PrepareQueryCommand(
				sqlWithLock,
				parameters, false, session );

			// This IDataReader is disposed of in EnumerableImpl.Dispose
			IDataReader rs = GetResultSet( st, parameters.RowSelection, session );
			return new EnumerableImpl( rs, st, session, ReturnTypes, ScalarColumnNames, parameters.RowSelection,
				holderClass );
		}

		public static string[ ] ConcreteQueries( string query, ISessionFactoryImplementor factory )
		{
			//scan the query string for class names appearing in the from clause and replace 
			//with all persistent implementors of the class/interface, returning multiple 
			//query strings (make sure we don't pick up a class in the select clause!) 

			//TODO: this is one of the ugliest and most fragile pieces of code in Hibernate...
			string[ ] tokens = StringHelper.Split( ParserHelper.Whitespace + "(),", query, true );
			if( tokens.Length == 0 )
			{
				return new String[ ] {query};
			} // just especially for the trivial collection filter 

			ArrayList placeholders = new ArrayList();
			ArrayList replacements = new ArrayList();
			StringBuilder templateQuery = new StringBuilder( 40 );
			int count = 0;
			string last = null;
			int nextIndex = 0;
			string next = null;
			templateQuery.Append( tokens[ 0 ] );
			for( int i = 1; i < tokens.Length; i++ )
			{
				//update last non-whitespace token, if necessary
				if( !ParserHelper.IsWhitespace( tokens[ i - 1 ] ) )
				{
					last = tokens[ i - 1 ].ToLower( System.Globalization.CultureInfo.InvariantCulture );
				}

				string token = tokens[ i ];
				if( !ParserHelper.IsWhitespace( token ) || last == null )
				{
					// scan for the next non-whitespace token
					if( nextIndex <= i )
					{
						for( nextIndex = i + 1; nextIndex < tokens.Length; nextIndex++ )
						{
							next = tokens[ nextIndex ].ToLower( System.Globalization.CultureInfo.InvariantCulture );
							if( !ParserHelper.IsWhitespace( next ) )
							{
								break;
							}
						}
					}

					//if ( Character.isUpperCase( token.charAt( token.lastIndexOf(".") + 1 ) ) ) {
					// added the checks for last!=null and next==null because an ISet can not contain 
					// a null key in .net - it is valid for a null key to be in a java.util.Set
					if(
						( ( last != null && beforeClassTokens.Contains( last ) ) && ( next == null || !notAfterClassTokens.Contains( next ) ) ) ||
							"class".Equals( last ) )
					{
						System.Type clazz = GetImportedClass( token, factory );
						if( clazz != null )
						{
							string[ ] implementors = factory.GetImplementors( clazz );
							string placeholder = "$clazz" + count++ + "$";

							if( implementors != null )
							{
								placeholders.Add( placeholder );
								replacements.Add( implementors );
							}
							token = placeholder; //Note this!!
						}
					}
				}
				templateQuery.Append( token );

			}
			string[ ] results = StringHelper.Multiply( templateQuery.ToString(), placeholders.GetEnumerator(), replacements.GetEnumerator() );
			if( results.Length == 0 )
			{
				log.Warn( "no persistent classes found for query class: " + query );
			}
			return results;
		}


		private static readonly ISet beforeClassTokens = new HashedSet();
		private static readonly ISet notAfterClassTokens = new HashedSet();

		/// <summary></summary>
		static QueryTranslator()
		{
			beforeClassTokens.Add( "from" );
			//beforeClassTokens.Add("new"); DEFINITELY DON'T HAVE THIS!!
			beforeClassTokens.Add( "," );

			notAfterClassTokens.Add( "in" );
			//notAfterClassTokens.Add(",");
			notAfterClassTokens.Add( "from" );
			notAfterClassTokens.Add( ")" );
		}

		/// <summary>
		/// Gets the Type for the name that might be an Imported Class.
		/// </summary>
		/// <param name="name">The name that might be an ImportedClass.</param>
		/// <returns>A <see cref="System.Type"/> if <c>name</c> is an Imported Class, <c>null</c> otherwise.</returns>
		internal System.Type GetImportedClass( string name )
		{
			return GetImportedClass( name, factory );
		}

		/// <summary>
		/// Gets the Type for the name that might be an Imported Class.
		/// </summary>
		/// <param name="name">The name that might be an ImportedClass.</param>
		/// <param name="factory">The <see cref="ISessionFactoryImplementor"/> that contains the Imported Classes.</param>
		/// <returns>A <see cref="System.Type"/> if <c>name</c> is an Imported Class, <c>null</c> otherwise.</returns>
		private static System.Type GetImportedClass( string name, ISessionFactoryImplementor factory )
		{
			string importedName = factory.GetImportedClassName( name );

			// don't care about the exception, just give us a null value.
			return System.Type.GetType( importedName, false );
		}

		private static string[ ][ ] GenerateColumnNames( IType[ ] types, ISessionFactoryImplementor f )
		{
			string[ ][ ] names = new string[types.Length][ ];
			for( int i = 0; i < types.Length; i++ )
			{
				int span = types[ i ].GetColumnSpan( f );
				names[ i ] = new string[span];
				for( int j = 0; j < span; j++ )
				{
					names[ i ][ j ] = ScalarName( i, j );
				}
			}
			return names;
		}

		protected override object GetResultColumnOrRow( object[ ] row, IDataReader rs, ISessionImplementor session )
		{
			IType[ ] returnTypes = ReturnTypes;
			row = ToResultRow( row );
			if( hasScalars )
			{
				string[ ][ ] names = ScalarColumnNames;
				int queryCols = returnTypes.Length;
				if( holderClass == null && queryCols == 1 )
				{
					return returnTypes[ 0 ].NullSafeGet( rs, names[ 0 ], session, null );
				}
				else
				{
					row = new object[queryCols];
					for( int i = 0; i < queryCols; i++ )
					{
						row[ i ] = returnTypes[ i ].NullSafeGet( rs, names[ i ], session, null );
					}
					return row;
				}
			}
			else if( holderClass == null )
			{
				return ( row.Length == 1 ) ? row[ 0 ] : row;
			}
			else
			{
				return row;
			}
		}

		protected override IList GetResultList(IList results)
		{
			if( holderClass != null )
			{
				for( int i = 0; i < results.Count; i++ )
				{
					object[] row = (object[]) results[i];
					try
					{
						results[i] = holderConstructor.Invoke( row );
					}
					catch( Exception e)
					{
						throw new QueryException( "could not instantiate: " + holderClass, e );
					}
				}
			}

			return results;
		}

		private object[ ] ToResultRow( object[ ] row )
		{
			if( selectLength == row.Length )
			{
				return row;
			}
			else
			{
				object[ ] result = new object[selectLength];
				int j = 0;
				for( int i = 0; i < row.Length; i++ )
				{
					if( includeInSelect[ i ] )
					{
						result[ j++ ] = row[ i ];
					}
				}
				return result;
			}
		}

		internal QueryJoinFragment CreateJoinFragment( bool useThetaStyleInnerJoins )
		{
			return new QueryJoinFragment( factory.Dialect, useThetaStyleInnerJoins );
		}

		internal System.Type HolderClass
		{
			set { holderClass = value; }
		}

		protected override LockMode[ ] GetLockModes( IDictionary lockModes )
		{
			// unfortunately this stuff can't be cached because
			// it is per-invocation, not constant for the
			// QueryTranslator instance
			IDictionary nameLockModes = new Hashtable();
			if( lockModes != null )
			{
				IDictionaryEnumerator it = lockModes.GetEnumerator();
				while( it.MoveNext() )
				{
					DictionaryEntry me = it.Entry;
					nameLockModes.Add(
						GetAliasName( ( String ) me.Key ),
						me.Value
						);
				}
			}
			LockMode[ ] lockModeArray = new LockMode[names.Length];
			for( int i = 0; i < names.Length; i++ )
			{
				LockMode lm = ( LockMode ) nameLockModes[ names[ i ] ];
				if( lm == null )
				{
					lm = LockMode.None;
				}
				lockModeArray[ i ] = lm;
			}
			return lockModeArray;
		}

		protected override SqlString ApplyLocks( SqlString sql, IDictionary lockModes, Dialect.Dialect dialect )
		{
			if( lockModes == null || lockModes.Count == 0 )
			{
				return sql;
			}
			else 
			{
				IDictionary aliasedLockModes = new Hashtable();
				IEnumerator keys = lockModes.Keys.GetEnumerator();
				object key;
				while ( keys.MoveNext() ) 
				{
					key = keys.Current;
					aliasedLockModes.Add( GetAliasName( (String)  key ), lockModes[key] );
				}
				return sql.Append(new ForUpdateFragment(aliasedLockModes).ToSqlStringFragment(dialect));
			}
		}

		protected override bool UpgradeLocks()
		{
			return true;
		}

		protected override int CollectionOwner
		{
			get { return collectionOwnerColumn; }
		}

		protected internal ISessionFactoryImplementor Factory
		{
			set { this.factory = value; }
			get { return factory; }
		}

		protected bool Compiled
		{
			get { return compiled; }
		}

		public override string ToString()
		{
			return queryString;
		}

		/// <summary></summary>
		protected override int[] Owners
		{
			get { return owners; }
			set { owners = value; }
		}

		/// <summary>
		/// Indicates if the SqlString has been fully populated - it goes
		/// through a 2 phase process.  The first part is the parsing of the
		/// hql and it puts in placeholders for the parameters, the second phase 
		/// puts in the actual types for the parameters using QueryParameters
		/// passed to query methods.  The completion of the second phase is
		/// when <c>isSqlStringPopulated==true</c>.
		/// </summary>
		private bool isSqlStringPopulated;

		private object prepareCommandLock = new object();

		private void PopulateSqlString( QueryParameters parameters )
		{
			lock( prepareCommandLock )
			{
				if( isSqlStringPopulated )
				{
					return;
				}

				SqlString sql = null;

				// when there is no untyped Parameters then we can avoid the need to create
				// a new sql string and just return the existing one because it is ready 
				// to be prepared and executed.
				if( sqlString.ContainsUntypedParameter == false )
				{
					sql = sqlString;
				}
				else
				{
					// holds the index of the sqlPart that should be replaced
					int sqlPartIndex = 0;

					// holds the index of the paramIndexes array that is the current position
					int paramIndex = 0;

					sql = sqlString.Clone();
					int[ ] paramIndexes = sql.ParameterIndexes;

					// if there are no Parameters in the SqlString then there is no reason to 
					// bother with this code.
					if( paramIndexes.Length > 0 )
					{
						for( int i = 0; i < parameters.PositionalParameterTypes.Length; i++ )
						{
							string[ ] colNames = new string[parameters.PositionalParameterTypes[ i ].GetColumnSpan( factory )];
							for( int j = 0; j < colNames.Length; j++ )
							{
								colNames[ j ] = "p" + paramIndex.ToString() + j.ToString();
							}

							Parameter[ ] sqlParameters = Parameter.GenerateParameters( factory, colNames, parameters.PositionalParameterTypes[ i ] );

							foreach( Parameter param in sqlParameters )
							{
								sqlPartIndex = paramIndexes[ paramIndex ];
								sql.SqlParts[ sqlPartIndex ] = param;

								paramIndex++;
							}
						}

						if( parameters.NamedParameters != null && parameters.NamedParameters.Count > 0 )
						{
							// convert the named parameters to an array of types
							ArrayList paramTypeList = new ArrayList();

							foreach( DictionaryEntry e in parameters.NamedParameters )
							{
								string name = ( string ) e.Key;
								TypedValue typedval = ( TypedValue ) e.Value;
								int[ ] locs = GetNamedParameterLocs( name );

								for( int i = 0; i < locs.Length; i++ )
								{
									int lastInsertedIndex = paramTypeList.Count;

									int insertAt = locs[ i ];

									// need to make sure that the ArrayList is populated with null objects
									// up to the index we are about to add the values into.  An Index Out 
									// of Range exception will be thrown if we add an element at an index
									// that is greater than the Count.
									if( insertAt >= lastInsertedIndex )
									{
										for( int j = lastInsertedIndex; j <= insertAt; j++ )
										{
											paramTypeList.Add( null );
										}
									}

									paramTypeList[ insertAt ] = typedval.Type;
								}
							}

							for( int i = 0; i < paramTypeList.Count; i++ )
							{
								IType type = ( IType ) paramTypeList[ i ];
								string[ ] colNames = new string[type.GetColumnSpan( factory )];

								for( int j = 0; j < colNames.Length; j++ )
								{
									colNames[ j ] = "p" + paramIndex.ToString() + j.ToString();
								}

								Parameter[ ] sqlParameters = Parameter.GenerateParameters( factory, colNames, type );

								foreach( Parameter param in sqlParameters )
								{
									sqlPartIndex = paramIndexes[ paramIndex ];
									sql.SqlParts[ sqlPartIndex ] = param;

									paramIndex++;
								}
							}
						}
					}
				}

				// replace the local field used by the SqlString property with the one we just built 
				// that has the correct parameters
				this.sqlString = sql;
				isSqlStringPopulated = true;
			}
		}
	}
}
