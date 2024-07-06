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
                        Age = u.Age,
                        Latitud = u.Latitud,
                        Longitud = u.Longitud,
                        Preferences = u.Preferences
                    };
                    _usersTable.InsertOneAsync(user);
                    return user.Id.ToString();
                }
                else
                {
                    ModificarUsuario(u);
                    return "existe y se modifico";
                }
            }
            else
            {
                return "usuario es null";
            }
        }

        public string ActualizarUbicacion(string name, double lat, double longitud) {
            UpdateDefinition<User> update = Builders<User>.Update.Set(x => x.Latitud, lat)
                                                                .Set(x => x.Longitud, longitud);


            var result = _usersTable.UpdateOneAsync(x => x.Name == name, update);
            return name.ToString();
        }

        public string ActualizarPreferencias(string name, List<PlaceCategoryPreference> preferencias)
        {
            UpdateDefinition<User> update = Builders<User>.Update.Set(x => x.Preferences, preferencias);


            var result = _usersTable.UpdateOneAsync(x => x.Name == name, update);
            return name.ToString();
        }

        public string ModificarUsuario(User u)
        {
            UpdateDefinition<User> update = Builders<User>.Update.Set(x => x.Age, u.Age)
                                                                .Set(x => x.Latitud, u.Latitud)
                                                            .Set(x => x.Longitud, u.Longitud)
                                                        .Set(x => x.Preferences, u.Preferences);


            var result = _usersTable.UpdateOneAsync(x => x.Name == u.Name, update);
            return u.Id.ToString();
        }

        public string EliminarUsuario(string name)
        {
            if (_usersTable.DeleteOne(x => x.Name == name).DeletedCount > 0)
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

        public List<User> GetUsuariosCercanosYSimilares(string name, double lat, double longitud) {

            var usuariosSimilares = new List<User>();

            var myUser = _usersTable.Find(x => x.Name == name).FirstOrDefault();
            var allUsers = _usersTable.Find(x => x.Name != name).ToList();

            var usuariosCercanos = new List<User>();
            foreach (var usuario in allUsers)
            {
                if (CalculateManhattanDistance(lat, longitud, usuario.Latitud, usuario.Longitud) < 4)
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

        public static double CalculateManhattanDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // Calcular las diferencias absolutas de latitud y longitud
            double latDifference = Math.Abs(lat1 - lat2);
            double lonDifference = Math.Abs(lon1 - lon2);

            // Convertir las diferencias de grados a kilómetros aproximadamente (1 grado de latitud ≈ 111 km y 1 grado de longitud depende de la latitud)
            double kmPerDegreeLat = 111.32; // Aproximadamente 111.32 km por cada grado de latitud
            double kmPerDegreeLon = 111.32 * Math.Cos(Radians((lat1 + lat2) / 2)); // Ajuste según la latitud promedio

            double latDistanceKm = latDifference * kmPerDegreeLat;
            double lonDistanceKm = lonDifference * kmPerDegreeLon;

            // Calcular la distancia de Manhattan
            double manhattanDistance = latDistanceKm + lonDistanceKm;

            return manhattanDistance;
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
