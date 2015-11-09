﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entitron.Sql
{
    public class SqlQuery_Select : SqlQuery_Selectable<SqlQuery_Select>
    {
        public List<string> columns { get; set; }
        
        protected override List<DBItem> BaseExecutionWithRead(MarshalByRefObject connection)
        {
            string parAppName = safeAddParam("applicationName", application.Name);
            string parTableName = safeAddParam("tableName", table.tableName);

            sqlString = string.Format(
                "DECLARE @realTableName NVARCHAR(50),@sql NVARCHAR(MAX);exec getTableRealName @{0}, @{1}, @realTableName OUTPUT;" +
                "SET @sql = CONCAT('SELECT {2} FROM ', @realTableName, ' {3} {4} {5} {6};');" +
                "exec sp_executesql @sql, N'{7}', {8};", 
                parAppName,parTableName,
                (columns != null && columns.Count > 0) ? string.Join(",", columns) : "*",
                _where.ToString(),
                string.Join(" ", _join),
                _group,
                _order,
                string.Join(", ", _datatypes.Select(d => "@" + d.Key + " " + d.Value)),
                string.Join(", ", _datatypes.Select(d => "@" + d.Key))
                );
            
            return base.BaseExecutionWithRead(connection);
        }

        protected override List<DBItem> Read(SqlDataReader reader)
        {
            List<DBItem> items = new List<DBItem>();

            while (reader.Read())
            {
                DBItem newItem = new DBItem();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    int columnId = table.columns.Single(x => x.Name == columnName).ColumnId;
                    newItem.createProperty(columnId, columnName, reader[columnName]);
                }

                items.Add(newItem);
            }

            return items;
        }

        public List<DBItem> ToList()
        {
            List<DBItem> output = ExecuteWithRead();
            
            foreach (DBItem item in output)
            {
                item.table = table;
            }
            return output;
        }
        public DBItem First()
        {
            DBItem output = ExecuteWithRead().First();
            output.table = table;

            return output;
        }

        public override string ToString()
        {
            return string.Format("Select row in {0}[{1}]", table.tableName, application.Name);
        }
    }
}
