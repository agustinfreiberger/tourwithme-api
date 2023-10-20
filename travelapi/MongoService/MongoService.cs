using MongoDB.Driver;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using travelapi.Models;
using travelapi.Settings;

namespace travelapi.MongoService
{
    public class MongoService : IMongoService
    {
        private MongoClient _mongoClient;
        private IMongoDatabase _database;
        private IMongoCollection<User> _usersTable;
        private IMongoCollection<Poi> _poisTable;


        public MongoService(MongoSettings mongoSettings)
        {
            var settings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
            _mongoClient = new MongoClient(settings);
            _database = _mongoClient.GetDatabase(mongoSettings.DbName);
            _usersTable = _database.GetCollection<User>(mongoSettings.CollectionNameTourists);
            _poisTable = _database.GetCollection<Poi>(mongoSettings.CollectionNamePlaces);
        }

        public string AltaUsuario(User u)
        {
            if (u != null)
            {
                User user = new User()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Latitud = u.Latitud,
                    Longitud = u.Longitud,
                    Preferences = u.Preferences,
                    Disponible = u.Disponible
                };
                _usersTable.InsertOneAsync(user);
                return user.Id.ToString();
            }
            else
            {
                return "El usuario fue null";
            }
        }

        public string ActualizarUbicacion(Guid id, double lat, double longitud) {
            UpdateDefinition<User> update = Builders<User>.Update.Set(x => x.Latitud, lat)
                                                                .Set(x => x.Longitud, longitud);


            var result = _usersTable.UpdateOneAsync(x => x.Id == id, update);
            return id.ToString();
        }

        public string ActualizarPreferencias(Guid id, List<PlaceCategoryPreference> preferencias)
        {
            UpdateDefinition<User> update = Builders<User>.Update.Set(x => x.Preferences, preferencias);


            var result = _usersTable.UpdateOneAsync(x => x.Id == id, update);
            return id.ToString();
        }

        public string ModificarUsuario(User u)
        {
            UpdateDefinition<User> update = Builders<User>.Update.Set(x => x.Latitud, u.Latitud)
                                                            .Set(x => x.Longitud, u.Longitud)
                                                        .Set(x => x.Preferences, u.Preferences)
                                                        .Set(x => x.Disponible, u.Disponible);


            var result = _usersTable.UpdateOneAsync(x => x.Id == u.Id, update);
            return u.Id.ToString();
        }

        public string EliminarUsuario(Guid promId)
        {
            if (_usersTable.DeleteOne(x => x.Id == promId).DeletedCount > 0)
            {
                return "Borrado con exito";
            }
            return "No existe el documento";
        }

        public User GetUsuario(Guid userId)
        {
            return _usersTable.Find(x => x.Id == userId).FirstOrDefault();
        }

        public List<User> GetUsuarios()
        {
            return _usersTable.Find(FilterDefinition<User>.Empty).ToList();
        }

        public List<User> GetUsuariosCercanos(double lat, double longitud)
        {
            var filter = Builders<User>.Filter.Where(x => GreatCircleDistance(lat, longitud, x.Latitud, x.Longitud) < 4); //distancia euclidea
            return _usersTable.Find(filter).ToList();
        }

        public List<Poi> GetPoisCercanos(Coordinate center) {
            var filter = Builders<Poi>.Filter.Where(x => GreatCircleDistance(center.X, center.Y, x.Longitud, x.Latitud) < 4); //distancia euclidea
            return _poisTable.Find(filter).ToList();
        }

        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        private static double GreatCircleDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double R = 6371e3; // m

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            double d = Math.Acos(cosD);

            double dist = (R * d) / 1000; //paso metros a Km

            return dist;
        }

    }
}
