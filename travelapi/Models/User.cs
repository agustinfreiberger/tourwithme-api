using NetTopologySuite.Geometries;

namespace travelapi.Models
{
    public class User
    {
        public string Name { get; set; }
        public Coordinate Location { get; set; }

        public User() { }

    }
}
