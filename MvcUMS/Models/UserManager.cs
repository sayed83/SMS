using MvcEnergyPac.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcUMS.Models
{
    public class UserManager
    {
        public bool isUserInRole(string userName, string roleName)
        {
            using (UsersContext db = new UsersContext())
            {
                UserProfile usrProfile = db.UserProfiles.Where(s => s.UserName.Equals(userName)).FirstOrDefault();
                if (usrProfile != null)
                {
                    var roles = from r in db.webpages_Roles
                                join u in db.webpages_UsersInRoles on r.RoleId equals u.RoleId
                                where r.RoleName.Equals(roleName) && u.UserId.Equals(usrProfile.UserId)
                                select r.RoleName;

                    if(roles !=null)
                    {
                        return roles.Any();
                    }
                }

                return false;
            }
        }
    }
}