using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Models.Tracks
{
    public class TrackDetailModel
    {
        public int Id { get; set; }
        public int AttandanceId { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public bool IsIn { get; set; }
        public bool IsOut { get; set; }
        public double TrackDuration { get; set; }
        public int FromBeacon { get; set; }
        public string Status { get; set; }
        public string PlaceName { get; set; }
        public string DepartmentName { get; set; }
    }
}
