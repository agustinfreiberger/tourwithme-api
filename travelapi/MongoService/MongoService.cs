using MongoDB.Driver;
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
        private FilterDefinition<User> _filter;



        public MongoService(MongoSettings mongoSettings)
        {
            var settings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
            _mongoClient = new MongoClient(settings);
            _database = _mongoClient.GetDatabase(mongoSettings.DbName);
            _usersTable = _database.GetCollection<User>(mongoSettings.CollectionNameTourists);

        }

        public string AltaUsuario(User u)
        {
            this._filter = Builders<User>.Filter.Eq("Name", u.Name);

            if (u != null)
            {
                if (_usersTable.Find(_filter).ToList().Count == 0) { 
                    User user = new User()
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Latitud = u.Latitud,
                        Longitud = u.Longitud,
                        Preferences = u.Preferences
                    };
                    _usersTable.InsertOneAsync(user);
                    return user.Id.ToString();
                }
                else
                {
                    return "existe";
                }
            }
            else
            {
                return "null";
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
                                                        .Set(x => x.Preferences, u.Preferences);


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

        public User GetUsuario(string name)
        {
            return _usersTable.Find(x => x.Name == name).FirstOrDefault();
        }

        public List<User> GetUsuarios()
        {
            return _usersTable.Find(FilterDefinition<User>.Empty).ToList();
        }

        public List<User> GetUsuariosCercanosYSimilares(Guid userId, double lat, double longitud) {

            var usuariosSimilares = new List<User>();

            var myUser = _usersTable.Find(x => x.Id == userId).FirstOrDefault();
            var allUsers = _usersTable.Find(x => x.Id != userId).ToList();

            var usuariosCercanos = new List<User>();
            foreach (var usuario in allUsers)
            {
                if (GreatCircleDistance(lat, longitud, usuario.Latitud, usuario.Longitud) < 4)
                {
                    usuariosCercanos.Add(usuario);
                }
            }

            double similarity = int.MaxValue;

            //Obtengo la menor similitud
            foreach (var user in usuariosCercanos)
            {
                if (similarity > CalculateSimilarity(myUser.Preferences, user.Preferences)) {
                    similarity = CalculateSimilarity(myUser.Preferences, user.Preferences);
                }
            }

            //Filtro los usuarios cercanos por similitud
            foreach (var user in usuariosCercanos)
            {
                if (similarity < CalculateSimilarity(myUser.Preferences, user.Preferences)) 
                {
                    usuariosSimilares.Add(user);
                }
                
                if (usuariosSimilares.Count() == 3)
                { //Supongo un tamaño de grupo maximo de 3 por el tamaño de muestra de la experimentación
                    return usuariosSimilares;
                }
            }

            //Si no llego a completar el cupo, completo con usuarios cercanos
            if (usuariosSimilares.Count() < 3) {
                foreach (var user in usuariosCercanos)
                {
                    if (!usuariosSimilares.Any(x => x.Id == user.Id))
                    {
                        usuariosSimilares.Add(user);
                    }

                    if (usuariosSimilares.Count() == 3)
                    { //Supongo un tamaño de grupo maximo de 3 por el tamaño de muestra de la experimentación
                        return usuariosSimilares;
                    }
                }
            }

            return usuariosSimilares;
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

        static double CalculateSimilarity(List<PlaceCategoryPreference> preferences1, List<PlaceCategoryPreference> preferences2)
        {
            var categories1 = preferences1.ToDictionary(p => p.Placecategory, p => p.Preference);
            var categories2 = preferences2.ToDictionary(p => p.Placecategory, p => p.Preference);

            var commonCategories = categories1.Keys.Intersect(categories2.Keys).ToList();
            if (commonCategories.Count == 0)
                return 0; // No hay categorías en común, similitud es cero.


            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            foreach (var categoria in commonCategories)
            {
                double valorPreferencia1 = SmoothCosine(categories1[categoria]);
                double valorPreferencia2 = SmoothCosine(categories2[categoria]);

                dotProduct += valorPreferencia1 * valorPreferencia2;
                magnitude1 += valorPreferencia1 * valorPreferencia1;
                magnitude2 += valorPreferencia2 * valorPreferencia2;
            }

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0; // Evitar división por cero

            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }

        static double SmoothCosine(double x)
        {
            return (1 - Math.Cos(x * Math.PI)) / 2;
        }
    }
}
