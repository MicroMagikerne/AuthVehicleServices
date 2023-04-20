using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;

namespace VehicleTest
{
    public class VehicleController
    {
        private readonly IMongoCollection<Vehicle> _vehicles;

         // En logger, der bruges til at logge beskeder i applikationen.
        private readonly ILogger<VehicleController> _logger;

        // App-konfigurationen, som kan indeholde forskellige indstillinger og oplysninger.
        private readonly IConfiguration _config;


        // Initialisering af forbindelsen til MongoDB-databasen og initialisering af Vehicles-samlingen i databasen

        public VehicleController(ILogger<VehicleController> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;

            // Opret en forbindelse til MongoDB-databasen - admin gøres til miljøvariabel:
            var mongoClient = new MongoClient(_config["connectionstring"]);
            //database gøres til en miljøvariabel
            var database = mongoClient.GetDatabase(_config["database"]);

            // Opret en samling (collection) for Vehicles - vehicles bliver gjort til en miljøvariabel:
            _vehicles = database.GetCollection<Vehicle>(_config["collection"]);
        }

//Metoder til Vehicle:

        // Metode til at tilføje en ny bil til Vehicles-samlingen i databasen med nogle grundlæggende oplysninger og nulstilling af service- og billedhistorik

        [Authorize]
        public void AddVehicle(string brand, string model, string registrationNumber, int mileage)
        {
            int id = _vehicles.AsQueryable().Count() + 1;
            var vehicle = new Vehicle(id, brand, model, registrationNumber, mileage);
            vehicle.ServiceHistory = new List<ServiceHistory>();
            vehicle.ImageHistory = new List<Image>();
            _vehicles.InsertOne(vehicle);
        }

        // Metode til at returnere en liste over alle biler i Vehicles-samlingen i databasen

         [Authorize]
        public List<Vehicle> GetAllVehicles()
        {
            return _vehicles.Find(_ => true).ToList();
        }

        // Metode til at returnere en bestemt bil i Vehicles-samlingen i databasen baseret på dens id

         [Authorize]
        public Vehicle GetVehicleById(int id)
        {
            return _vehicles.Find(vehicle => vehicle.ID == id).FirstOrDefault();
        }

//Metode til Image:

        // Metode til at tilføje et nyt billede til billedhistorikken for en bestemt bil i Vehicles-samlingen i databasen baseret på dens id

         [Authorize]
        public void AddImageToVehicle(int vehicleId, string location, DateTime timestamp, string description, string addedBy)
        {
            var vehicle = GetVehicleById(vehicleId);
            if (vehicle != null)
            {
                var image = new Image(vehicle.ImageHistory.Count + 1, location, timestamp, description, addedBy);
                vehicle.ImageHistory.Add(image);
                var filter = Builders<Vehicle>.Filter.Eq(v => v.ID, vehicleId);
                var update = Builders<Vehicle>.Update.Set(v => v.ImageHistory, vehicle.ImageHistory);
                _vehicles.UpdateOne(filter, update);
            }
        }

//Metode til ServiceHistory:

        // Metode til at tilføje en ny servicehistorik til en bestemt bil i Vehicles-samlingen i databasen baseret på dens id

         [Authorize]
        public void AddServiceHistoryToVehicle(int vehicleId, DateTime timestamp, string description, string servicedBy)
        {
            var vehicle = GetVehicleById(vehicleId);
            if (vehicle != null)
            {
                var serviceHistory = new ServiceHistory(vehicle.ServiceHistory.Count + 1, timestamp, description, servicedBy);
                vehicle.ServiceHistory.Add(serviceHistory);
                var filter = Builders<Vehicle>.Filter.Eq(v => v.ID, vehicleId);
                var update = Builders<Vehicle>.Update.Set(v => v.ServiceHistory, vehicle.ServiceHistory);
                _vehicles.UpdateOne(filter, update);
            }
        }
    }
}
