using Cllearworks.COH.BusinessManager;
using Cllearworks.COH.BusinessManager.Applications;
using Cllearworks.COH.BusinessManager.Attendances;
using Cllearworks.COH.BusinessManager.Beacons;
using Cllearworks.COH.BusinessManager.Clients;
using Cllearworks.COH.BusinessManager.Employees;
using Cllearworks.COH.BusinessManager.Permissions;
using Cllearworks.COH.BusinessManager.Shifts;
using Cllearworks.COH.BusinessManager.Places;
using Cllearworks.COH.BusinessManager.Tracks;
using Cllearworks.COH.BusinessManager.Users;
using Cllearworks.COH.Models.Attendances;
using Cllearworks.COH.Models.Beacons;
using Cllearworks.COH.Models.Clients;
using Cllearworks.COH.Models.Employees;
using Cllearworks.COH.Models.Shifts;
using Cllearworks.COH.Models.Places;
using Cllearworks.COH.Models.Tracks;
using Cllearworks.COH.Models.Users;
using Cllearworks.COH.Repository;
using Cllearworks.COH.Repository.Applications;
using Cllearworks.COH.Repository.Attendances;
using Cllearworks.COH.Repository.Beacons;
using Cllearworks.COH.Repository.Clients;
using Cllearworks.COH.Repository.Employees;
using Cllearworks.COH.Repository.Permissions;
using Cllearworks.COH.Repository.Shifts;
using Cllearworks.COH.Repository.Places;
using Cllearworks.COH.Repository.Tracks;
using Cllearworks.COH.Repository.Users;
using Cllearworks.COH.Web.Utility;
using Cllearworks.COH.Web.Utility.Exceptions;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Cllearworks.COH.Repository.Departments;
using Cllearworks.COH.BusinessManager.Departments;
using Cllearworks.COH.Models.Departments;
using Cllearworks.COH.BusinessManager.ShiftHistory;
using Cllearworks.COH.Repository.ShiftHistory;
using Cllearworks.COH.Models.ShiftHistory;
using Cllearworks.COH.Repository.ShiftHistories;
using Cllearworks.COH.Repository.Reports;
using Cllearworks.COH.BusinessManager.Reports;
using Cllearworks.COH.Repository.Dashboard;
using Cllearworks.COH.BusinessManager.Dashboard;
using Cllearworks.COH.Repository.Holidays;
using Cllearworks.COH.BusinessManager.Holidays;
using Cllearworks.COH.Models.Holidays;
using Cllearworks.COH.Repository.Leaves;
using Cllearworks.COH.BusinessManager.Leaves;
using Cllearworks.COH.Models.Leaves;

