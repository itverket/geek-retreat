using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Storage.Azure.Table
{
    public class AzureTableStorage
    {
        private readonly CloudTable _cloudTable;

        public AzureTableStorage(string connectionString, string tableName)
        {
            var tableClient = CreateTableClient(connectionString);

            _cloudTable = tableClient.GetTableReference(tableName);
        }

        private static CloudTableClient CreateTableClient(string connectionString)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            return storageAccount.CreateCloudTableClient();
        }

        public bool CreateIfNotExists() => _cloudTable.CreateIfNotExists();

        public bool DeleteIfExists() => _cloudTable.DeleteIfExists();

        public TableResult InsertEntity(ITableEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);

            return _cloudTable.Execute(insertOperation);
        }

        public Task<TableResult> InsertEntityAsync(ITableEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);

            return _cloudTable.ExecuteAsync(insertOperation);
        }

        public TableResult InsertOrReplaceEntity(ITableEntity entity)
        {
            var insertOperation = TableOperation.InsertOrReplace(entity);

            return _cloudTable.Execute(insertOperation);
        }

        public Task<TableResult> InsertOrReplaceEntityAsync(ITableEntity entity)
        {
            var insertOperation = TableOperation.InsertOrReplace(entity);

            return _cloudTable.ExecuteAsync(insertOperation);
        }

        public IList<TableResult> BatchInsertEntities(IEnumerable<ITableEntity> entities)
        {
            var batchInsertOperation = new TableBatchOperation();

            foreach (var entity in entities)
            {
                batchInsertOperation.Insert(entity);
            }

            return _cloudTable.ExecuteBatch(batchInsertOperation);
        }

        public Task<IList<TableResult>> BatchInsertEntitiesAsync(IEnumerable<ITableEntity> entities)
        {
            var batchInsertOperation = new TableBatchOperation();

            foreach (var entity in entities)
            {
                batchInsertOperation.Insert(entity);
            }

            return _cloudTable.ExecuteBatchAsync(batchInsertOperation);
        }

        public IList<TableResult> BatchInsertOrReplaceEntities(IEnumerable<ITableEntity> entities)
        {
            var batchInsertOperation = new TableBatchOperation();

            foreach (var entity in entities)
            {
                var insertOperation = TableOperation.InsertOrReplace(entity);
                batchInsertOperation.Add(insertOperation);
            }

            return _cloudTable.ExecuteBatch(batchInsertOperation);
        }

        public Task<IList<TableResult>> BatchInsertOrReplaceEntitiesAsync(IEnumerable<ITableEntity> entities)
        {
            var batchInsertOperation = new TableBatchOperation();

            foreach (var entity in entities)
            {
                var insertOperation = TableOperation.InsertOrReplace(entity);
                batchInsertOperation.Add(insertOperation);
            }

            return _cloudTable.ExecuteBatchAsync(batchInsertOperation);
        }

        public TEntity RetrieveEntity<TEntity>(string partitionKey, string rowKey) where TEntity : class, ITableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

            var tableResult = _cloudTable.Execute(retrieveOperation);

            return tableResult.Result as TEntity;
        }

        public Task<TableResult> RetrieveEntityAsync<TEntity>(string partitionKey, string rowKey)
            where TEntity : class, ITableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<TEntity>(partitionKey, rowKey);

            return _cloudTable.ExecuteAsync(retrieveOperation);
        }

        public IEnumerable<TEntity> RetrieveEntities<TEntity>(string filter) where TEntity : ITableEntity, new()
        {
            var retrieveQuery = new TableQuery<TEntity>().Where(filter);

            return _cloudTable.ExecuteQuery(retrieveQuery);
        }

        public IEnumerable<TEntity> RetrieveEntities<TEntity>(int count) where TEntity : ITableEntity, new()
        {
            var retrieveQuery = new TableQuery<TEntity>().Take(count);

            return _cloudTable.ExecuteQuery(retrieveQuery);
        }
    }
}