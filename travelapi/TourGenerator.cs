
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using travelapi.Models;
using travelapi.MongoService;

namespace travelapi
{
    public class TourGenerator
    {
        IMongoService mongoService;

        public TourGenerator(IMongoService mongoService)
        {
            this.mongoService = mongoService;
        }

        public List<Poi> GenerateTour(List<User> users)
        {
            List<Poi> tour = new List<Poi>();
            List<Poi> pois = new List<Poi>();

            if (users.Count == 0)
            {
                return tour;
            }
            else 
            {
                List<Poi> Pois = mongoService.GetPoisCercanos(CalculateCenter(users));
            }


            // Calculate the average preference scores and range for each Poi
            Dictionary<int, (double AverageScore, double Range)> PoiPreferenceScores = new Dictionary<int, (double, double)>();
            
            foreach (var Poi in pois)
            {
                double totalScore = 0;
                double maxScore = double.MinValue;
                double minScore = double.MaxValue;

                foreach (var user in users)
                {
                    var preference = user.Preferences.FirstOrDefault(p => p.Preference == Poi.id);
                    if (preference != null)
                    {
                        totalScore += preference.Preference;
                        maxScore = Math.Max(maxScore, preference.Preference);
                        minScore = Math.Min(minScore, preference.Preference);
                    }
                }

                if (maxScore != int.MinValue && minScore != int.MaxValue)
                {
                    double averageScore = totalScore / users.Count;
                    double range = maxScore - minScore;
                    PoiPreferenceScores[Poi.id] = (averageScore, range);
                }
            }

            // Sort Pois by a weighted score in descending order
            var sortedPois = pois.OrderByDescending(a =>
            {
                if (PoiPreferenceScores.ContainsKey(a.id))
                {
                    var (averageScore, range) = PoiPreferenceScores[a.id];
                    // Adjust the weight according to the range of preferences
                    return averageScore + (0.1 * range);
                }
                return 0.0;
            });

            // Generate the tour by adding Pois in the sorted order
            foreach (var Poi in sortedPois)
            {
                tour.Add(Poi);
            }

            return tour;
        }

        private static Coordinate CalculateCenter(List<User> users)
        {
            if (users.Count == 0)
            {
                throw new ArgumentException("The list of users is empty.");
            }

            double totalLatitude = 0;
            double totalLongitude = 0;

            foreach (var user in users)
            {
                totalLatitude += user.Latitud;
                totalLongitude += user.Longitud;
            }

            double centerLatitude = totalLatitude / users.Count;
            double centerLongitude = totalLongitude / users.Count;

            Coordinate c = new Coordinate(centerLatitude, centerLongitude);
            return c;
        }

        private async void LoadPlaces() {
            string city = "Tandil"; // Location you want to search for attractions.

            using (HttpClient client = new HttpClient())
            {
                // Define the OSM API endpoint for searching attractions.
                string osmApiEndpoint = $"https://nominatim.openstreetmap.org/search?format=json&q=attraction+in+{city}";
                
                
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.");
                // Send a GET request to the OSM API.
                HttpResponseMessage response = await client.GetAsync(osmApiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("Error: Unable to retrieve data from the OSM API.");
                }
            }
        }
    }
}
