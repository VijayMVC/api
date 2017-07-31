using System;

namespace Cllearworks.COH.Models.Tracks
{
    public class TrackModel
    {
        public int Id { get; set; }
        public int AttandanceId { get; set; }
        //public int EmployeeId { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public bool IsIn { get; set; }
        public bool IsOut { get; set; }
        public double TrackDuration { get; set; }
        public int FromBeacon { get; set; }
        public string Status { get; set; }
    }
}
