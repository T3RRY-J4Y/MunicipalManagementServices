using System;
using System.Collections.Generic;
using System.Linq;

namespace MunicipalManagementServices
{
    /// <summary>
    /// All advanced data structures for Part 2:
    /// SortedDictionary, Dictionary (hash table), Queue, Stack,
    /// Priority Queue (via SortedDictionary), HashSet (set), Recommendation engine.
    /// </summary>
    public class EventDataStore
    {
        // 1. SORTED DICTIONARY — events keyed by date, auto-sorted O(log n)
        private SortedDictionary<DateTime, List<LocalEvent>> _eventsByDate =
            new SortedDictionary<DateTime, List<LocalEvent>>();

        // 2. DICTIONARY / HASH TABLE — O(1) lookup by ID
        private Dictionary<int, LocalEvent> _eventsById =
            new Dictionary<int, LocalEvent>();

        // 3. DICTIONARY — O(1) category index (hash table)
        private Dictionary<string, List<LocalEvent>> _byCategory =
            new Dictionary<string, List<LocalEvent>>(StringComparer.OrdinalIgnoreCase);

        // 4. QUEUE — announcements in FIFO arrival order
        private Queue<LocalEvent> _announcements = new Queue<LocalEvent>();

        // 5. STACK — recently viewed, LIFO
        private Stack<LocalEvent> _recentlyViewed = new Stack<LocalEvent>();

        // 6. PRIORITY QUEUE — min-heap via SortedDictionary (lower int = more urgent)
        private SortedDictionary<int, Queue<LocalEvent>> _priorityQueue =
            new SortedDictionary<int, Queue<LocalEvent>>();

        // 7. SET — unique category names
        private HashSet<string> _categories =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 8. RECOMMENDATION — search term frequency map
        private Dictionary<string, int> _searchFreq =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private int _nextId = 1;

        public EventDataStore() { SeedData(); }

        // ── ADD ──────────────────────────────────────────────
        public void AddEvent(LocalEvent ev)
        {
            ev.Id = _nextId++;
            _eventsById[ev.Id] = ev;

            var key = ev.EventDate.Date;
            if (!_eventsByDate.ContainsKey(key)) _eventsByDate[key] = new List<LocalEvent>();
            _eventsByDate[key].Add(ev);

            if (!_byCategory.ContainsKey(ev.Category)) _byCategory[ev.Category] = new List<LocalEvent>();
            _byCategory[ev.Category].Add(ev);

            _categories.Add(ev.Category);

            if (!_priorityQueue.ContainsKey(ev.Priority)) _priorityQueue[ev.Priority] = new Queue<LocalEvent>();
            _priorityQueue[ev.Priority].Enqueue(ev);

            if (ev.IsAnnouncement) _announcements.Enqueue(ev);
        }

        // ── ADMIN: lookup, edit, delete ───────────────────────
        public LocalEvent FindById(int id)
            => _eventsById.ContainsKey(id) ? _eventsById[id] : null;

        // Rebuild every index from the master dictionary. Called after an edit
        // or delete so all structures (date order, category index, priority
        // queue, announcement queue, category set) stay consistent.
        private void RebuildIndexes()
        {
            var all = new List<LocalEvent>(_eventsById.Values);

            _eventsByDate.Clear();
            _byCategory.Clear();
            _categories.Clear();
            _priorityQueue.Clear();
            _announcements.Clear();

            foreach (var ev in all.OrderBy(e => e.Id))
            {
                var key = ev.EventDate.Date;
                if (!_eventsByDate.ContainsKey(key)) _eventsByDate[key] = new List<LocalEvent>();
                _eventsByDate[key].Add(ev);

                if (!_byCategory.ContainsKey(ev.Category)) _byCategory[ev.Category] = new List<LocalEvent>();
                _byCategory[ev.Category].Add(ev);

                _categories.Add(ev.Category);

                if (!_priorityQueue.ContainsKey(ev.Priority)) _priorityQueue[ev.Priority] = new Queue<LocalEvent>();
                _priorityQueue[ev.Priority].Enqueue(ev);

                if (ev.IsAnnouncement) _announcements.Enqueue(ev);
            }
        }

        public bool UpdateEvent(int id, string title, string category, DateTime date,
                                string location, string description, bool isAnnouncement, int priority)
        {
            if (!_eventsById.ContainsKey(id)) return false;
            var ev = _eventsById[id];
            ev.Title          = title;
            ev.Category       = category;
            ev.EventDate      = date;
            ev.Location       = location;
            ev.Description    = description;
            ev.IsAnnouncement = isAnnouncement;
            ev.Priority       = priority;
            RebuildIndexes();
            return true;
        }

