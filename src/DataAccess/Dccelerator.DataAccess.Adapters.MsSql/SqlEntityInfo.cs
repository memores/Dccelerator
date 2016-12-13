﻿using System;
using System.Data;
using Dccelerator.DataAccess.Ado.Implementation;

namespace Dccelerator.DataAccess.Ado.SqlClient {
    public sealed class SqlEntityInfo<TRepository> : AdoEntityInfo<TRepository, SqlDbType> where TRepository : class, IAdoNetRepository {
        public SqlEntityInfo(Type entityType) : base(entityType) {}

        protected override IAdoEntityInfo GetInstance(Type type) => new SqlEntityInfo<TRepository>(type);

        public override SqlDbType GetDefaultDbType(Type propertyType) => propertyType.SqlType();

    }
}