﻿using System;
using System.Collections.Generic;
using FSS.Omnius.Modules.CORE;

namespace FSS.Omnius.Modules.Tapestry.Actions.Math
{
    [MathRepository]
    class SubtractAction : Action
    {
        public override int Id => 4001;

        public override string[] InputVar => new string[] { "A", "B" };

        public override string Name => "Math: Subtract";

        public override string[] OutputVar => new string[] { "Result" };

        public override int? ReverseActionId => null;

        public override void InnerRun(Dictionary<string, object> vars, Dictionary<string, object> outputVars, Dictionary<string, object> InvertedInputVars, Message message)
        {
            var operandA = vars["A"];
            var operandB = vars["B"];
            if (operandA is int && operandB is int)
                outputVars["Result"] = (int)operandA - (int)operandB;
            else
                outputVars["Result"] = Convert.ToDouble(operandA) - Convert.ToDouble(operandB);
        }
    }
}
