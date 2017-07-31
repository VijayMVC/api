using System.Collections.Generic;

namespace Cllearworks.COH.Models.Users
{
    public static class Permission
    {
        public const string CanViewSuperAdminDashboard = "CanViewSuperAdminDashboard";
        public const string CanViewClientAdminDashboard = "CanViewClientAdminDashboard";

        public const string CanViewClient = "CanViewClient";
        public const string CanAddClient = "CanAddClient";
        public const string CanEditClient = "CanEditClient";
        public const string CanDeleteClient = "CanDeleteClient";

        public const string CanViewUser = "CanViewUser";
        public const string CanAddUser = "CanAddUser";
        public const string CanEditUser = "CanEditUser";
        public const string CanDeleteUser = "CanDeleteUser";

        public const string CanViewPlace = "CanViewPlace";
        public const string CanAddPlace = "CanAddPlace";
        public const string CanEditPlace = "CanEditPlace";
        public const string CanDeletePlace = "CanDeletePlace";

        public const string CanViewEmployee = "CanViewEmployee";
        public const string CanAddEmployee = "CanAddEmployee";
        public const string CanEditEmployee = "CanEditEmployee";
        public const string CanDeleteEmployee = "CanDeleteEmployee";

        public const string CanViewShift = "CanViewShift";
        public const string CanAddShift = "CanAddShift";
        public const string CanEditShift = "CanEditShift";
        public const string CanDeleteShift = "CanDeleteShift";

        public const string CanViewBeacon = "CanViewBeacon";
        public const string CanAddBeacon = "CanAddBeacon";
        public const string CanEditBeacon = "CanEditBeacon";
        public const string CanDeleteBeacon = "CanDeleteBeacon";
        public const string CanEditBeaconDetails = "CanEditBeaconDetails"; //this is using for disable the beaconsdetails other then name

        public const string CanViewDepartment = "CanViewDepartment";
        public const string CanAddDepartment = "CanAddDepartment";
        public const string CanEditDepartment = "CanEditDepartment";
        public const string CanDeleteDepartment = "CanDeleteDepartment";

        public const string CanViewSuperAdminReport = "CanViewSuperAdminReport";
        public const string CanViewClientAdminReport = "CanViewClientAdminReport";
        public const string CanViewHRUserReport = "CanViewHRUserReport";

        public const string CanViewHoliday = "CanViewHoliday";
        public const string CanAddHoliday = "CanAddHoliday";
        public const string CanEditHoliday = "CanEditHoliday";
        public const string CanDeleteHoliday = "CanDeleteHoliday";

        public const string CanViewLeave = "CanViewLeave";
        public const string CanAddLeave = "CanAddLeave";
        public const string CanEditLeave = "CanEditLeave";
        public const string CanDeleteLeave = "CanDeleteLeave";


