namespace Cllearworks.COH.Models.Beacons
{
    public class BeaconModel
    {
        public int BeaconId { get; set; }
        public string MacAddress { get; set; }
        public string Uuid { get; set; }
        public int? Major { get; set; }
        public int? Minor { get; set; }
        public BeaconTypes BeaconType { get; set; }
        public bool IsActive { get; set; }
        public int PlaceId { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }

        public string PlaceName { get; set; }
        public string DepartmentName { get; set; }
    }
}
