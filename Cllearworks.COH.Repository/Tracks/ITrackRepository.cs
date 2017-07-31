using Cllearworks.COH.Models.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.Repository.Tracks
{
    public interface ITrackRepository
    {
        #region Mobile

        Task<Track> AddAsync(Track dataModel);
        Task<IEnumerable<Track>> GetAllByDayAsync(int id);
        Task<long> TodayInDuration(int employeeId);
        Task<long> TodayOutDuration(int employeeId);

        #endregion Mobile

        //This is get all tracks by attendance Id
        Task<IQueryable<Track>> GetTracksByAttendanceIdAsync(int attendanceid);
    }
}
