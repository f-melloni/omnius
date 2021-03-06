﻿using System.Collections.Generic;
using FSS.Omnius.Modules.CORE;
using FSS.Omnius.Modules.Entitron.DB;

namespace FSS.Omnius.Modules.Tapestry.Actions.Entitron
{
    [EntitronRepository]
    public class GetDictionaryFromTableAction : Action
    {
        public override int Id => 1028;

        public override int? ReverseActionId => null;

        public override string[] InputVar => new string[] { "TableData", "KeyColumn", "ValueColumn", "?StaticKey", "?StaticValue" };

        public override string Name => "Get dictionary from table";

        public override string[] OutputVar => new string[] { "Result" };

        public override void InnerRun(Dictionary<string, object> vars, Dictionary<string, object> outputVars, Dictionary<string, object> InvertedInputVars, Message message)
        {
            var result = new Dictionary<string, string>();
            if (vars.ContainsKey("StaticKey") && vars.ContainsKey("StaticValue"))
            {
                string key = vars["StaticKey"].ToString();
                string value = vars["StaticValue"].ToString();
                if (result.ContainsKey(key))
                    result[key] = value;
                else
                    result.Add(key, value);
            }
            else if (vars["TableData"] is DBItem)
            {
                var row = (DBItem)vars["TableData"];
                string key = (string)row[(string)vars["KeyColumn"]];
                string value = (string)row[(string)vars["ValueColumn"]];
                if (result.ContainsKey(key))
                    result[key] = value;
                else
                    result.Add(key, value);
            }
            else
            {
                foreach (var row in (List<DBItem>)vars["TableData"])
                {
                    string key = (string)row[(string)vars["KeyColumn"]];
                    string value = (string)row[(string)vars["ValueColumn"]];
                    if (result.ContainsKey(key))
                        result[key] = value;
                    else
                        result.Add(key, value);
                }
            }
            outputVars["Result"] = result;
        }
    }
}
