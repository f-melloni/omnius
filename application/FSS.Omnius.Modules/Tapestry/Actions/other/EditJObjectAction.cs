﻿using System;
using System.Collections.Generic;
using System.Linq;
using FSS.Omnius.Modules.CORE;
using Newtonsoft.Json.Linq;

namespace FSS.Omnius.Modules.Tapestry.Actions.other
{
    [OtherRepository]
    class EditJObjectAction : Action
    {
        public override int Id => 181111;

        public override string[] InputVar => new string[] { "v$JObject", "PropertyName[index]", "Value[index]" };

        public override string Name => "Edit JObject";

        public override string[] OutputVar => new string[] { "Result" };

        public override int? ReverseActionId => null;

        public override void InnerRun(Dictionary<string, object> vars, Dictionary<string, object> outputVars, Dictionary<string, object> InvertedInputVars, Message message)
        {
            JObject originalJObject = (JObject)vars["JObject"];
            JObject jObject = (JObject)originalJObject.DeepClone();

            int propertiesCount = vars.Keys.Where(k => k.StartsWith("PropertyName[") && k.EndsWith("]")).Count();
            int valuesCount = vars.Keys.Where(k => k.StartsWith("Value[") && k.EndsWith("]")).Count();
            if (propertiesCount != valuesCount)
                throw new Exception("Values count differs from properties count!");

            for (int i = 0; i < propertiesCount; i++)
            {
                string propertyName = (string)vars[$"PropertyName[{i}]"];
                var value = vars[$"Value[{i}]"];
                if (jObject.Property(propertyName) != null) {
                    jObject.Property(propertyName).Value = new JValue(value);
                }
                else {
                    jObject.Add(propertyName, new JValue(value));
                }
            }

            outputVars["Result"] = jObject;
        }
    }
}
