using System.Collections.Generic;

namespace CimcSite.Web.Config
{
    public class AuditLogConfig
    {
        public bool IsEnabled { get; set; } = true;

        public List<string> RecordOperations { get; set; } = new List<string>
        {
            "Login", "Logout", "Add", "Edit", "Delete", "Authorize", "Upload"
        };

        public bool ShouldRecord(string operationType)
        {
            if (!IsEnabled) return false;
            if (RecordOperations == null || RecordOperations.Count == 0) return true;
            return RecordOperations.Contains(operationType);
        }
    }
}
