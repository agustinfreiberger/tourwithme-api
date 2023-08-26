using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using travelapi.Models;

namespace travelapi.MongoService
{
    public interface IMongoService
    {
        string AltaUsuario(User u);

        string ModificarUsuario(User u);

        string EliminarUsuario(Guid promId);

        User Get(Guid promocionId);
        List<User> Gets();

        List<User> GetUsuariosCercanos(double x, double y);
    }
}

