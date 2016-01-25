﻿using System;
using System.Collections.Generic;


namespace Dccelerator.DataAccess {

    /// <summary>
    /// Abstraction of reading repository, used only in internal implementation of data access.
    /// </summary>
    public interface IInternalReadingRepository {

        /// <summary>
        /// Reads entities by its <paramref name="entityName"/>, filtering they by <paramref name="criteria"/>
        /// </summary>
        IEnumerable<object> Read(string entityName, Type entityType, ICollection<IDataCriterion> criteria);

        /// <summary>
        /// Checks it any entity with <paramref name="entityName"/> satisfies specified <paramref name="criteria"/>
        /// </summary>
        bool Any(string entityName, Type entityType, ICollection<IDataCriterion> criteria);


        /// <summary>
        /// Reads column with specified <paramref name="columnName"/> from entity with <paramref name="entityName"/>, filtered with specified <paramref name="criteria"/>.
        /// It's used to .Select() something. 
        /// </summary>
        IEnumerable<object> ReadColumn(string columnName, string entityName, Type entityType, ICollection<IDataCriterion> criteria);

        /// <summary>
        /// Returns count of entities with <paramref name="entityName"/> that satisfies specified <paramref name="criteria"/>
        /// </summary>
        int CountOf(string entityName, Type entityType, ICollection<IDataCriterion> criteria);
    }
}