﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSS.Omnius.Modules.Entitron.Sql
{
    class SqlQuery_SelectStringsTypes:SqlQuery
    {
        protected override List<DBItem> BaseExecutionWithRead(MarshalByRefObject connection)
        {

            sqlString = "SELECT name FROM sys.types WHERE max_length=8000;";

            return base.BaseExecutionWithRead(connection);
        }
    }
}
