﻿using MyAccountAPI.Domain.Model;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MyAccountAPI.Domain.Model.Customers;

namespace MyAccountAPI.Producer.Infrastructure.DataAccess
{
    public class MongoContext
    {
        private readonly MongoClient mongoClient;
        private readonly IMongoDatabase database;

        public MongoContext(string connectionString, string databaseName)
        {
            this.mongoClient = new MongoClient(connectionString);
            this.database = mongoClient.GetDatabase(databaseName);
            Map();
        }

        public void DatabaseReset(string databaseName)
        {
            mongoClient.DropDatabase(databaseName);
        }

        public IMongoCollection<Customer> Customers
        {
            get
            {
                return database.GetCollection<Customer>("Customers");
            }
        }

        private void Map()
        {
            BsonClassMap.RegisterClassMap<Entity>(cm =>
            {
                cm.MapIdProperty(c => c.Id);
            });

            BsonClassMap.RegisterClassMap<AggregateRoot>(cm =>
            {
                cm.MapProperty(c => c.Version).SetElementName("_version");
            });

            BsonClassMap.RegisterClassMap<Account>(cm =>
            {
                cm.MapField("amount").SetElementName("amount");
            });

            BsonClassMap.RegisterClassMap<Customer>(cm =>
            {
                cm.MapField("name").SetElementName("name");
                cm.MapField("pin").SetElementName("pin");
            });
        }
    }
}