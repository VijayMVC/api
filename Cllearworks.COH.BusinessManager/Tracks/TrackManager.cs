using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cllearworks.COH.Models.Tracks;
using Cllearworks.COH.Repository.Tracks;
using Cllearworks.COH.Repository;
using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.Models.Users;

namespace Cllearworks.COH.BusinessManager.Tracks
{
    public class TrackManager : ITrackManager
    {
        public ITrackRepository _trackRepository;
        public IMappingFactory<Track, TrackModel, TrackModel> _trackMapper;
        private readonly IPermissionManager _permissionManager;

        public TrackManager(ITrackRepository trackRepository, IMappingFactory<Track, TrackModel, TrackModel> trackMapper,
             IPermissionManager permissionManager)
        {
            _trackRepository = trackRepository;
            _trackMapper = trackMapper;
            _permissionManager = permissionManager;
        }

        #region Mobile

        public async Task<IEnumerable<TrackModel>> AddCollectionAsync(IEnumerable<TrackModel> model)
        {
            if (model == null)
                return null;

            var dataModelList = new List<Track>();
            foreach (TrackModel trackModel in model)
            {
                var dataModel = await _trackRepository.AddAsync(_trackMapper.ConvertToDataModel(trackModel));
                dataModelList.Add(dataModel);
            }

            var modelList = new List<TrackModel>();
            foreach (Track track in dataModelList)
            {
                var trackModel = _trackMapper.ConvertToModel(track);
                modelList.Add(trackModel);
            }
            return modelList;
        }

        public async Task<IEnumerable<TrackModel>> GetAllByDayAsync(int employeeId)
        {
            // return await Task.Run(() => {
            var dataModel = await _trackRepository.GetAllByDayAsync(employeeId);
            IEnumerable<TrackModel> trackModel = dataModel.Select(dm => _trackMapper.ConvertToModel(dm));
            return trackModel;
            //});
        }

        public async Task<long> TodayInDuration(int employeeId)
        {
            return await _trackRepository.TodayInDuration(employeeId);
        }
        public async Task<long> TodayOutDuration(int employeeId)
        {
            return await _trackRepository.TodayOutDuration(employeeId);
        }

        #endregion Mobile

        public async Task<IEnumerable<TrackDetailModel>> GetTracksByAttendanceIdAsync(int clientId, int userId, int attendanceid)
        {
            if (!await _permissionManager.HasPermission(clientId, userId, Permission.CanViewHRUserReport))
                throw new Exception("User has not permission to perform this operation");


            var tracksByAttendance = await _trackRepository.GetTracksByAttendanceIdAsync(attendanceid);

            var list = new List<TrackDetailModel>();

            foreach (var item in tracksByAttendance)
            {
                var model = new TrackDetailModel();
                model.Id = item.Id;
                model.AttandanceId = item.AttendanceId;
                model.FromTime = item.FromTime.HasValue ? item.FromTime.Value : default(DateTime);
                model.ToTime = item.ToTime.HasValue ? item.ToTime.Value : default(DateTime);
                model.IsIn = item.IsIn.HasValue ? item.IsIn.Value : default(bool);
                model.IsOut = item.IsOut.HasValue ? item.IsOut.Value : default(bool);
                model.TrackDuration = item.TrackDuration.HasValue ? new TimeSpan(item.TrackDuration.Value).TotalMinutes : 0;
                model.FromBeacon = item.FromBeacon;
                model.Status = item.Status;                
                model.DepartmentName = item.Beacon.Department.Name;
                model.PlaceName = item.Beacon.Place.Name;
                list.Add(model);
            }

            return list;
        }
    }
}
