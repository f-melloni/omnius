﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSS.Omnius.Modules.Entitron.Entity.Tapestry;

namespace FSS.Omnius.Modules.Entitron.Entity.Persona
{
    public partial class User
    {
        public bool isInGroup(string groupName)
        {
            return Groups.Any(g => g.Name == groupName);
        }

        public bool canUseAction(int actionId, DBEntities e)
        {
            var right = e.ActionRuleRights.FirstOrDefault(ar => ar.ActionRuleId == actionId && Groups.Contains(ar.Group));
            return right.Executable;
        }
    }
}
