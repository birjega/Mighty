﻿using System;
using System.Reflection;

using Mighty.Mapping;

namespace Mighty.Interfaces
{
    /// <summary>
    /// Pass an instance of this interface to the constructor of <see cref="MightyOrm"/> in order to
    /// map between C# field names and SQL column names.
    /// If you're not (yet) used to <see cref="Action"/>/<see cref="Func{T, TResult}"/> syntax in C#, you may find
    /// slightly harder to set up this mapper than if it had just been a class with methods you can override (see
    /// Mighty documentation for examples). One reason for doing it like this is that Mighty can then do much more
    /// aggressive and successful caching of its data contracts, by checking whether the mapping functions (not just
    /// the whole mapper) are the same.
    /// </summary>
    abstract public class SqlNamingMapperAbstractInterface
    {
        #region Table-only features (not needed in column mapping conract)
        /// <summary>
        /// Function to get database table name from the data item type.
        /// Default is to return <see cref="Type"/>.Name unmodified.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, string> TableNameMapping { get; protected set; }

        /// <summary>
        /// Function to get primary key field name(s) from the data item type and field or property name.
        /// The exact C# field/property name(s) should be returned and not database column names (where these are different).
        /// The default behaviour is to return <c>null</c> which results in no primary keys being specified in this way -
        /// they may still be specified using the <see cref="MightyOrm"/> `keys` constructor parameter.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, string> GetPrimaryKeyFieldNames { get; protected set; }

        /// <summary>
        /// Function to get the sequence from the data item type.
        /// Generally only applicable to sequence-based databases (Oracle and Postgres), except in the rare case where
        /// you may need to override the default identity function on identity-based databases (see Mighty documentation).
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, string> GetSequenceName { get; protected set; }
        #endregion

        #region Table-column features (needed in column mapping conract)
        /// <summary>
        /// Specify whether Mighty should automatically remap any `keys`, `columns` and `orderBy` inputs it receives if one or more column names have been remapped.
        /// Default is to return <see cref="AutoMap.On"/>.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, AutoMap> AutoMap { get; protected set; }

        /// <summary>
        /// Should <see cref="MightyOrm{T}"/> be case sensitive when matching returned data to class properties?
        /// Provided the data item type in case you need it.
        /// Default is to return <c>false</c> since many databases are case insensitive and use different case conventions from C#, by default.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, bool> CaseSensitiveColumns { get; protected set; }
        #endregion

        #region Column-level features
        /// <summary>
        /// Function to get database column name from the data item type and field or property name.
        /// Default is to return name unmodified.
        /// Since incoming data in Mighty can come from any name-value collection, <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, string, string> ColumnNameMapping { get; protected set; }

        /// <summary>
        /// Function to determine whether to ignore database column based on the data item type and field or property name.
        /// Default is to return <c>false</c> for do not ignore.
        /// Since incoming data in Mighty can come from any name-value collection, <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, string, bool> IgnoreColumn { get; protected set; }

        /// <summary>
        /// Function to determine column data direction based on the data item type and field or property name.
        /// Default is to return <c>0</c> to leave direction unspecified.
        /// Since incoming data in Mighty can come from any name-value collection, <see cref="MemberInfo"/>
        /// cannot always be provided and is left out to ensure consistent mapping.
        /// The type passed in is the class or subclass type for dynamic instances of <see cref="MightyOrm"/>
        /// and is the generic type T for generic instances of <see cref="MightyOrm{T}"/>.
        /// </summary>
        abstract public Func<Type, string, DataDirection> ColumnDataDirection { get; protected set; }
        #endregion

        #region Id mapping
        /// <summary>
        /// Function to perform database specific identifier quoting (such as "name" -> "[name]" or "name" -> "'name'").
        /// Default is to return the passed in string unmodified.
        /// You should handle quoting identifiers here only, or in <see cref="TableNameMapping"/> and <see cref="ColumnNameMapping"/> only, but not both.
        /// </summary>
        /// <remarks>
        /// TO DO: Might be useful to provide additional method which splits the name at the dots then rejoins it, with single overrideable method to quote the individual parts
        /// </remarks>
        abstract public Func<string, string> QuoteDatabaseIdentifier { get; protected set; }
        #endregion
    }
}
