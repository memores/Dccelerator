using System;


namespace Dccelerator.DataAccess.Ado.ReadingRepositories {
    internal sealed class ForcedCacheReadingRepository : CachedReadingRepository
    {
        #region Overrides of CachedReadingRepository

        protected override TimeSpan CacheTimeoutOf(Type entityType) {
            return TimeSpan.MaxValue;
        }

        #endregion
    }
}