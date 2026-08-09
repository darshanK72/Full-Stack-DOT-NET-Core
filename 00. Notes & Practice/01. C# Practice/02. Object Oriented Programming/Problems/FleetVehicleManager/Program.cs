/*
 * PROBLEM: Fleet Vehicle Manager
 *
 * Logistics tracks cars and trucks. Dispatch assigns routes; reporting sums
 * odometer-style distances using a value type wrapper.
 *
 * This exercise covers:
 *   ch05 — abstract base; virtual/override; inheritance hierarchy
 *   ch05 — operator overloading on Distance struct
 *   ch09 — vehicle hierarchy real-world example
 */

using System;
using System.Collections.Generic;

namespace FleetManagement
{
    /*
     * Immutable distance value type with + and - operators.
     */
    struct Distance
    {
        public double Kilometers { get; }

        public Distance(double kilometers)
        {
            // TODO: reject negative kilometers
            throw new NotImplementedException();
        }

        public static Distance operator +(Distance a, Distance b)
        {
            // TODO: return new Distance with summed kilometers
            throw new NotImplementedException();
        }

        public static Distance operator -(Distance a, Distance b)
        {
            // TODO: throw if result would be negative
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            // TODO: "{Kilometers:F1} km"
            throw new NotImplementedException();
        }
    }

    abstract class Vehicle
    {
        public string PlateNumber { get; }
        public string Make { get; }

        protected Distance _distance = new Distance(0);

        public Distance TotalDistance => _distance;

        public abstract string VehicleKind { get; }

        protected Vehicle(string plateNumber, string make)
        {
            PlateNumber = plateNumber;
            Make = make;
        }

        public virtual void Drive(Distance delta)
        {
            // TODO: add delta to _distance using overloaded +
            throw new NotImplementedException();
        }

        public virtual string Describe()
        {
            // TODO: "{VehicleKind}: {PlateNumber} ({Make}) — {TotalDistance}"
            throw new NotImplementedException();
        }
    }

    class Car : Vehicle
    {
        public int PassengerCapacity { get; }

        public override string VehicleKind => "Car";

        public Car(string plateNumber, string make, int passengerCapacity)
            : base(plateNumber, make)
        {
            // TODO: validate capacity >= 1
            throw new NotImplementedException();
        }

        public override string Describe()
        {
            // TODO: append passenger capacity to base description
            throw new NotImplementedException();
        }
    }

    class Truck : Vehicle
    {
        public double LoadCapacityTons { get; }

        public override string VehicleKind => "Truck";

        public Truck(string plateNumber, string make, double loadCapacityTons)
            : base(plateNumber, make)
        {
            // TODO: validate loadCapacityTons > 0
            throw new NotImplementedException();
        }

        public override void Drive(Distance delta)
        {
            // TODO: reject delta > 500 km per trip; else base.Drive
            throw new NotImplementedException();
        }
    }

    class FleetManager
    {
        private readonly List<Vehicle> _vehicles = new List<Vehicle>();

        public bool AddVehicle(Vehicle vehicle)
        {
            // TODO: false if duplicate PlateNumber (case-insensitive)
            throw new NotImplementedException();
        }

        public List<Vehicle> GetByKind(string kind)
        {
            // TODO: case-insensitive match on VehicleKind
            throw new NotImplementedException();
        }

        public Distance GetFleetTotalDistance()
        {
            // TODO: sum TotalDistance using overloaded +
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: add car and truck; drive each; print descriptions and fleet total
            throw new NotImplementedException();
        }
    }
}
