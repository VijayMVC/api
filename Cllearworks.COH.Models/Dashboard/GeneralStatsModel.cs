namespace Cllearworks.COH.Models.Dashboard
{
    public class GeneralStatsModel
    {
        public int TotalRegisteredEmployees { get; set; }
        public int TotalNewRegisteredEmployees { get; set; }
        public int TotalDeviceChangeRequests { get; set; }

        /// <summary>
        /// Total employees who are ontime for today
        /// </summary>
        public int TotalOntimeEmployees { get; set; }

        /// <summary>
        /// Total employees who are late for today
        /// </summary>
        public int TotalLateEmployees { get; set; }
    }
}
