using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using travelapi.Models;

namespace travelapi.MongoService
{
    public interface IMongoService
    {
        string AltaUsuario(User u);

        string ModificarUsuario(User u);

        string EliminarUsuario(Guid userId);

        User GetUsuario(Guid userId);
        List<User> GetUsuarios();

        List<User> GetUsuariosCercanos(double x, double y);

        List<Poi> GetPoisCercanos(Coordinate coordinate);

    }
}

