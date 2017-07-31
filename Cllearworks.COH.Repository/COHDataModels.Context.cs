﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cllearworks.COH.Repository
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class COHEntities : DbContext
    {
        public COHEntities()
            : base("name=COHEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<Beacon> Beacons { get; set; }
        public virtual DbSet<ChangeRequest> ChangeRequests { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<HolidayDetail> HolidayDetails { get; set; }
        public virtual DbSet<Leave> Leaves { get; set; }
        public virtual DbSet<Place> Places { get; set; }
        public virtual DbSet<Track> Tracks { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Shift> Shifts { get; set; }
        public virtual DbSet<ShiftDetail> ShiftDetails { get; set; }
        public virtual DbSet<ShiftEmployeeHistory> ShiftEmployeeHistories { get; set; }
    
        public virtual ObjectResult<GetDailyAttendanceCount_Result> GetDailyAttendanceCount(Nullable<int> clientId, Nullable<int> placeId)
        {
            var clientIdParameter = clientId.HasValue ?
                new ObjectParameter("clientId", clientId) :
                new ObjectParameter("clientId", typeof(int));
    
            var placeIdParameter = placeId.HasValue ?
                new ObjectParameter("placeId", placeId) :
                new ObjectParameter("placeId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetDailyAttendanceCount_Result>("GetDailyAttendanceCount", clientIdParameter, placeIdParameter);
        }
    
        public virtual ObjectResult<GetAdvancedReport_Result> GetAdvancedReport(Nullable<int> clientId, Nullable<int> placeId, Nullable<int> departmentId, Nullable<int> shiftId, Nullable<int> employeeId, Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate, Nullable<int> lateBy)
        {
            var clientIdParameter = clientId.HasValue ?
                new ObjectParameter("clientId", clientId) :
                new ObjectParameter("clientId", typeof(int));
    
            var placeIdParameter = placeId.HasValue ?
                new ObjectParameter("placeId", placeId) :
                new ObjectParameter("placeId", typeof(int));
    
            var departmentIdParameter = departmentId.HasValue ?
                new ObjectParameter("departmentId", departmentId) :
                new ObjectParameter("departmentId", typeof(int));
    
            var shiftIdParameter = shiftId.HasValue ?
                new ObjectParameter("shiftId", shiftId) :
                new ObjectParameter("shiftId", typeof(int));
    
            var employeeIdParameter = employeeId.HasValue ?
                new ObjectParameter("employeeId", employeeId) :
                new ObjectParameter("employeeId", typeof(int));
    
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("startDate", startDate) :
                new ObjectParameter("startDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("endDate", endDate) :
                new ObjectParameter("endDate", typeof(System.DateTime));
    
            var lateByParameter = lateBy.HasValue ?
                new ObjectParameter("lateBy", lateBy) :
                new ObjectParameter("lateBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetAdvancedReport_Result>("GetAdvancedReport", clientIdParameter, placeIdParameter, departmentIdParameter, shiftIdParameter, employeeIdParameter, startDateParameter, endDateParameter, lateByParameter);
        }
    
        public virtual ObjectResult<GetYearlyAttendanceByEmployee_Result> GetYearlyAttendanceByEmployee(Nullable<int> employeeId, Nullable<int> year)
        {
            var employeeIdParameter = employeeId.HasValue ?
                new ObjectParameter("employeeId", employeeId) :
                new ObjectParameter("employeeId", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetYearlyAttendanceByEmployee_Result>("GetYearlyAttendanceByEmployee", employeeIdParameter, yearParameter);
        }
    
        public virtual ObjectResult<GetYearlyAttendanceByEmployee_ForMobile_Result> GetYearlyAttendanceByEmployee_ForMobile(Nullable<int> employeeId, Nullable<int> year)
        {
            var employeeIdParameter = employeeId.HasValue ?
                new ObjectParameter("employeeId", employeeId) :
                new ObjectParameter("employeeId", typeof(int));
    
            var yearParameter = year.HasValue ?
                new ObjectParameter("year", year) :
                new ObjectParameter("year", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetYearlyAttendanceByEmployee_ForMobile_Result>("GetYearlyAttendanceByEmployee_ForMobile", employeeIdParameter, yearParameter);
        }
    
        public virtual ObjectResult<GetWeeklyAttendanceByEmployee_ForMobile_Result> GetWeeklyAttendanceByEmployee_ForMobile(Nullable<int> employeeId, Nullable<System.DateTime> minDate)
        {
            var employeeIdParameter = employeeId.HasValue ?
                new ObjectParameter("employeeId", employeeId) :
                new ObjectParameter("employeeId", typeof(int));
    
            var minDateParameter = minDate.HasValue ?
                new ObjectParameter("minDate", minDate) :
                new ObjectParameter("minDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetWeeklyAttendanceByEmployee_ForMobile_Result>("GetWeeklyAttendanceByEmployee_ForMobile", employeeIdParameter, minDateParameter);
        }
    
        public virtual ObjectResult<GetMonthlyReport_Result> GetMonthlyReport(Nullable<int> employeeId, Nullable<System.DateTime> startDate, Nullable<System.DateTime> endDate)
        {
            var employeeIdParameter = employeeId.HasValue ?
                new ObjectParameter("employeeId", employeeId) :
                new ObjectParameter("employeeId", typeof(int));
    
            var startDateParameter = startDate.HasValue ?
                new ObjectParameter("startDate", startDate) :
                new ObjectParameter("startDate", typeof(System.DateTime));
    
            var endDateParameter = endDate.HasValue ?
                new ObjectParameter("endDate", endDate) :
                new ObjectParameter("endDate", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetMonthlyReport_Result>("GetMonthlyReport", employeeIdParameter, startDateParameter, endDateParameter);
        }
    }
}
