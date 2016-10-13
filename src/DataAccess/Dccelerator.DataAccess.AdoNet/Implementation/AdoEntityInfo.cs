﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Dccelerator.DataAccess.Implementation;
using Dccelerator.Reflection;

#if !NET40
using System.Reflection;
#endif

namespace Dccelerator.DataAccess.Ado.Implementation {

    public abstract class AdoEntityInfo<TRepository, TDbTypeEnum> : BaseEntityInfo<TRepository>, IAdoEntityInfo<TDbTypeEnum> 
        where TRepository : class, IAdoNetRepository
        where TDbTypeEnum : struct 
    {

        //        class DefaultAdoEntityInfo : AdoEntityInfo<TRepository, TDbTypeEnum> {
        //            public DefaultAdoEntityInfo(Type entityType) : base(entityType) {}
        //            protected override TDbTypeEnum GetDefaultDbType(Type propertyType) {
        //                return default(TDbTypeEnum);
        //            }
        //        }
        //
        //        internal static AdoEntityInfo<TRepository> GetInstanse(Type entityInfo) {
        //            return new DefaultAdoEntityInfo(entityInfo);
        //        }


        protected abstract IAdoEntityInfo GetInstanse(Type type);
        public abstract TDbTypeEnum GetDefaultDbType(Type propertyType);


        readonly object _lock = new object();

        protected AdoEntityInfo(Type entityType) : base(entityType) { }


        void IAdoEntityInfo.SetupRepository(IAdoNetRepository repository) {
            var genericRepository = repository as TRepository;
            if (genericRepository == null)
                throw new InvalidOperationException($"{nameof(repository)} should be of type {typeof(TRepository)}.");

            if (Repository != null)
                throw new InvalidOperationException("Repository already initialized.");

            Repository = genericRepository;
        }




        private Dictionary<string, TDbTypeEnum> _typeMappings;

        protected virtual Dictionary<string, TDbTypeEnum> TypeMappings {
            get {
                if (_typeMappings != null)
                    return _typeMappings;

                return _typeMappings = GetDbTypeMappings();
            }
        }



        public virtual TDbTypeEnum GetParameterDbType(string parameterName) {
            TDbTypeEnum type;
            if (!TypeMappings.TryGetValue(parameterName, out type))
                throw new InvalidOperationException($"Parameter with name {parameterName} not exist in {nameof(TypeMappings)} of {EntityType}.");

            return type;
        }


        #region Implementation of IAdoEntityInfo

        public virtual Dictionary<string, PropertyInfo> NavigationProperties { get; }


        IAdoNetRepository IAdoEntityInfo.Repository => Repository;

        public virtual string[] ReaderColumns { get; protected set; }


        protected virtual Dictionary<string, TDbTypeEnum> GetDbTypeMappings()
        {
            var mappings = new Dictionary<string, TDbTypeEnum>();
            foreach (var property in PersistedProperties.Values)
            {
                TDbTypeEnum result;

                var dbTypeAttribute =
                    property.GetMany<DbTypeAttribute>()
                        .FirstOrDefault(x => x.RepositoryType == (Repository?.GetType() ?? typeof(TRepository)));
                if (dbTypeAttribute != null)
                {
                    if (dbTypeAttribute.DbTypeName is TDbTypeEnum)
                        result = (TDbTypeEnum)dbTypeAttribute.DbTypeName;
                    else if (!Enum.TryParse(dbTypeAttribute.DbTypeName.ToString(), out result))
                        result = GetDefaultDbType(property.PropertyType);
                }
                else
                {
                    result = GetDefaultDbType(property.PropertyType);
                }

                mappings.Add(property.Name, result);
            }


            return mappings;
        }

        Dictionary<string, int> _readerColumnsIndexes;
        public int IndexOf(string columnName) {
            if (_readerColumnsIndexes == null) {
                lock (_lock) {
                    if (_readerColumnsIndexes == null) {
                        _readerColumnsIndexes = new Dictionary<string, int>(ReaderColumns.Length);

                        for (var i = 0; i < ReaderColumns.Length; i++) {
                            _readerColumnsIndexes.Add(ReaderColumns[i], i);
                        }
                    }
                }
            }

            return _readerColumnsIndexes[columnName];
        }


        public void InitReaderColumns(DbDataReader reader) {
            if (ReaderColumns != null)
                return;

#if NET40 || NET45
            var columns = reader.GetSchemaTable()?.Rows.Cast<DataRow>().Select(x => (string) x[0]).ToArray();
#else
            throw new NotImplementedException() and don't build until it's implemented!
#endif
            lock (_lock) {
                if (ReaderColumns == null)
                    ReaderColumns = columns;
            }
        }




        Dictionary<int, Includeon> _inclusions;


        public Dictionary<int, Includeon> Inclusions {
            get {
                if (_inclusions == null) {
                    lock (_lock) {
                        if (_inclusions == null)
                            _inclusions = GetInclusions();
                    }
                }

                return _inclusions;
            }
        }


        IEnumerable<IIncludeon> IEntityInfo.Inclusions => Inclusions.Values.Any() ? Inclusions.Values : null;

        





        Dictionary<int, Includeon> GetInclusions() {
            var inclusions = new Dictionary<int, Includeon>();

            var inclusionAttributes = TypeInfo.GetCustomAttributes<IncludeChildrenAttribute>().ToArray();
            if (inclusionAttributes.Length == 0)
                return inclusions;

            foreach (var inclusionAttribute in inclusionAttributes) {
                var includeon = new Includeon(inclusionAttribute, this, GetInstanse);
                inclusions.Add(inclusionAttribute.ResultSetIndex, includeon);
            }

            return inclusions;
        }
        


        #endregion
    }


}