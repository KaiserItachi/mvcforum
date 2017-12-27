﻿namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using DomainModel.Entities;
    using DomainModel.General;

    public partial interface IRoleService
    {
        IList<MembershipRole> AllRoles();
        void Delete(MembershipRole role);
        MembershipRole GetRole(string rolename, bool removeTracking = false);
        MembershipRole GetRole(Guid id);
        IList<MembershipUser> GetUsersForRole(string roleName);
        MembershipRole CreateRole(MembershipRole role);
        PermissionSet GetPermissions(Category category, MembershipRole role);
    }
}