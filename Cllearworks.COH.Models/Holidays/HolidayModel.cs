namespace Cllearworks.COH.Models.Holidays
{
    public class HolidayModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }

        public int? ToClient { get; set; }
        public int?[] ToPlace { get; set; }
        public int?[] ToDepartment { get; set; }
        public int?[] ToEmployee { get; set; }
    }
}
