using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using travelapi.Models;
using travelapi.MongoService;

namespace travelapi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TourController : ControllerBase
    {
        private TourGenerator tourGenerator;

        public TourController(IMongoService mongoService) {
            tourGenerator = new TourGenerator(mongoService);
        }


        [HttpPost]
        public ActionResult<string> GetTour([FromBody] List<User> users)
        {

            var tour = tourGenerator.GenerateTour(users);

            if (tour.Count > 0)
            {
                return JsonConvert.SerializeObject(tour, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
            }

            return "";
        }
    }
}
