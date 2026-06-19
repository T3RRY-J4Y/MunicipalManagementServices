using System;
using System.Collections.Generic;

namespace MunicipalManagementServices
{
    public class LocalEvent : IComparable<LocalEvent>
    {
        public int      Id             { get; set; }
        public string   Title          { get; set; }
        public string   Category       { get; set; }
        public DateTime EventDate      { get; set; }
        public string   Location       { get; set; }
        public string   Description    { get; set; }
        public bool     IsAnnouncement { get; set; }
        public int      Priority       { get; set; }

        public LocalEvent()
        {
            Title = string.Empty; Category = string.Empty;
            Location = string.Empty; Description = string.Empty;
        }

        public int CompareTo(LocalEvent other)
        {
            if (other == null) return 1;
            int d = EventDate.CompareTo(other.EventDate);
            return d != 0 ? d : Id.CompareTo(other.Id);
        }

        public override string ToString() =>
            $"{(IsAnnouncement ? "[ANN]" : "[EVT]")} {Title} — {EventDate:dd MMM yyyy}";
    }
}
