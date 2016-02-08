using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Dccelerator.DataAccess.Ado.Infrastructure;
using Dccelerator.DataAccess.Infrastructure;
using Dccelerator.Reflection;


namespace Dccelerator.DataAccess.Ado {
    

    /// <summary>
    /// Am <see cref="IAdoNetRepository"/> what defines special names of CRUD-operation stored procedures.
    /// </summary>
    /// <seealso cref="NameOfReadProcedureFor"/>
    /// <seealso cref="NameOfInsertProcedureFor"/>
    /// <seealso cref="NameOfUpdateProcedureFor"/>
    /// <seealso cref="NameOfDeleteProcedureFor"/>
    public abstract class AdoNetRepository<TCommand, TParameter, TConnection> : IAdoNetRepository
        where TCommand : DbCommand
        where TParameter: DbParameter
        where TConnection : DbConnection {


        protected abstract TParameter PrimaryKeyParameterOf<TEntity>(IEntityInfo info, TEntity entity);

        protected abstract TConnection GetConnection();

        protected abstract TParameter ParameterWith(string name, Type type, object value);

        protected abstract TCommand CommandFor(string commandText, TConnection connection, IEnumerable<TParameter> parameters, CommandType type = CommandType.StoredProcedure);


        
        protected virtual string NameOfReadProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_get_by_criteria");
        }


        
        protected virtual string NameOfInsertProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_insert");
        }


        
        protected virtual string NameOfUpdateProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_update");
        }


        
        protected virtual string NameOfDeleteProcedureFor( string entityName) {
            return string.Concat("obj_", entityName, "_delete");
        }


        protected virtual IEnumerable<TParameter> ParametersFrom<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            return info.PersistedProperties.Select(x => {
                object value;
                if (!RUtils<TEntity>.TryGetValueOnPath(entity, x.Key, out value))
                    throw new InvalidOperationException($"Entity of type {entity.GetType()} should contain property '{x.Key}', " +
                                                        $"but in some reason value or that property could not be getted.");

                return ParameterWith(x.Key, x.Value, value);
            });
        }
        


        /// <summary>
        /// Returns reader that can be used to get some data by <paramref name="entityName"/>, filtering it by <paramref name="criteria"/>.
        /// </summary>
        /// <param name="entityName">Database-specific name of some entity</param>
        /// <param name="criteria">Filtering criteria</param>
        public IEnumerable<object> Read(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return Read((IAdoEntityInfo) info, criteria);
        }

        protected virtual IEnumerable<object> Read(IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return ReadToEnd(reader, info);
            }
        }


        public bool Any(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return reader.Read();
            }
        }


        public IEnumerable<object> ReadColumn(string columnName, IEntityInfo info, ICollection<IDataCriterion> criteria) {
            return ReadColumn(columnName, (IAdoEntityInfo) info, criteria);
        }


        protected virtual IEnumerable<object> ReadColumn(string columnName, IAdoEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            var idx = info.ReaderColumns != null ? info.IndexOf(columnName) : -1;

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader()) {
                    if (idx < 0) {
                        info.InitReaderColumns(reader);
                        idx = info.IndexOf(columnName);
                    }

                    return SelectColumn(reader, idx);
                }
            }
        }


        public int CountOf(IEntityInfo info, ICollection<IDataCriterion> criteria) {
            var parameters = criteria.Select(x => ParameterWith(x.Name, x.Type, x.Value));

            using (var connection = GetConnection())
            using (var command = CommandFor(NameOfReadProcedureFor(info.EntityName), connection, parameters)) {
                connection.Open();
                using (var reader = command.ExecuteReader())
                    return RowsCount(reader);
            }
        }


        /// <summary>
        /// Inserts an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool Insert<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            var parameters = ParametersFrom(info, entity);

            using (var connection = GetConnection()) {
                using (var command = CommandFor(NameOfInsertProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }



        /// <summary>
        /// Inserts an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool InsertMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfInsertProcedureFor(info.EntityName);
            using (var connection = GetConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(info, entity);

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
                            return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Updates an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool Update<T>(IEntityInfo info, T entity) where T : class {
            var parameters = ParametersFrom(info, entity);

            using (var connection = GetConnection()) {
                using (var command = CommandFor(NameOfUpdateProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        /// <summary>
        /// Updates an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool UpdateMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfUpdateProcedureFor(info.EntityName);
            using (var connection = GetConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    var parameters = ParametersFrom(info, entity);

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
                            return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Removes an <paramref name="entity"/> using it's database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool Delete<TEntity>(IEntityInfo info, TEntity entity) where TEntity : class {
            var parameters = new [] { PrimaryKeyParameterOf(info, entity) };

            using (var connection = GetConnection()) {
                using (var command = CommandFor(NameOfDeleteProcedureFor(info.EntityName), connection, parameters)) {
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }


        /// <summary>
        /// Removes an <paramref name="entities"/> using they database-specific <paramref name="entityName"/>.
        /// </summary>
        /// <returns>Result of operation</returns>
        public virtual bool DeleteMany<TEntity>(IEntityInfo info, IEnumerable<TEntity> entities) where TEntity : class {
            var name = NameOfDeleteProcedureFor(info.EntityName);
            using (var connection = GetConnection()) {
                connection.Open();

                foreach (var entity in entities) {
                    
                    var parameters = new[] { PrimaryKeyParameterOf(info, entity)};

                    using (var command = CommandFor(name, connection, parameters)) {

                        if (command.ExecuteNonQuery() <= 0)
                            return false;
                    }
                }
            }

            return true;
        }





        
        protected virtual IEnumerable<object> SelectColumn(DbDataReader reader, int columnIndex) {
            while (reader.Read()) {
                yield return reader.GetValue(columnIndex);
            }
        }


        protected virtual int RowsCount(DbDataReader reader) {
            var count = 0;
            while (reader.Read()) {
                count++;
            }
            return count;
        }


        protected virtual IEnumerable<object> GetFirstOrDefault(DbDataReader reader, IAdoEntityInfo info) {
            info.InitReaderColumns(reader);

            while (reader.Read()) {
                object keyId;
                yield return ReadItem(reader, info, out keyId);
            }
        }



        protected virtual IEnumerable<object> ReadToEnd(DbDataReader reader, IAdoEntityInfo mainObjectInfo) {

            if (mainObjectInfo.Inclusions == null)
                return GetFirstOrDefault(reader, mainObjectInfo);


            var mainObjects = new Dictionary<object, object>();

            mainObjectInfo.InitReaderColumns(reader);

            while (reader.Read()) {
                object keyId;
                var item = ReadItem(reader, mainObjectInfo, out keyId);
                try {
                    mainObjects.Add(keyId, item);
                }
                catch (Exception e) {
                    Internal.TraceEvent(TraceEventType.Critical,
                        $"On reading '{mainObjectInfo.EntityType}' using special name {mainObjectInfo.EntityName} getted exception, " +
                        "possibly because reader contains more then one object with same identifier.\n" +
                        $"Identifier: {keyId}\n" +
                        $"Exception: {e}");

                    throw;
                }
            }

            if (mainObjectInfo.Inclusions == null || !mainObjectInfo.Inclusions.Any())
                return mainObjects.Values;


            var tableIndex = 0;

            while (reader.NextResult()) {
                tableIndex++;

                Includeon includeon;
                if (!mainObjectInfo.Inclusions.TryGetValue(tableIndex, out includeon)) {
                    Internal.TraceEvent(TraceEventType.Warning, $"Reader for object {mainObjectInfo.EntityType.FullName} returned more than one table, " +
                                                                $"but it has not includeon information for table#{tableIndex}.");
                    continue;
                }

                var info = includeon.Info;

                info.InitReaderColumns(reader);


                if (!includeon.IsCollection) {
                    
                    while (reader.Read()) {
                        object keyId;
                        var item = ReadItem(reader, info, out keyId);

                        if (keyId == null) {
                            Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.EntityType}, {includeon.Attribute.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                            break;
                        }

                        var index = tableIndex;
                        Parallel.ForEach(mainObjects.Values,
                            mainObject => {
                                object value;
                                if (!mainObject.TryGetValueOnPath(includeon.Attribute.TargetPath, out value) || !keyId.Equals(value))
                                    return;

                                if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, item))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");
                            });
                    }

                }
                else {
                    var children = new Dictionary<object, IList>(); //? Key is Main Object Primary Key, Value is children collection of main object's navigation property

                    while (reader.Read()) {
                        object keyId;
                        var item = ReadItem(reader, info, out keyId);

                        if (keyId == null) {
                            Internal.TraceEvent(TraceEventType.Error, $"Can't get key id from item with info {info.EntityType}, {includeon.Attribute.TargetPath} (used on entity {mainObjectInfo.EntityType}");
                            break;
                        }


                        IList collection;
                        if (!children.TryGetValue(keyId, out collection)) {
                            collection = (IList) Activator.CreateInstance(includeon.TargetCollectionType);
                            children.Add(keyId, collection);
                        }

                        collection.Add(item);
                    }


                    var index = tableIndex;
                    Parallel.ForEach(children,
                        child => {
                            object mainObject;
                            if (!mainObjects.TryGetValue(child.Key, out mainObject)) {
                                Internal.TraceEvent(TraceEventType.Warning, $"In result set #{index} finded data row of type {info.EntityType}, that doesn't has owner object in result set #1.\nOwner Id is {child.Key}.\nTarget path is '{includeon.Attribute.TargetPath}'.");
                                return;
                            }

                            if (!mainObject.TrySetValueOnPath(includeon.Attribute.TargetPath, child.Value))
                                Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.Attribute.TargetPath} from '{mainObjectInfo.EntityType.FullName}' context.\nTarget path specified for child item {info.EntityType} in result set #{index}.");

                            if (string.IsNullOrWhiteSpace(includeon.OwnerNavigationReferenceName))
                                return;

                            foreach (var item in child.Value) {
                                if (!item.TrySetValueOnPath(includeon.OwnerNavigationReferenceName, mainObject))
                                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {includeon.OwnerNavigationReferenceName} from '{info.EntityType}' context. This should be reference to owner object ({mainObject})");
                            }
                        });
                }
            }

            

            return mainObjects.Values;
        }


        protected virtual object ReadItem(DbDataReader reader, IAdoEntityInfo info, out object keyId) {
            var item = Activator.CreateInstance(info.EntityType);

            keyId = null;

            for (var i = 0; i < info.ReaderColumns.Length; i++) {
                var name = info.ReaderColumns[i];
                var value = reader.GetValue(i);
                if (value == null || value.GetType().FullName == "System.DBNull")
                    continue;

                if (IsPrimaryKey(name, info))
                    keyId = value;

                if (!item.TrySetValueOnPath(name, value))
                    Internal.TraceEvent(TraceEventType.Warning, $"Can't set property {name} from '{info.EntityType.FullName}' context.");
            }
            return item;
        }


        protected abstract bool IsPrimaryKey(string propertyName, IAdoEntityInfo info);
    }
}