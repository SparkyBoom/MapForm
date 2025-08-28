using System;
using System.Collections.Generic;
using System.Linq;

namespace ZooDemo
{
    // --- Animal setup ---
    public abstract class Animal
    {
        public string Name { get; }
        public string Species { get; }
        public string Area { get; } // NEW: added Area for tours

        protected Animal(string name, string species, string area)
        {
            Name = name;
            Species = species;
            Area = area;
        }

        public abstract void MakeSound();

        public override string ToString() => Name + " the " + Species + " | Area: " + Area;
    }

    public class ZooAnimal : Animal
    {
        public ZooAnimal(string name, string species, string area) : base(name, species, area) { }

        public override void MakeSound()
        {
            Console.WriteLine(Name + " the " + Species + " makes a sound!");
        }
    }

    // --- Visitor ---
    public class Visitor
    {
        public string Name { get; }
        public int MoneySpent { get; set; } // NEW: track money spent

        public Visitor(string name)
        {
            Name = name;
        }
    }

    // --- Worker base ---
    public abstract class Worker
    {
        public string Name { get; }
        public Animal AssignedAnimal { get; protected set; }
        public int StartTime { get; set; } // NEW: start time
        public int Duration { get; protected set; } // NEW: duration
        public abstract int TicketPrice { get; } // NEW: ticket price per guest

        protected Worker(string name)
        {
            Name = name;
        }

        public abstract bool TryAssign(Animal animal, int time);
    }

    // --- Worker types ---
    public class Doctor : Worker
    {
        public override int TicketPrice { get { return 50; } }
        public Doctor(string name) : base(name) { Duration = 5; }
        public override bool TryAssign(Animal animal, int time)
        {
            AssignedAnimal = animal;
            return true;
        }
    }

    public class Feeder : Worker
    {
        public override int TicketPrice { get { return 30; } }
        public Feeder(string name) : base(name) { Duration = 10; }
        public override bool TryAssign(Animal animal, int time)
        {
            AssignedAnimal = animal;
            return true;
        }
    }

    public class Cleaner : Worker
    {
        public override int TicketPrice { get { return 10; } }
        public Cleaner(string name) : base(name) { Duration = 2; }
        public override bool TryAssign(Animal animal, int time)
        {
            AssignedAnimal = animal;
            return true;
        }
    }

    // --- Tour class ---
    public class Tour
    {
        public string Area { get; }
        public int StartTime { get; }
        public int Duration { get; }
        public Worker Worker { get; }

        public Tour(string area, int startTime, int duration, Worker worker)
        {
            Area = area;
            StartTime = startTime;
            Duration = duration;
            Worker = worker;
        }
    }

    // --- ZooSimulation singleton ---
    public class ZooSimulation
    {
        // --- Singleton implementation ---
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

        // --- Fields ---
        private List<Animal> _animals;
        private List<Worker> _workers;
        private List<Visitor> _visitors;
        private Random _rng = new Random(); // NEW: random number generator
        private List<Tour> _tours = new List<Tour>(); // NEW: track tours
        private Dictionary<string, List<string>> _conflictingAreas; // NEW: areas that can't overlap

        private const int DayLength = 120;
        private const int TourLength = 10;
        private const int GuestCount = 5;
        private int _totalEarnings = 0; // NEW: total zoo earnings

        // --- Constructor ---
        public ZooSimulation(List<Animal> animals, List<Worker> workers, List<Visitor> visitors)
        {
            if (_instance != null)
                throw new InvalidOperationException("ZooSimulation is already created!"); // NEW: singleton guard
            _instance = this; // NEW: set singleton

            // --- Ensure only one animal per species ---
            _animals = animals
                .GroupBy(a => a.Species)
                .Select(g => g.First())
                .ToList();

            _workers = workers;
            _visitors = visitors;

            // --- Initialize conflicting areas (space to define more) ---
            _conflictingAreas = new Dictionary<string, List<string>>();
            _conflictingAreas["Land"] = new List<string> { "Aquatic" };
            _conflictingAreas["Aquatic"] = new List<string> { "Land" };

            // --- Assign random start times to workers ---
            foreach (var w in _workers)
                w.StartTime = _rng.Next(0, DayLength - w.Duration);
        }

        // --- Run the full day simulation ---
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

                        // --- Prevent overlapping workers on same animal ---
                        if (_workers.All(w => w.AssignedAnimal != animal) && worker.TryAssign(animal, time))
                        {
                            Console.WriteLine(time + ": " + worker.Name + " started working with " + animal.Name);

                            // --- Run a tour for this animal ---
                            RunTour(animal, worker, time);
                        }
                    }

                    // --- Release worker after duration ---
                    if (worker.AssignedAnimal != null && time >= worker.StartTime + worker.Duration)
                        worker.AssignedAnimal = null;
                }
            }

            // --- End of day report ---
            Console.WriteLine("\n--- End of Day Report ---");
            Console.WriteLine("Total zoo revenue: " + _totalEarnings);
        }

        // --- Run a single tour ---
        private void RunTour(Animal animal, Worker worker, int startTime)
        {
            // --- Skip tour if area conflicts with existing tours ---
            bool conflict = false;
            foreach (var t in _tours)
            {
                if (t.StartTime <= startTime && t.StartTime + t.Duration > startTime)
                {
                    if (_conflictingAreas.ContainsKey(t.Area) && _conflictingAreas[t.Area].Contains(animal.Area))
                    {
                        conflict = true;
                        break;
                    }
                }
            }
            if (conflict) return;

            // --- Calculate earnings for 5 guests ---
            int price = worker.TicketPrice;
            int earnings = GuestCount * price;
            _totalEarnings += earnings;

            _tours.Add(new Tour(animal.Area, startTime, TourLength, worker));
            Console.WriteLine("Tour started in " + animal.Area + " at " + startTime +
                              ". Each guest paid " + price + ", total " + earnings + ".");
        }
    }

    // --- Program ---
    internal static class Program
    {
        private static void Main()
        {
            // --- Create animals ---
            var animals = new List<Animal>
            {
                new ZooAnimal("Skye", "Eagle", "Flying"),
                new ZooAnimal("Splash", "Dolphin", "Aquatic"),
                new ZooAnimal("Simba", "Lion", "Land"),
                new ZooAnimal("Momo", "Penguin", "Aquatic")
            };

            foreach (var animal in animals)
                animal.MakeSound(); // NEW: animal sound

            // --- Create workers ---
            var workers = new List<Worker>
            {
                new Doctor("Dr. Maya"),
                new Feeder("Alex"),
                new Cleaner("Rina")
            };

            // --- Create visitors ---
            var visitors = new List<Visitor>
            {
                new Visitor("Alice"),
                new Visitor("Bob"),
                new Visitor("Carla"),
                new Visitor("David"),
                new Visitor("Eva")
            };

            // --- Initialize singleton simulation ---
            new ZooSimulation(animals, workers, visitors);

            // --- Run the simulation ---
            ZooSimulation.Instance.Run();
        }
    }
}
