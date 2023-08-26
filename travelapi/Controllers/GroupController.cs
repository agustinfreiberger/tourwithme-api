using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult<User> VerUsuario(Guid promId) => _usersRepo.Get(promId);


        [HttpGet]
        public ActionResult<List<User>> VerUsuarios() => _usersRepo.Gets();


        [HttpGet]
        public string VerUsuariosCercanos(double x, double y) //url= /group/verusuarioscercanos?x=-37.32084072165888&y=-59.14231321160657
        {
            var usuarios = _usersRepo.GetUsuariosCercanos(x,y);

            if (x != 0 && y != 0)
            {

                if (usuarios.Count > 0)
                {

                    List<User> usuariosCercanos = usuarios.ToList(); 

                    return JsonConvert.SerializeObject(usuariosCercanos, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
            }
            return "";
        }
    }
}
