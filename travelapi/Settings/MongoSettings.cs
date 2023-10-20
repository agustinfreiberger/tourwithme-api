using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace travelapi.Settings
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string DbName { get; set; }
        public string CollectionNameTourists { get; set; }

        public string CollectionNamePlaces { get; set; }
    }
}