namespace Cllearworks.COH.API
{
    /// <summary>
    /// 
    /// </summary>
    public static class WebApiConfig
    {
        internal static Container DiContainer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public static void Register(HttpConfiguration config)
        {
            var corsAttr = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(corsAttr);

            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Use a custom exception handler to
            config.Services.Replace(typeof(IExceptionHandler), new COHApiExceptionHandler());

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            ConfigureDependencyInjection(config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        public static void ConfigureDependencyInjection(HttpConfiguration config)
        {
            // Simple Injector configuration
            // *****************************
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
            container.EnableHttpRequestMessageTracking(config);

            // Register types
            container.RegisterSingleton<IRequestMessageProvider>(new RequestMessageProvider(container));

            #region Repositories

            container.Register<IUsersRepository, UsersRepository>(Lifestyle.Scoped);
            container.Register<IApplicationRepository, ApplicationRepository>(Lifestyle.Scoped);
            container.Register<IAttendanceRepository, AttendanceRepository>(Lifestyle.Scoped);
            container.Register<IEmployeeRepository, EmployeeRepository>(Lifestyle.Scoped);
            container.Register<IBeaconRepository, BeaconRepository>(Lifestyle.Scoped);
            container.Register<ITrackRepository, TrackRepository>(Lifestyle.Scoped);
            container.Register<IClientRepository, ClientRepository>(Lifestyle.Scoped);
            container.Register<IPermissionRepository, PermissionRepository>(Lifestyle.Scoped);
            container.Register<IPlaceRepository, PlaceRepository>(Lifestyle.Scoped);
            container.Register<IDepartmentRepository, DepartmentRepository>(Lifestyle.Scoped);
            container.Register<IShiftHistoryRepository, ShiftHistoryRepository>(Lifestyle.Scoped);
            container.Register<IChangeRequestRepository, ChangeRequestRepository>(Lifestyle.Scoped);
            container.Register<IReportsRepository, ReportsRepository>(Lifestyle.Scoped);
            container.Register<IDashboardRepository, DashboardRepository>(Lifestyle.Scoped);
            container.Register<IShiftRepository, ShiftRepository>(Lifestyle.Scoped);
            container.Register<IHolidayRepository, HolidayRepository>(Lifestyle.Scoped);
            container.Register<ILeaveRepository, LeaveRepository>(Lifestyle.Scoped);

            #endregion

            #region Business Manager

            container.Register<IUsersManager, UsersManager>(Lifestyle.Scoped);
            container.Register<IApplicationManager, ApplicationManager>(Lifestyle.Scoped);
            container.Register<IAttendanceManager, AttendanceManager>(Lifestyle.Scoped);
            container.Register<IEmployeeManager, EmployeeManager>(Lifestyle.Scoped);
            container.Register<IBeaconManager, BeaconManager>(Lifestyle.Scoped);
            container.Register<ITrackManager, TrackManager>(Lifestyle.Scoped);
            container.Register<IClientManager, ClientManager>(Lifestyle.Scoped);
            container.Register<IPermissionManager, PermissionManager>(Lifestyle.Scoped);
            container.Register<IPlaceManager, PlaceManager>(Lifestyle.Scoped);
            container.Register<IDepartmentManager, DepartmentManager>(Lifestyle.Scoped);
            container.Register<IShiftHistoryManager, ShiftHistoryManager>(Lifestyle.Scoped);
            container.Register<IChangeRequestManager, ChangeRequestManager>(Lifestyle.Scoped);
            container.Register<IReportsManager, ReportsManager>(Lifestyle.Scoped);
            container.Register<IDashboardManager, DashboardManager>(Lifestyle.Scoped);
            container.Register<IShiftManager, ShiftManager>(Lifestyle.Scoped);
            container.Register<IHolidayManager, HolidayManager>(Lifestyle.Scoped);
            container.Register<ILeaveManager, LeaveManager>(Lifestyle.Scoped);

            #endregion

            #region Mapping Factory

            container.Register<IMappingFactory<User, UserModel, UserModel>, UsersMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Attendance, AttendanceModel, AttendanceModel>, AttendanceMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Employee, EmployeeListModel, EmployeeModel>, EmployeeMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Beacon, BeaconModel, BeaconModel>, BeaconMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Track, TrackModel, TrackModel>, TrackMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Client, ClientModel, ClientModel>, ClientMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Place, PlaceModel, PlaceModel>, PlaceMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Department, DepartmentModel, DepartmentModel>, DepartmentMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Employee, EmployeeUpdateModel, EmployeeUpdateModel>, EmployeeUpdateMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Employee, EmployeeRegisterModel, EmployeeRegisterModel>, EmployeeRegisterMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<ShiftEmployeeHistory, ShiftHistoryModel, ShiftHistoryModel>, ShiftHistoryMappingFactory >(Lifestyle.Scoped);
            container.Register<IMappingFactory<User, UserMeModel, UserMeModel>, UserMeMappingFactory> (Lifestyle.Scoped);
            container.Register<IMappingFactory<ChangeRequest, DeviceChangeRequestModel, DeviceChangeRequestModel>, DeviceChangeRequestMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Shift, ShiftModel, ShiftModel>, ShiftMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Holiday, HolidayModel, HolidayModel>, HolidayMappingFactory>(Lifestyle.Scoped);
            container.Register<IMappingFactory<Leave, LeaveModel, LeaveModel>, LeaveMappingFactory>(Lifestyle.Scoped);

            #endregion

            container.RegisterWebApiControllers(config);

            container.Verify();

            DiContainer = container;

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }

        private sealed class RequestMessageProvider : IRequestMessageProvider
        {
            private readonly Container _container;

            public RequestMessageProvider(Container container)
            {
                _container = container;
            }

            public HttpRequestMessage CurrentMessage
            {
                get { return _container.GetCurrentHttpRequestMessage(); }
            }
        }
    }
}
