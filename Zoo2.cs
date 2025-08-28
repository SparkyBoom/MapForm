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
        {
            return $"{Name} the {Species} [{Kind}] | Enclosure: {Enclosure}";
        }
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

        public Visitor(string name)
        {
            Name = name;
        }

        public void Spend(decimal amount)
        {
            MoneySpent += amount;
        }

        public override string ToString()
        {
            return $"{Name} (Spent: {MoneySpent:C})";
        }
    }

    // --- Worker base ---
    public enum Role { Doctor, Feeder, Cleaner }

    public abstract class Worker
    {
        public string Name { get; }
        public Role Role { get; }
        public int StartTime { get; set; }
        public int EndTime { get { return StartTime + Duration; } }
        public Animal AssignedAnimal { get; private set; }
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

        public override string ToString()
        {
            return $"{Name} ({Role}) [{StartTime}-{EndTime}]";
        }
    }

    // --- Worker types ---
    public class Doctor : Worker { public Doctor(string name) : base(name, Role.Doctor, 5) { } }
    public class Feeder : Worker { public Feeder(string name) : base(name, Role.Feeder, 10) { } }
    public class Cleaner : Worker { public Cleaner(string name) : base(name, Role.Cleaner, 2) { } }

    // --- Simulation ---
    public class ZooSimulation
    {
        // Singleton
        private static ZooSimulation _instance;
        public static ZooSimulation Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("Simulation not initialized.");
                return _instance;
            }
        }

        private readonly List<Animal> _animals;
        private readonly List<Worker> _workers;
        private readonly List<Visitor> _visitors;
        private readonly Random _rng = new Random();
        private readonly List<Tour> _tours = new List<Tour>();
        private readonly Dictionary<string, List<string>> _conflictingSpecies;

        private const int DayLength = 120;
        private const int TourLength = 10;
        private const int GuestCount = 5;
        private decimal _totalEarnings;

        // --- Tour inner class ---
        private class Tour
        {
            public string Species;
            public int StartTime;
            public int Duration;
            public Worker Worker;

            public Tour(string species, int startTime, int duration, Worker worker)
            {
                Species = species;
                StartTime = startTime;
                Duration = duration;
                Worker = worker;
            }
        }

        public ZooSimulation(List<Animal> animals, List<Worker> workers, List<Visitor> visitors)
        {
            if (_instance != null)
                throw new InvalidOperationException("ZooSimulation is already created!");
            _instance = this;

            // Only one animal per species
            _animals = animals.GroupBy(a => a.Species).Select(g => g.First()).ToList();
            _workers = workers;
            _visitors = visitors;

            _conflictingSpecies = new Dictionary<string, List<string>>();
            _conflictingSpecies["Lion"] = new List<string> { "Dolphin" };
            _conflictingSpecies["Dolphin"] = new List<string> { "Lion" };
            // Add more conflicts here

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

                        // Prevent overlapping workers on same animal
                        if (_workers.All(w => w.AssignedAnimal != animal) && worker.TryAssign(animal, time))
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

            Console.WriteLine($"Total earnings: {_totalEarnings:C}");
        }

        private void RunTour(Worker worker, Animal animal, int startTime)
        {
            // Skip tour if conflicting species tour is running
            foreach (var t in _tours)
            {
                if (t.StartTime <= startTime && t.StartTime + TourLength > startTime)
                {
                    if (_conflictingSpecies.ContainsKey(t.Species) && _conflictingSpecies[t.Species].Contains(animal.Species))
                        return;
                }
            }

            decimal price = worker.Role == Role.Doctor ? 30m :
                            worker.Role == Role.Feeder ? 20m :
                            10m;

            foreach (var v in _visitors)
            {
                v.Spend(price);
                _totalEarnings += price;
                Console.WriteLine($"  [{startTime}] {v.Name} paid {price:C} for watching {worker.Role} with {animal.Name}");
            }

            _tours.Add(new Tour(animal.Species, startTime, TourLength, worker));
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

            new ZooSimulation(animals, workers, visitors);
            ZooSimulation.Instance.Run();
        }
    }
}
