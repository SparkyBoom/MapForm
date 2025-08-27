using System;
using System.Collections.Generic;
using System.Linq;

namespace ZooDemo
{
    // --- Animal setup ---
    public enum AnimalKind { Land, Flying, Aquatic }

    public abstract class Animal
    {
        public string Name { get; }
        public string Species { get; }
        public AnimalKind Kind { get; }
        public string Enclosure { get; }

        protected Animal(string name, string species, AnimalKind kind, string enclosure)
        {
            Name = name;
            Species = species;
            Kind = kind;
            Enclosure = enclosure;
        }

        public override string ToString()
            => $"{Name} the {Species} [{Kind}] | Enclosure: {Enclosure}";
    }

    public class ZooAnimal : Animal
    {
        public ZooAnimal(string name, string species, AnimalKind kind, string enclosure)
            : base(name, species, kind, enclosure) { }
    }

    // --- Visitor ---
    public class Visitor
    {
        public string Name { get; }
        public decimal MoneySpent { get; private set; }

        public Visitor(string name) => Name = name;

        public void Spend(decimal amount) => MoneySpent += amount;

        public override string ToString() => $"{Name} (Spent: {MoneySpent:C})";
    }

    // --- Worker base ---
    public enum Role { Doctor, Feeder, Cleaner }

    public abstract class Worker
    {
        public string Name { get; }
        public Role Role { get; }
        public int StartTime { get; set; }
        public int EndTime => StartTime + Duration;

        public Animal? AssignedAnimal { get; private set; }
        public int Duration { get; }

        protected Worker(string name, Role role, int duration)
        {
            Name = name;
            Role = role;
            Duration = duration;
        }

        public bool TryAssign(Animal animal, int currentTime)
        {
            if (AssignedAnimal != null || currentTime < StartTime || currentTime >= EndTime)
                return false;

            AssignedAnimal = animal;
            return true;
        }

        public void Release(int currentTime)
        {
            if (AssignedAnimal != null && currentTime >= EndTime)
                AssignedAnimal = null;
        }

        public override string ToString() => $"{Name} ({Role}) [{StartTime}-{EndTime}]";
    }

    // --- Worker types ---
    public class Doctor : Worker
    {
        public Doctor(string name) : base(name, Role.Doctor, 5) { }
    }

    public class Feeder : Worker
    {
        public Feeder(string name) : base(name, Role.Feeder, 10) { }
    }

    public class Cleaner : Worker
    {
        public Cleaner(string name) : base(name, Role.Cleaner, 2) { }
    }

    // --- Simulation ---
    public class ZooSimulation
    {
        private readonly List<Animal> _animals;
        private readonly List<Worker> _workers;
        private readonly List<Visitor> _visitors;
        private readonly Random _rng = new();

        private const int DayLength = 120;
        private const int TourLength = 10;

        public ZooSimulation(List<Animal> animals, List<Worker> workers, List<Visitor> visitors)
        {
            _animals = animals;
            _workers = workers;
            _visitors = visitors;

            foreach (var w in _workers)
                w.StartTime = _rng.Next(0, DayLength - w.Duration);
        }

        public void Run()
        {
            Console.WriteLine("=== Zoo Day Simulation ===");

            for (int time = 0; time < DayLength; time++)
            {
                foreach (var worker in _workers)
                {
                    if (time == worker.StartTime)
                    {
                        var animal = _animals[_rng.Next(_animals.Count)];
                        if (worker.TryAssign(animal, time))
                        {
                            Console.WriteLine($"[{time}] {worker} started working with {animal.Name} ({animal.Species})");
                            RunTour(worker, animal, time);
                        }
                    }

                    worker.Release(time);
                }
            }

            Console.WriteLine("=== Day Finished ===");
            foreach (var v in _visitors)
                Console.WriteLine(v);
        }

        private void RunTour(Worker worker, Animal animal, int startTime)
        {
            decimal price = worker.Role switch
            {
                Role.Doctor => 30m,
                Role.Feeder => 20m,
                Role.Cleaner => 10m,
                _ => 15m
            };

            foreach (var v in _visitors)
            {
                if (_rng.NextDouble() < 0.5)
                {
                    v.Spend(price);
                    Console.WriteLine($"  [{startTime}] {v.Name} paid {price:C} for {TourLength} time units to watch {worker.Role} with {animal.Name}");
                }
            }
        }
    }

    // --- Program ---
    internal static class Program
    {
        private static void Main()
        {
            var animals = new List<Animal>
            {
                new ZooAnimal("Skye", "Eagle", AnimalKind.Flying, "Aviary A"),
                new ZooAnimal("Splash", "Dolphin", AnimalKind.Aquatic, "Aquarium 1"),
                new ZooAnimal("Simba", "Lion", AnimalKind.Land, "Savannah 2"),
                new ZooAnimal("Momo", "Penguin", AnimalKind.Aquatic, "Penguin Pool")
            };

            var workers = new List<Worker>
            {
                new Doctor("Dr. Maya"),
                new Feeder("Alex"),
                new Cleaner("Rina")
            };

            var visitors = new List<Visitor>
            {
                new Visitor("Alice"),
                new Visitor("Bob"),
                new Visitor("Carla")
            };

            var simulation = new ZooSimulation(animals, workers, visitors);
            simulation.Run();
        }
    }
}
