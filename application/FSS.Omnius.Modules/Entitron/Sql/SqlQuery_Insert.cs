﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSS.Omnius.Modules.Entitron.Sql
{
    class SqlQuery_Insert : SqlQuery_withApp
    {
        public Dictionary<DBColumn, object> data { get; set; }
        
        protected override List<DBItem> BaseExecutionWithRead(MarshalByRefObject connection)
        {
            if (data == null || data.Count < 1)
                throw new ArgumentNullException("data");

            string parAppName = safeAddParam("AppName", application.Name);
            string parTableName = safeAddParam("tableName", table.tableName);
            Dictionary<DBColumn, string> values = safeAddParam(data);

            sqlString = string.Format(
                "DECLARE @realTableName NVARCHAR(50), @sql NVARCHAR(MAX); exec getTableRealName @{0}, @{1}, @realTableName OUTPUT;" +
                "SET @sql = CONCAT('INSERT INTO ', @realTableName, ' ({2}) Output Inserted.Id VALUES ({3}) ;');" +
                "exec sp_executesql @sql, N'{4}', {5};",
                parAppName, // 0
                parTableName, // 1
                string.Join(", ", values.Select(pair => "[" + pair.Key.Name + "]")), // 2
                string.Join(", ", values.Select(pair => "@" + pair.Value)), // 3
                string.Join(", ", _datatypes.Select(s => "@" + s.Key + " " + s.Value)),
                string.Join(", ", _datatypes.Select(s => "@" + s.Key))
                );

            return base.BaseExecutionWithRead(connection);
        }

        public int GetInsertedId()
        {
            return Convert.ToInt32(ExecuteWithRead().First()["Id"]);
        }

        public override string ToString()
        {
            return string.Format("Insert row in {0}[{1}]", table.tableName, application.Name);
        }
    }
}
