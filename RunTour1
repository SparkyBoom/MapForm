private void RunTour(Worker worker, Animal animal, int startTime)
{
    // --- Check conflicts with existing tours ---
    foreach (var t in _tours)
    {
        bool overlaps = t.StartTime <= startTime && (t.StartTime + t.Duration) > startTime;
        if (overlaps)
        {
            List<AnimalKind> blocked;
            if (_conflicts.TryGetValue(t.Kind, out blocked) && blocked.Contains(animal.Kind))
            {
                Console.WriteLine("  [" + startTime + "] Tour skipped due to conflict with " + t.Kind);
                return; // conflict → skip this tour
            }
        }
    }

    // --- Determine price ---
    int price;
    if (worker != null)
        price = worker.TicketPrice;
    else
        price = 15;

    // --- Pick exactly 5 participants ---
    var participants = _visitors.OrderBy(v => _rng.Next()).Take(5).ToList();

    foreach (var v in participants)
    {
        v.Spend(price);
        _totalEarnings += price;
        if (worker != null)
            Console.WriteLine("  [" + startTime + "] " + v.Name + " paid " + price + " to watch " + worker.Role + " with " + animal.Name);
        else
            Console.WriteLine("  [" + startTime + "] " + v.Name + " paid " + price + " to watch " + animal.Name + " alone");
    }

    // --- Track this tour if worker exists ---
    if (worker != null)
        _tours.Add(new Tour(animal.Kind, startTime, TourLength, worker));
}