        public bool DeleteEvent(int id)
        {
            if (!_eventsById.ContainsKey(id)) return false;
            _eventsById.Remove(id);
            RebuildIndexes();
            return true;
        }

        // ── ALL EVENTS (SortedDictionary — date ordered) ─────
        public List<LocalEvent> GetAllEventsSorted()
        {
            var result = new List<LocalEvent>();
            foreach (var kvp in _eventsByDate)
                result.AddRange(kvp.Value.OrderBy(e => e.Id));
            return result;
        }

        // ── SEARCH (records frequency for recommendations) ───
        public List<LocalEvent> Search(string term, string category, DateTime? date)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                string k = term.Trim().ToLower();
                _searchFreq[k] = _searchFreq.ContainsKey(k) ? _searchFreq[k] + 1 : 1;
            }

            IEnumerable<LocalEvent> r = GetAllEventsSorted();

            if (!string.IsNullOrWhiteSpace(term))
            {
                string lo = term.ToLower();
                r = r.Where(e => e.Title.ToLower().Contains(lo) ||
                                 e.Description.ToLower().Contains(lo) ||
                                 e.Location.ToLower().Contains(lo) ||
                                 e.Category.ToLower().Contains(lo));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All Categories")
                r = r.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

            if (date.HasValue)
                r = r.Where(e => e.EventDate.Date == date.Value.Date);

            return r.ToList();
        }

        // ── RECOMMENDATIONS ───────────────────────────────────
        public List<LocalEvent> GetRecommendations(int n = 6)
        {
            if (_searchFreq.Count == 0) return GetAllEventsSorted().Take(n).ToList();

            var top = _searchFreq.OrderByDescending(kv => kv.Value).Take(3).Select(kv => kv.Key).ToList();
            var scored = new Dictionary<int, int>();

            foreach (var ev in GetAllEventsSorted())
            {
                int s = 0;
                foreach (var t in top)
                {
                    if (ev.Title.ToLower().Contains(t))       s += 3;
                    if (ev.Category.ToLower().Contains(t))    s += 2;
                    if (ev.Description.ToLower().Contains(t)) s += 1;
                }
                if (ev.EventDate >= DateTime.Today) s += 2;
                scored[ev.Id] = s;
            }

            return scored.Where(kv => kv.Value > 0)
                         .OrderByDescending(kv => kv.Value)
                         .Take(n)
                         .Select(kv => _eventsById[kv.Key])
                         .ToList();
        }

        // ── RECENTLY VIEWED (Stack) ───────────────────────────
        public void RecordView(LocalEvent ev)
        {
            if (_recentlyViewed.Count > 0 && _recentlyViewed.Peek().Id == ev.Id) return;
            _recentlyViewed.Push(ev);
            if (_recentlyViewed.Count > 10)
            {
                var tmp = _recentlyViewed.ToList(); tmp.RemoveAt(tmp.Count - 1);
                _recentlyViewed = new Stack<LocalEvent>(Enumerable.Reverse(tmp));
            }
        }
        public List<LocalEvent> GetRecentlyViewed() => _recentlyViewed.ToList();

        // ── ANNOUNCEMENTS (Queue) ─────────────────────────────
        public List<LocalEvent> GetAnnouncements() => _announcements.ToList();

        // ── PRIORITY EVENTS ───────────────────────────────────
        public List<LocalEvent> GetPriorityEvents(int count = 20)
        {
            var r = new List<LocalEvent>();
            foreach (var kvp in _priorityQueue)
                foreach (var ev in kvp.Value) { r.Add(ev); if (r.Count >= count) return r; }
            return r;
        }

        // ── CATEGORIES (Set) ──────────────────────────────────
        public List<string> GetCategories()
        {
            var list = _categories.OrderBy(c => c).ToList();
            list.Insert(0, "All Categories");
            return list;
        }

        public int TotalEvents    => _eventsById.Count;
        public int TotalSearches  => _searchFreq.Values.Sum();

