using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using travelapi.Models;

namespace travelapi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class GroupController : ControllerBase
    {
        private List<User> users;


        public GroupController(ILogger<GroupController> logger)
        {
            users = new List<User>();
            users.Add(new User { 
                Name = "Juan Perez",
                Location = new Coordinate (-37.33128596261828, -59.12601895464619)
            });
            users.Add(new User
            {
                Name = "Julia Rodriguez",
                Location = new Coordinate(-37.33215074715598, -59.108643119007645)
            });
            users.Add(new User
            {
                Name = "Nicolas Guemes",
                Location = new Coordinate(-37.32217884003679, -59.08329570115637)
            });
            users.Add(new User
            {
                Name = "Remedios del Valle",
                Location = new Coordinate(-37.2904947574382, -59.18372646731288)
            });
        }


        [HttpGet]
        [Route("")]
        public string UsuariosCercanos(double x, double y) //url= /group/usuarioscercanos?x=-37.32084072165888&y=-59.14231321160657
        {
            if (x != 0 && y!=0) { 

                if (users.Count >0) {

                    List<User> usuariosCercanos = users.Where(user => GreatCircleDistance(x,y,user.Location.X, user.Location.Y) < 4).ToList(); //distancia euclidea

                    return JsonConvert.SerializeObject(usuariosCercanos, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }
            }
            return "";
        }


        [HttpPost]
        public IActionResult AddUser([FromBody] User user)
        {
            users.Add(user);
            return Created("~api/users/1", user);
        }

        [HttpPost]
        public IActionResult RemoveUser([FromBody] User user)
        {
            users.Remove(user);
            return Created("~api/users/1", user);
        }

        public static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        public static double GreatCircleDistance(double lon1, double lat1, double lon2, double lat2)
        {
            double R = 6371e3; // m

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            double d = Math.Acos(cosD);

            double dist = (R * d)/1000; //paso metros a Km

            return dist;
        }

    }
}
