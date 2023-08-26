namespace travelapi.Models
{
    public class PlaceCategoryPreference
    {
        public int Placecategory { get; set; }

        public double Preference { get; set; }

        public PlaceCategoryPreference(int placeCategory, double preference) {
            this.Placecategory = placeCategory;
            this.Preference = preference;
        }
    }
}