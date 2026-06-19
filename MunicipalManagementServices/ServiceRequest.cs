using System;

namespace MunicipalManagementServices
{
    /// <summary>
    /// Represents a citizen service request submitted to the municipality.
    /// </summary>
    public class ServiceRequest : IComparable<ServiceRequest>
    {
        public string   RequestId   { get; set; }   // Unique identifier e.g. SR-00001
        public string   Title       { get; set; }
        public string   Category    { get; set; }
        public string   Location    { get; set; }
        public string   Description { get; set; }
        public string   Status      { get; set; }   // Pending, In Progress, Resolved, Closed
        public string   Approval    { get; set; }   // Pending, Approved, Rejected
        public int      Priority    { get; set; }   // 1 = highest
        public DateTime SubmittedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // For graph relationships — which ward/district this request belongs to
        public string Ward { get; set; }

        public ServiceRequest()
        {
            Title = string.Empty; Category = string.Empty;
            Location = string.Empty; Description = string.Empty;
            Status = "Pending"; Ward = string.Empty; Approval = "Pending";
        }

        // Used by BST / AVL tree ordering (by RequestId lexicographically)
        public int CompareTo(ServiceRequest other)
        {
            if (other == null) return 1;
            return string.Compare(this.RequestId, other.RequestId, StringComparison.Ordinal);
        }

        public override string ToString() => $"{RequestId} — {Title} [{Status}]";
    }
}
