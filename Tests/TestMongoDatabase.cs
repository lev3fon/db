using System;
using MongoDB.Driver;

namespace Tests
{
    public static class TestMongoDatabase
    {
        public static IMongoDatabase Create()
        {
            var mongoConnectionString = "mongodb+srv://lev_3fon:lev1990t@cluster0.qz5qk.mongodb.net/myFirstDatabase?retryWrites=true&w=majority";
            // var mongoConnectionString = Environment.GetEnvironmentVariable("PROJECT5100_MONGO_CONNECTION_STRING")
            //                             ?? "mongodb://localhost:27017";
            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient.GetDatabase("game-tests");
        }
    }
}