        // ── SEED DATA ─────────────────────────────────────────
        private void SeedData()
        {
            AddEvent(new LocalEvent { Title = "Water Outage in Zone 3", Category = "Utilities", EventDate = DateTime.Today.AddDays(1), Location = "Benoni East", Description = "Scheduled maintenance will cause a water outage from 08:00 to 16:00. Residents are advised to store water in advance.", IsAnnouncement = true, Priority = 1 });
            AddEvent(new LocalEvent { Title = "Load Shedding Schedule at Stage 3", Category = "Utilities", EventDate = DateTime.Today.AddDays(2), Location = "All Areas", Description = "Stage 3 load shedding will be in effect. Please check the Eskom schedule for your area's specific times.", IsAnnouncement = true, Priority = 1 });
            AddEvent(new LocalEvent { Title = "Street Light Outage on Tom Jones St", Category = "Utilities", EventDate = DateTime.Today, Location = "Tom Jones Street", Description = "Several street lights are out. Technicians have been dispatched and repairs are expected within 48 hours.", IsAnnouncement = true, Priority = 1 });
            AddEvent(new LocalEvent { Title = "Road Closure at Main Reef Road", Category = "Roads", EventDate = DateTime.Today.AddDays(3), Location = "Main Reef Road", Description = "Road resurfacing works will close Main Reef Road from Northmead Ave to Jenkins Ave. Use Tom Jones Street as alternative route.", IsAnnouncement = true, Priority = 2 });
            AddEvent(new LocalEvent { Title = "Pothole Repair Programme in Ward 7", Category = "Roads", EventDate = DateTime.Today.AddDays(4), Location = "Ward 7 Streets", Description = "The municipality will systematically patch potholes across Ward 7. Intermittent single-lane closures may apply. Expected duration: 5 days.", IsAnnouncement = false, Priority = 2 });
            AddEvent(new LocalEvent { Title = "Waste Collection Holiday Schedule", Category = "Sanitation", EventDate = DateTime.Today.AddDays(8), Location = "All Areas", Description = "Collection days will shift by one day during the upcoming public holiday week. Monday collections move to Tuesday, and so forth.", IsAnnouncement = true, Priority = 2 });
            AddEvent(new LocalEvent { Title = "Community Clean Up Drive", Category = "Community", EventDate = DateTime.Today.AddDays(5), Location = "Rynfield Park", Description = "Join your neighbours for a community park clean-up! Gloves and refuse bags will be provided. Refreshments served. All welcome.", IsAnnouncement = false, Priority = 3 });
            AddEvent(new LocalEvent { Title = "Heritage Day Celebration", Category = "Community", EventDate = DateTime.Today.AddDays(20), Location = "Lakefield Showgrounds", Description = "Cultural performances, food stalls, local artisan market, and traditional music. Free entry. Family-friendly event.", IsAnnouncement = false, Priority = 4 });
            AddEvent(new LocalEvent { Title = "Youth Skills Development Workshop", Category = "Education", EventDate = DateTime.Today.AddDays(7), Location = "Benoni Civic Centre", Description = "Free CV writing, interview coaching, and digital literacy workshop for youth aged 18–35. Register at the Civic Centre or call 011-742-8000.", IsAnnouncement = false, Priority = 3 });
            AddEvent(new LocalEvent { Title = "Free Health Screening Clinic", Category = "Health", EventDate = DateTime.Today.AddDays(10), Location = "Northmead Mall Parking", Description = "Free blood pressure, diabetes, and HIV screening. No appointment necessary. Bring your ID. Available 08:00–15:00.", IsAnnouncement = false, Priority = 2 });
            AddEvent(new LocalEvent { Title = "Public Budget Meeting 2025/26", Category = "Governance", EventDate = DateTime.Today.AddDays(14), Location = "Benoni Town Hall", Description = "Citizens are invited to attend and participate in the municipal budget presentation. Your voice shapes our community's future.", IsAnnouncement = false, Priority = 2 });
            AddEvent(new LocalEvent { Title = "Free Legal Aid Clinic", Category = "Social Services", EventDate = DateTime.Today.AddDays(12), Location = "Benoni Magistrate Court", Description = "Free legal advice on tenant rights, consumer protection, and family law. First-come first-served. Bring all relevant documents.", IsAnnouncement = false, Priority = 3 });
            AddEvent(new LocalEvent { Title = "Park Renovation in Northmead Gardens", Category = "Community", EventDate = DateTime.Today.AddDays(30), Location = "Northmead Gardens", Description = "Major renovation of Northmead Gardens begins next month: new playground equipment, lighting, and walking paths.", IsAnnouncement = false, Priority = 4 });
            AddEvent(new LocalEvent { Title = "Water Quality Report Released", Category = "Utilities", EventDate = DateTime.Today.AddDays(-2), Location = "Municipal Website", Description = "The quarterly water quality test results are available. All zones passed safety standards. Full report available at ekurhuleni.gov.za.", IsAnnouncement = true, Priority = 3 });
        }
    }
}
