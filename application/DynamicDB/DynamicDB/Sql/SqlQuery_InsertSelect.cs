﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDB.Sql
{
    class SqlQuery_InsertSelect:SqlQuery_Selectable
    {
        public string table1Name { get; set; }
        public string table2Name { get; set; }
        public List<string> columns1 { get; set; }
        public List<string> columns2 { get; set; } 

        public SqlQuery_InsertSelect(string ApplicationName) : base(ApplicationName)
        {
        }

        protected override void BaseExecution(MarshalByRefObject transaction)
        {
            string parAppName = safeAddParam("applicationName", _applicationName);
            string parTable1Name = safeAddParam("tableName", table1Name);
            string parTable2Name = safeAddParam("tableName", table2Name);
            string parColumn1Name = safeAddParam("columnName", string.Join(", ", columns1));
            string parColumn2Name = safeAddParam("columnName", string.Join(", ", columns2));
            string parWhere = safeAddParam("where", _where);

            _sqlString = string.Format(
                "DECLARE @realTable1Name NVARCHAR(50), @realTable2Name NVARCHAR(50), @sql NVARCHAR(MAX); exec getRealTableName @{0}, @{1}, @realTable1Name OUTPUT; exec getRealTableName @{0}, @{2}, @realTable2Name OUTPUT;" +
                "SET @sql=CONCAT('INSERT INTO ', @realTable1Name, '({3}) SELECT ', @realTable2Name, '({4})', @{5}, ';'"+
                "exec (@sql)",
                parAppName,parTable1Name,parTable2Name, parColumn1Name, parColumn2Name, parWhere);

            base.BaseExecution(transaction);
        }
    }
}
