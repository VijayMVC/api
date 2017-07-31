using Cllearworks.COH.Models.Beacons;
using Cllearworks.COH.Models.Tracks;
using Cllearworks.COH.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cllearworks.COH.BusinessManager.Tracks
{
    public class TrackMappingFactory : IMappingFactory<Track, TrackModel, TrackModel>
    {
        public Track ConvertToDataModel(TrackModel model)
        {
            if (model == null)
                return null;

            var dataModel = new Track();
            dataModel.Id = model.Id;
            dataModel.AttendanceId = model.AttandanceId;
            dataModel.FromTime = model.FromTime;
            dataModel.ToTime = model.ToTime;
            dataModel.IsIn = model.IsIn;
            dataModel.IsOut = model.IsOut;
            //dataModel.TrackDuration = model.TrackDuration;
            dataModel.FromBeacon = model.FromBeacon;
            dataModel.Status = model.Status;

            return dataModel;
        }

        public TrackModel ConvertToListModel(Track dataModel)
        {
            throw new NotImplementedException();
        }

        public TrackModel ConvertToModel(Track dataModel)
        {
            if (dataModel == null)
                return null;

            var model = new TrackModel();
            model.Id = dataModel.Id;
            model.AttandanceId = dataModel.AttendanceId;
            model.FromTime = dataModel.FromTime != null ? dataModel.FromTime.Value : default(DateTime);
            model.ToTime = dataModel.ToTime != null ? dataModel.ToTime.Value : default(DateTime);
            model.IsIn = dataModel.IsIn != null ? dataModel.IsIn.Value : default(bool);
            model.IsOut = dataModel.IsOut != null ? dataModel.IsOut.Value : default(bool);
            model.TrackDuration = dataModel.TrackDuration.HasValue ? new TimeSpan(dataModel.TrackDuration.Value).TotalMinutes : 0;
            model.FromBeacon = dataModel.FromBeacon;
            model.Status = dataModel.Status;

            return model;
        }
    }
}
