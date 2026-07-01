using CIMC.Data.ExtModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace CIMC.Data
{
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string UserId { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string UserName { get; set; }

        public DateTime OperationTime { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string IpAddress { get; set; }

        [StringLength(ModelUnits.Len_20)]
        public string OperationType { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string OperationModule { get; set; }

        [StringLength(ModelUnits.Len_200)]
        public string OperationDesc { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string RequestUrl { get; set; }

        [StringLength(ModelUnits.Len_20)]
        public string HttpMethod { get; set; }

        public string RequestData { get; set; }

        public string OldData { get; set; }

        public string NewData { get; set; }

        [StringLength(ModelUnits.Len_20)]
        public string ResultStatus { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string ResultMessage { get; set; }

        [StringLength(ModelUnits.Len_64)]
        public string DataHash { get; set; }

        public long Duration { get; set; }

        [StringLength(ModelUnits.Len_500)]
        public string UserAgent { get; set; }

        [StringLength(ModelUnits.Len_100)]
        public string OperationTable { get; set; }

        [StringLength(ModelUnits.Len_50)]
        public string RecordId { get; set; }

        public bool IsArchived { get; set; }

        [StringLength(ModelUnits.Len_64)]
        public string PreviousHash { get; set; }
    }
}
