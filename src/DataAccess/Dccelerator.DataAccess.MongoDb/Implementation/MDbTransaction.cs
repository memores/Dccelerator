﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;


namespace Dccelerator.DataAccess.MongoDb.Implementation {
    public class MDbTransaction: IMDbTransaction, IDisposable {
        readonly IMongoDatabase _mongoDatabase;
        bool _isAborted;
        bool _isCommited;

        readonly ConcurrentQueue<TransactionElement> _completedElements = new ConcurrentQueue<TransactionElement>();
        readonly ConcurrentDictionary<string, object> _originEntities = new ConcurrentDictionary<string, object>();


        public bool IsAborted {
            get { return _isAborted; }
            set { _isAborted = value; }
        }


        public bool IsCommited {
            get { return _isCommited; }
            private set { _isCommited = value; }
        }
        

        public bool Commit() {
            IsCommited = true;
            return true;
        }


        public bool Abort() {
            foreach (var element in _completedElements) {
                var keyValuePair = element.Info.Repository.KeyValuePairOf(element.Entity, element.Info);
                if(!_originEntities.TryGetValue(keyValuePair.Value, out var origin))
                    continue;

                element.Info.Repository.Update(element.Info, origin);
            }

            _isAborted = true;
            return true;
        }


        public void Dispose() {
            Commit();
        }

        public void Begin() {
            _isAborted = false;
            _isCommited = false;
        }


        public void StoreOrigin(TransactionElement element) {
            var keyValuePair = element.Info.Repository.KeyValuePairOf(element.Entity, element.Info);
            var origin = element.Info.Repository.Read(
                    element.Info,
                    new List<IDataCriterion>() {
                        new DataCriterion() {
                            Name = keyValuePair.Key,
                            Type = element.Info.PersistedProperties.SingleOrDefault(x=>x.Key == keyValuePair.Key).Value.PropertyType,
                            Value = keyValuePair.Value
                        }
                    })
                .SingleOrDefault();

            if (!_originEntities.TryAdd(keyValuePair.Value, origin))
                Abort();
        }


        public void CompleteAction(TransactionElement element) {
            _completedElements.Enqueue(element);
        }
    }
}