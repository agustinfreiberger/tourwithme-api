namespace travelapi.Models
{
    public class Poi
    {
        public int id { get; set; }
        public string name { get; set;}
        public string address { get; set; }
        public string category { get; set; }

        public double Latitud { get; set; }

        public double Longitud { get; set; }


        public Poi() { }

        public Poi(int id, string name, string address, string category) {
            this.id = id;
            this.name = name;
            this.address = address;
            this.category = category;
        }
    }
}
