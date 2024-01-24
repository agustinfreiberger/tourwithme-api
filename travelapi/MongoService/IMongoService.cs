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

        User GetUsuario(string name);
        List<User> GetUsuarios();

        List<User> GetUsuariosCercanosYSimilares(Guid userId, double lat, double longitud);

    }
}

