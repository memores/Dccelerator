using System;
using Dccelerator.DataAccess.Ado.BasicImplementation;


namespace Dccelerator.DataAccess.Ado.SqlClient.Implementation {
    class SqlClientCommonReadingRepository : CachedReadingRepository {
        #region Overrides of DirectReadingRepository

        protected override bool IsDeadlock(Exception exception) {
            return exception.IsDeadlock();
        }

        #endregion
    }
}