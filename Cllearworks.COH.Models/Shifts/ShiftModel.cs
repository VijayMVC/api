using System;
using System.Collections.Generic;

namespace Cllearworks.COH.Models.Shifts
{
    public class ShiftModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<WeekDayModel> ShiftDetails { get; set; }
    }

    public class WeekDayModel
    {
        public bool IsWorkingDay { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? WorkingHours { get; set; }
        public decimal? BreakHours { get; set; }
    }
}
