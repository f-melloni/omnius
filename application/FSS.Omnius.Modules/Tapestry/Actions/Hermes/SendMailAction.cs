﻿using System.Collections.Generic;
using System.Linq;
using FSS.Omnius.Modules.CORE;
using FSS.Omnius.Modules.Entitron.Entity;
using FSS.Omnius.Modules.Hermes;

namespace FSS.Omnius.Modules.Tapestry.Actions.Mozaic
{
    [MozaicRepository]
    class SendMailAction : Action
    {
        public override int Id => 2005;

        public override int? ReverseActionId => null;

        public override string[] InputVar => new string[] { "?Recipients", "Subject", "Template", "?CC", "?BCC", "?Data" };

        public override string[] OutputVar => new string[] { "Result", "ErrorMessage" };

        public override string Name => "Send mail";

        public override void InnerRun(Dictionary<string, object> vars, Dictionary<string, object> outputVars, Dictionary<string, object> InvertedInputVars, Message message)
        {
            var context = COREobject.i.Context;
            var dataDictionary = new Dictionary<string, object>();
            if (vars.ContainsKey("Data"))
                dataDictionary = (Dictionary<string, object>)vars["Data"];
            string templateName = (string)vars["Template"];

            Mailer czechMailer = new Mailer("", templateName, dataDictionary, Locale.cs);
            Mailer englishMailer = new Mailer("", templateName, dataDictionary, Locale.en);

            var recipientsCzechOutput = new Dictionary<string, string>();
            var recipientsEnglishOutput = new Dictionary<string, string>();
            var bccCzechOutput = new Dictionary<string, string>();
            var bccEnglishOutput = new Dictionary<string, string>();
            
            if (vars.ContainsKey("Recipients"))
            {
                var recipientsInputDict = (Dictionary<string, string>)vars["Recipients"];
                
                foreach (var addressPair in recipientsInputDict)
                {
                    var user = context.Users.Where(u => u.Email == addressPair.Key).FirstOrDefault();
                    if (user != null && user.Locale == Locale.en)
                        recipientsEnglishOutput.Add(addressPair.Key, addressPair.Value);
                    else
                        recipientsCzechOutput.Add(addressPair.Key, addressPair.Value);
                }

                czechMailer.To(recipientsCzechOutput);
                englishMailer.To(recipientsEnglishOutput);
            }
            if (vars.ContainsKey("BCC")) {
                var bccInputDict = (Dictionary<string, string>)vars["BCC"];
                
                foreach (var addressPair in bccInputDict)
                {
                    var user = context.Users.Where(u => u.Email == addressPair.Key).FirstOrDefault();
                    if (user != null && user.Locale == Locale.en)
                        bccEnglishOutput.Add(addressPair.Key, addressPair.Value);
                    else
                        bccCzechOutput.Add(addressPair.Key, addressPair.Value);
                }

                czechMailer.BCC(bccCzechOutput);
                englishMailer.BCC(bccEnglishOutput);
            }

            if(recipientsCzechOutput.Count > 0 || bccCzechOutput.Count > 0)
                czechMailer.SendBySender();
            if(recipientsEnglishOutput.Count > 0 || bccEnglishOutput.Count > 0)
                englishMailer.SendBySender();
        }
    }
}
