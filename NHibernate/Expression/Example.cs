using System;
using System.Collections;
using Iesi.Collections;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.SqlCommand;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Expression
{
	/// <summary>
	/// Support for <c>Query By Example</c>.
	/// </summary>
	public class Example : AbstractCriterion
	{
		private readonly object _entity;
		private readonly ISet _excludedProperties = new HashedSet();
		private IPropertySelector _selector;
		private bool _isLikeEnabled;
		private bool _isIgnoreCaseEnabled;
		private MatchMode _matchMode;

		/// <summary>
		/// A strategy for choosing property values for inclusion in the query criteria
		/// </summary>
		public interface IPropertySelector
		{
			/// <summary>
			/// Determine if the Property should be included.
			/// </summary>
			/// <param name="propertyValue">The value of the property that is being checked for inclusion.</param>
			/// <param name="propertyName">The name of the property that is being checked for inclusion.</param>
			/// <param name="type">The <see cref="IType"/> of the property.</param>
			/// <returns>
			/// <c>true</c> if the Property should be included in the Query, 
			/// <c>false</c> otherwise.
			/// </returns>
			bool Include(object propertyValue, String propertyName, IType type);
		}

		private static readonly IPropertySelector NotNull = new NotNullPropertySelector();
		private static readonly IPropertySelector NotNullOrEmptyString = new NotNullOrEmptyStringPropertySelector();
		private static readonly IPropertySelector All = new AllPropertySelector();
		private static readonly IPropertySelector NotNullOrZero = new NotNullOrZeroPropertySelector();

		/// <summary>
		/// Implementation of <see cref="IPropertySelector"/> that includes all
		/// properties regardless of value.
		/// </summary>
		private class AllPropertySelector : IPropertySelector
		{
			public bool Include(object propertyValue, String propertyName, IType type)
			{
				return true;
			}
		}

		private class NotNullPropertySelector : IPropertySelector
		{
			public bool Include( object propertyValue, string propertyName, IType type )
			{
				return propertyValue != null;
			}
		}

		private class NotNullOrZeroPropertySelector : IPropertySelector
		{
			private static bool IsZero( object value )
			{
				// Only try to check IConvertibles, to be able to handle various flavors
				// of nullable numbers, etc. Skip strings.
				if( value is IConvertible && !(value is string) )
				{
					try
					{
						return Convert.ToInt64( value ) == 0L;
					}
					catch( FormatException )
					{
						// Ignore
					}
					catch( InvalidCastException )
					{
						// Ignore
					}
				}

				return false;
			}

			public bool Include(object propertyValue, String propertyName, IType type)
			{
				return propertyValue != null && !IsZero( propertyValue );
			}
		}

		/// <summary>
		/// Implementation of <see cref="IPropertySelector"/> that includes the
		/// properties that are not <c>null</c> and do not have an <see cref="String.Empty"/>
		/// returned by <c>propertyValue.ToString()</c>.
		/// </summary>
		/// <remarks>
		/// This selector is not present in H2.1. It may be useful if nullable types
		/// are used for some properties.
		/// </remarks>
		private class NotNullOrEmptyStringPropertySelector : IPropertySelector
		{
			public bool Include(object propertyValue, String propertyName, IType type)
			{
				if( propertyValue != null )
				{
					return propertyValue.ToString().Length > 0;
				}
				else
				{
					return false;
				}

			}
		}

		/// <summary>
		/// Set the <see cref="IPropertySelector"/> for this <see cref="Example"/>.
		/// </summary>
		/// <param name="selector">The <see cref="IPropertySelector"/> to determine which properties to include.</param>
		/// <returns>This <see cref="Example"/> instance.</returns>
		/// <remarks>
		/// This should be used when a custom <see cref="IPropertySelector"/> has
		/// been implemented.  Otherwise use the methods <see cref="Example.ExcludeNulls"/> 
		/// or <see cref="Example.ExcludeNone"/> to set the <see cref="IPropertySelector"/>
		/// to the <see cref="IPropertySelector"/>s built into NHibernate.
		/// </remarks>
		public Example SetPropertySelector(IPropertySelector selector)
		{
			_selector = selector;
			return this;
		}

		/// <summary>
		/// Set the <see cref="IPropertySelector"/> for this <see cref="Example"/>
		/// to exclude zero-valued properties.
		/// </summary>
		public Example ExcludeZeroes()
		{
			return SetPropertySelector( NotNullOrZero );
		}

		/// <summary>
		/// Set the <see cref="IPropertySelector"/> for this <see cref="Example"/>
		/// to exclude no properties.
		/// </summary>
		public Example ExcludeNone()
		{
			SetPropertySelector( All );
			return this;
		}

		public Example ExcludeNulls()
		{
			SetPropertySelector( NotNullOrEmptyString );
			return this;
		}

		/// <summary>
		/// Use the "like" operator for all string-valued properties with
		/// the specified <see cref="MatchMode"/>.
		/// </summary>
		/// <param name="matchMode">
		/// The <see cref="MatchMode"/> to convert the string to the pattern
		/// for the <c>like</c> comparison.
		/// </param>
		public Example EnableLike(MatchMode matchMode)
		{
			_isLikeEnabled = true;
			_matchMode = matchMode;
			return this;
		}

		/// <summary>
		/// Use the "like" operator for all string-valued properties.
		/// </summary>
		/// <remarks>
		/// The default <see cref="MatchMode"/> is <see cref="MatchMode.Exact">MatchMode.Exact</see>.
		/// </remarks>
		public Example EnableLike()
		{
			return EnableLike( MatchMode.Exact );
		}

		public Example IgnoreCase()
		{
			_isIgnoreCaseEnabled = true;
			return this;
		}

		/// <summary>
		/// Exclude a particular named property
		/// </summary>
		/// <param name="name">The name of the property to exclude.</param>
		public Example ExcludeProperty(String name)
		{
			_excludedProperties.Add( name );
			return this;
		}

		/// <summary>
		/// Create a new instance, which includes all non-null properties 
		/// by default
		/// </summary>
		/// <param name="entity"></param>
		/// <returns>A new instance of <see cref="Example" />.</returns>
		public static Example Create(object entity)
		{
			if( entity == null )
			{
				throw new ArgumentNullException( "entity", "null example" );
			}
			return new Example( entity, NotNullOrEmptyString );
		}

		/// <summary>
		/// Initialize a new instance of the <see cref="Example" /> class for a particular
		/// entity.
		/// </summary>
		/// <param name="entity">The <see cref="Object"/> that the Example is being built from.</param>
		/// <param name="selector">The <see cref="IPropertySelector"/> the Example should use.</param>
		protected Example(object entity, IPropertySelector selector)
		{
			_entity = entity;
			_selector = selector;
		}

		public override String ToString()
		{
			return _entity.ToString();
		}

		/// <summary>
		/// Determines if the property should be included in the Query.
		/// </summary>
		/// <param name="value">The value of the property.</param>
		/// <param name="name">The name of the property.</param>
		/// <param name="type">The <see cref="IType"/> of the property.</param>
		/// <returns>
		/// <c>true</c> if the Property should be included, <c>false</c> if
		/// the Property should not be a part of the Query.
		/// </returns>
		private bool IsPropertyIncluded(object value, String name, IType type)
		{
			return !_excludedProperties.Contains( name ) &&
				!type.IsAssociationType &&
				_selector.Include( value, name, type );
		}

		public override SqlString ToSqlString(
			ISessionFactoryImplementor factory,
			System.Type persistentClass,
			string alias,
			IDictionary aliasClasses )
		{
			SqlStringBuilder builder = new SqlStringBuilder();
			builder.Add( StringHelper.OpenParen );

			IClassMetadata meta = factory.GetClassMetadata( persistentClass );
			String[] propertyNames = meta.PropertyNames;
			IType[] propertyTypes = meta.PropertyTypes;
			object[] propertyValues = meta.GetPropertyValues( _entity );
			for( int i = 0; i < propertyNames.Length; i++ )
			{
				object propertyValue = propertyValues[ i ];
				String propertyName = propertyNames[ i ];

				bool isPropertyIncluded = i != meta.VersionProperty &&
					IsPropertyIncluded( propertyValue, propertyName, propertyTypes[ i ] );
				if( isPropertyIncluded )
				{
					if( propertyTypes[ i ].IsComponentType )
					{
						AppendComponentCondition(
							propertyName,
							propertyValue,
							(IAbstractComponentType)propertyTypes[ i ],
							persistentClass,
							alias,
							aliasClasses,
							factory,
							builder
							);
					}
					else
					{
						AppendPropertyCondition(
							propertyName,
							propertyValue,
							persistentClass,
							alias,
							aliasClasses,
							factory,
							builder
							);
					}
				}
			}
			if( builder.Count == 1 )
			{
				builder.Add( "1=1" ); // yuck!
			}

			builder.Add( StringHelper.ClosedParen );
			return builder.ToSqlString();
		}

		public override TypedValue[] GetTypedValues(ISessionFactoryImplementor sessionFactory, System.Type persistentClass, IDictionary aliasClasses)
		{
			IClassMetadata meta = sessionFactory.GetClassMetadata( persistentClass );
			string[] propertyNames = meta.PropertyNames;
			IType[] propertyTypes = meta.PropertyTypes;
			object[] values = meta.GetPropertyValues( _entity );

			ArrayList list = new ArrayList();
			for( int i = 0; i < propertyNames.Length; i++ )
			{
				object value = values[ i ];
				IType type = propertyTypes[ i ];
				string name = propertyNames[ i ];

				bool isPropertyIncluded = ( i != meta.VersionProperty && IsPropertyIncluded( value, name, type ) );

				if( isPropertyIncluded )
				{
					if( propertyTypes[ i ].IsComponentType )
					{
						AddComponentTypedValues( name, value, (IAbstractComponentType)type, list );
					}
					else
					{
						AddPropertyTypedValue( value, type, list );
					}
				}
			}

			return (TypedValue[])list.ToArray( typeof( TypedValue ) );
		}

		/// <summary>
		/// Adds a <see cref="TypedValue"/> based on the <c>value</c> 
		/// and <c>type</c> parameters to the <see cref="IList"/> in the
		/// <c>list</c> parameter.
		/// </summary>
		/// <param name="value">The value of the Property.</param>
		/// <param name="type">The <see cref="IType"/> of the Property.</param>
		/// <param name="list">The <see cref="IList"/> to add the <see cref="TypedValue"/> to.</param>
		/// <remarks>
		/// This method will add <see cref="TypedValue"/> objects to the <c>list</c> parameter.
		/// </remarks>
		protected void AddPropertyTypedValue(object value, IType type, IList list)
		{
			// TODO: I don't like this at all - why don't we have it return a TypedValue[]
			// or an ICollection that can be added to the list instead of modifying the
			// parameter passed in.
			if( value != null )
			{
				if( value is string )
				{
					string stringValue = (string)value;
					if( _isIgnoreCaseEnabled )
					{
						stringValue = stringValue.ToLower();
					}
					if( _isLikeEnabled )
					{
						stringValue = _matchMode.ToMatchString( stringValue );
					}
					value = stringValue;
				}
				list.Add( new TypedValue( type, value ) );
			}
		}

		protected void AddComponentTypedValues(string path, object component, IAbstractComponentType type, IList list)
		{
			if( component != null )
			{
				string[] propertyNames = type.PropertyNames;
				IType[] subtypes = type.Subtypes;
				object[] values = type.GetPropertyValues( component );
				for( int i = 0; i < propertyNames.Length; i++ )
				{
					object value = values[ i ];
					IType subtype = subtypes[ i ];
					string subpath = StringHelper.Qualify( path, propertyNames[ i ] );
					if( IsPropertyIncluded( value, subpath, subtype ) )
					{
						if( subtype.IsComponentType )
						{
							AddComponentTypedValues( subpath, value, (IAbstractComponentType)subtype, list );
						}
						else
						{
							AddPropertyTypedValue( value, subtype, list );
						}
					}

				}
			}
		}

		protected void AppendPropertyCondition(
			String propertyName,
			object propertyValue,
			System.Type persistentClass,
			String alias,
			IDictionary aliasClasses,
			ISessionFactoryImplementor sessionFactory,
			SqlStringBuilder builder)
		{
			if( builder.Count > 1 )
			{
				builder.Add( " and " );
			}

			ICriterion crit;
			if( propertyValue != null )
			{
				bool isString = propertyValue is String;
				crit = ( _isLikeEnabled && isString ) ?
					(ICriterion)new LikeExpression( propertyName, propertyValue, _isIgnoreCaseEnabled ) :
					(ICriterion)new EqExpression( propertyName, propertyValue, _isIgnoreCaseEnabled && isString );

			}
			else
			{
				crit = new NullExpression( propertyName );
			}
			builder.Add( crit.ToSqlString( sessionFactory, persistentClass, alias, aliasClasses ) );
		}

		protected void AppendComponentCondition(
			String path,
			object component,
			IAbstractComponentType type,
			System.Type persistentClass,
			String alias,
			IDictionary aliasClasses,
			ISessionFactoryImplementor sessionFactory,
			SqlStringBuilder builder)
		{
			if( component != null )
			{
				String[] propertyNames = type.PropertyNames;
				object[] values = type.GetPropertyValues( component, null );
				IType[] subtypes = type.Subtypes;
				for( int i = 0; i < propertyNames.Length; i++ )
				{
					String subpath = StringHelper.Qualify( path, propertyNames[ i ] );
					object value = values[ i ];
					if( IsPropertyIncluded( value, subpath, subtypes[ i ] ) )
					{
						IType subtype = subtypes[ i ];
						if( subtype.IsComponentType )
						{
							AppendComponentCondition(
								subpath,
								value,
								(IAbstractComponentType)subtype,
								persistentClass,
								alias,
								aliasClasses,
								sessionFactory,
								builder );
						}
						else
						{
							AppendPropertyCondition(
								subpath,
								value,
								persistentClass,
								alias,
								aliasClasses,
								sessionFactory,
								builder
								);
						}
					}
				}
			}
		}
	}
}