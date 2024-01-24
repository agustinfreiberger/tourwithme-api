using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using travelapi.Models;
using travelapi.MongoService;

namespace travelapi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GroupController : ControllerBase
    {
        private IMongoService _usersRepo;
        

        public GroupController(IMongoService mongoService)
        {
            this._usersRepo = mongoService;
            
        }


        [HttpPost]
        public ActionResult<string> CrearUsuario(User usuario)
        {
            return _usersRepo.AltaUsuario(usuario);
        }

        [HttpPut]
        public ActionResult<string> ModificarUsuario(User usuario)
        {
            return _usersRepo.ModificarUsuario(usuario);
        }

      
        [HttpDelete]
        public ActionResult<string> EliminarUsuario(Guid id)
        {
            return _usersRepo.EliminarUsuario(id);
        }


        [HttpGet]
        public ActionResult<User> GetUsuario(string name) => _usersRepo.GetUsuario(name);


        [HttpGet]
        public ActionResult<List<User>> GetUsuarios() => _usersRepo.GetUsuarios();

        [HttpGet]
        public ActionResult<List<User>> GetUsuariosCercanosYSimilares(Guid Id, double lat, double longitud) => _usersRepo.GetUsuariosCercanosYSimilares(Id, lat, longitud);

    }
}
