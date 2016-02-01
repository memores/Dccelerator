﻿using System;
using System.Data;
using System.Linq.Expressions;
using Dccelerator.DataAccess.Implementations.Schedulers;
using Dccelerator.DataAccess.Infrastructure;


namespace Dccelerator.DataAccess.BerkeleyDb {
    public class BDbDataManagerFactory : IDataManagerFactory {
        readonly string _environmentPath;
        readonly string _dbFilePath;
        readonly string _password;


        public BDbDataManagerFactory(string environmentPath, string dbFilePath, string password) {
            _environmentPath = environmentPath;
            _dbFilePath = dbFilePath;
            _password = password;
        }


        #region Implementation of IDataManagerFactory

        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in cached context.
        /// This method will be called one time for each <typeparamref name="TEntity"/> requested in each data manager.
        /// </summary>
        public IDataGetter<TEntity> GetterFor<TEntity>() where TEntity : class, new() {
            return NotCachedGetterFor<TEntity>();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataGetter{TEntity}"/>, that will be used in not cached context.
        /// This method will be called on each request of any not cached entity.
        /// </summary>
        public IDataGetter<TEntity> NotCachedGetterFor<TEntity>() where TEntity : class, new() {
            return new BDbDataGetter<TEntity>(typeof(TEntity).Name, _environmentPath, _dbFilePath, _password);
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        public IDataExistenceChecker<TEntity> DataExistenceChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called on each delete request.
        /// </summary>
        public IDataExistenceChecker<TEntity> NoCachedExistenceChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>.
        /// This method will be called one time for every <typeparamref name="TEntity"/>.
        /// </summary>
        public IDataCountChecker<TEntity> DataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataExistenceChecker{TEntity}"/>
        /// This method will be called on each delete request.
        /// </summary>
        public IDataCountChecker<TEntity> NoCachedDataCountChecker<TEntity>() where TEntity : class {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Instantinate an <see cref="IDataTransaction"/>.
        /// This method will be called on each <see cref="IDataManager.BeginTransaction"/> call.
        /// </summary>
        public IDataTransaction DataTransaction(ITransactionScheduler scheduler, IsolationLevel isolationLevel) {
            return new NotScheduledBDbTransaction();
        }


        /// <summary>
        /// Instantinate an <see cref="ITransactionScheduler"/>.
        /// This method will be called one time in every <see cref="IDataManager"/>.
        /// </summary>
        public ITransactionScheduler Scheduler() {
            return new DummyScheduler(); //todo: test it!
        }


        public IEntityInfo InfoAbout<TEntity>() {
            throw new NotImplementedException();
        }

        #endregion
    }
}