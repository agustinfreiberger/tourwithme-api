using System;
using System.Collections.Generic;

namespace travelapi.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Latitud { get; set; }

        public double Longitud { get; set; }

        public List<PlaceCategoryPreference> Preferences { get; set; }

    public User() { }

    }
}
