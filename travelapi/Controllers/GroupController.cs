using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public ActionResult<User> GetUsuario(Guid Id) => _usersRepo.GetUsuario(Id);


        [HttpGet]
        public ActionResult<List<User>> GetUsuarios() => _usersRepo.GetUsuarios();


        [HttpGet]
        public ActionResult<List<User>> GetUsuariosCercanos(double x, double y) //https://localhost:5001/Group/GetUsuariosCercanos?x=-37.32084072165888&y=-59.14231321160657
        {
            return _usersRepo.GetUsuariosCercanos(x, y);
        }
    }
}