        public static Dictionary<string, bool> GetPermissions(UserRoles role)
        {
            var permissions = new Dictionary<string, bool>();
            switch (role)
            {
                case UserRoles.SuperAdmin:

                    permissions.Add(CanViewSuperAdminDashboard, true);
                    permissions.Add(CanViewClientAdminDashboard, true);

                    permissions.Add(CanViewClient, true);
                    permissions.Add(CanAddClient, true);
                    permissions.Add(CanEditClient, true);
                    permissions.Add(CanDeleteClient, true);

                    permissions.Add(CanViewUser, true);
                    permissions.Add(CanAddUser, true);
                    permissions.Add(CanEditUser, true);
                    permissions.Add(CanDeleteUser, true);

                    permissions.Add(CanViewPlace, true);
                    permissions.Add(CanAddPlace, true);
                    permissions.Add(CanEditPlace, true);
                    permissions.Add(CanDeletePlace, true);
                                    
                    permissions.Add(CanViewEmployee, true);
                    permissions.Add(CanAddEmployee, true);
                    permissions.Add(CanEditEmployee, true);
                    permissions.Add(CanDeleteEmployee, true);
                                    
                    permissions.Add(CanViewShift, true);
                    permissions.Add(CanAddShift, true);
                    permissions.Add(CanEditShift, true);
                    permissions.Add(CanDeleteShift, true);
                                    
                    permissions.Add(CanViewBeacon, true);
                    permissions.Add(CanAddBeacon, true);
                    permissions.Add(CanEditBeacon, true);
                    permissions.Add(CanDeleteBeacon, true);
                    permissions.Add(CanEditBeaconDetails, true);

                    permissions.Add(CanViewDepartment, true);
                    permissions.Add(CanAddDepartment, true);
                    permissions.Add(CanEditDepartment, true);
                    permissions.Add(CanDeleteDepartment, true);
                                    
                    permissions.Add(CanViewSuperAdminReport, true);
                    permissions.Add(CanViewClientAdminReport, true);
                    permissions.Add(CanViewHRUserReport, true);

                    permissions.Add(CanViewHoliday, true);
                    permissions.Add(CanAddHoliday, true);
                    permissions.Add(CanEditHoliday, true);
                    permissions.Add(CanDeleteHoliday, true);

                    permissions.Add(CanViewLeave, true);
                    permissions.Add(CanAddLeave, true);
                    permissions.Add(CanEditLeave, true);
                    permissions.Add(CanDeleteLeave, true);

                    break;

                case UserRoles.ClientAdmin:

                    permissions.Add(CanViewSuperAdminDashboard, false);
                    permissions.Add(CanViewClientAdminDashboard, true);

                    permissions.Add(CanViewClient, false);
                    permissions.Add(CanAddClient, false);
                    permissions.Add(CanEditClient, false);
                    permissions.Add(CanDeleteClient, false);

                    permissions.Add(CanViewUser, true);
                    permissions.Add(CanAddUser, true);
                    permissions.Add(CanEditUser, true);
                    permissions.Add(CanDeleteUser, true);

                    permissions.Add(CanViewPlace, true);
                    permissions.Add(CanAddPlace, false);
                    permissions.Add(CanEditPlace, true);
                    permissions.Add(CanDeletePlace, false);

                    permissions.Add(CanViewEmployee, true);
                    permissions.Add(CanAddEmployee, true);
                    permissions.Add(CanEditEmployee, true);
                    permissions.Add(CanDeleteEmployee, true);

                    permissions.Add(CanViewShift, true);
                    permissions.Add(CanAddShift, true);
                    permissions.Add(CanEditShift, true);
                    permissions.Add(CanDeleteShift, true);

                    permissions.Add(CanViewBeacon, true);
                    permissions.Add(CanAddBeacon, false);
                    permissions.Add(CanEditBeacon, true);
                    permissions.Add(CanDeleteBeacon, false);
                    permissions.Add(CanEditBeaconDetails, false);

                    permissions.Add(CanViewDepartment, true);
                    permissions.Add(CanAddDepartment, true);
                    permissions.Add(CanEditDepartment, true);
                    permissions.Add(CanDeleteDepartment, true);

                    permissions.Add(CanViewSuperAdminReport, false);
                    permissions.Add(CanViewClientAdminReport, true);
                    permissions.Add(CanViewHRUserReport, true);

                    permissions.Add(CanViewHoliday, true);
                    permissions.Add(CanAddHoliday, true);
                    permissions.Add(CanEditHoliday, true);
                    permissions.Add(CanDeleteHoliday, true);

                    permissions.Add(CanViewLeave, true);
                    permissions.Add(CanAddLeave, true);
                    permissions.Add(CanEditLeave, true);
                    permissions.Add(CanDeleteLeave, true);

                    break;

                case UserRoles.HRUser:

                    permissions.Add(CanViewSuperAdminDashboard, false);
                    permissions.Add(CanViewClientAdminDashboard, true);

                    permissions.Add(CanViewClient, false);
                    permissions.Add(CanAddClient, false);
                    permissions.Add(CanEditClient, false);
                    permissions.Add(CanDeleteClient, false);

                    permissions.Add(CanViewUser, false);
                    permissions.Add(CanAddUser, false);
                    permissions.Add(CanEditUser, false);
                    permissions.Add(CanDeleteUser, false);

                    permissions.Add(CanViewPlace, true);
                    permissions.Add(CanAddPlace, false);
                    permissions.Add(CanEditPlace, false);
                    permissions.Add(CanDeletePlace, false);

                    permissions.Add(CanViewEmployee, true);
                    permissions.Add(CanAddEmployee, true);
                    permissions.Add(CanEditEmployee, true);
                    permissions.Add(CanDeleteEmployee, true);

                    permissions.Add(CanViewShift, true);
                    permissions.Add(CanAddShift, true);
                    permissions.Add(CanEditShift, true);
                    permissions.Add(CanDeleteShift, true);

                    permissions.Add(CanViewBeacon, true);
                    permissions.Add(CanAddBeacon, false);
                    permissions.Add(CanEditBeacon, false);
                    permissions.Add(CanDeleteBeacon, false);
                    permissions.Add(CanEditBeaconDetails, false);

                    permissions.Add(CanViewDepartment, true);
                    permissions.Add(CanAddDepartment, false);
                    permissions.Add(CanEditDepartment, false);
                    permissions.Add(CanDeleteDepartment, false);

                    permissions.Add(CanViewSuperAdminReport, false);
                    permissions.Add(CanViewClientAdminReport, false);
                    permissions.Add(CanViewHRUserReport, true);

                    permissions.Add(CanViewHoliday, true);
                    permissions.Add(CanAddHoliday, true);
                    permissions.Add(CanEditHoliday, true);
                    permissions.Add(CanDeleteHoliday, true);

                    permissions.Add(CanViewLeave, true);
                    permissions.Add(CanAddLeave, true);
                    permissions.Add(CanEditLeave, true);
                    permissions.Add(CanDeleteLeave, true);

                    break;
            }
            return permissions;
        }
    }
}
