using System;
using System.Collections.Generic;

namespace MunicipalManagementServices
{
    /// <summary>
    /// Single shared data layer for the whole application.
    /// Ensures the citizen-facing forms and the Admin area all see the same live data.
    /// </summary>
    public static class AppData
    {
        public static readonly EventDataStore       Events          = new EventDataStore();
        public static readonly ServiceRequestStore  ServiceRequests = new ServiceRequestStore();
        public static readonly List<ReportedIssue>  Issues          = new List<ReportedIssue>();
    }

    /// <summary>
    /// Very small in-memory authentication helper for the Admin area.
    /// In a production system these credentials would live in a secure store,
    /// but for this project a hard-coded staff account is sufficient.
    /// </summary>
    public static class AdminAuth
    {
        // Demo staff credentials
        private const string ADMIN_USER = "admin";
        private const string ADMIN_PASS = "admin123";

        public static bool IsLoggedIn { get; private set; }

        public static bool Validate(string username, string password)
        {
            bool ok = username == ADMIN_USER && password == ADMIN_PASS;
            if (ok) IsLoggedIn = true;
            return ok;
        }

        public static void LogOut() => IsLoggedIn = false;
    }
}
