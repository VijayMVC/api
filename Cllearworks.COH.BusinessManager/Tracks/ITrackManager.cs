using Cllearworks.COH.Models.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Tracks
{
    public interface ITrackManager
    {
        #region Mobile

        Task<IEnumerable<TrackModel>> AddCollectionAsync(IEnumerable<TrackModel> model);
        Task<IEnumerable<TrackModel>> GetAllByDayAsync(int employeeId);
        Task<long> TodayInDuration(int employeeId);
        Task<long> TodayOutDuration(int employeeId);

        #endregion Mobile
        Task<IEnumerable<TrackDetailModel>> GetTracksByAttendanceIdAsync(int clientId, int userId, int attendanceid);
    }
}
