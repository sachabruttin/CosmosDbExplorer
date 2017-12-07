using System;

namespace DocumentDbExplorer.Infrastructure
{
    public static class Constants
    {
        public static class Emulator
        {
            public static Uri Endpoint = new Uri("https://localhost:8081");
            public const string Secret = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        }

        public static class Default
        {
            public const string StoredProcedure = "function storedProcedure(){}";
            public const string Trigger = "function trigger(){}";
            public const string UserDefiniedFunction = "function userDefinedFunction(){}";
        }
    }
}
