﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tricentis.TCAddOns;
using Tricentis.TCAPIObjects;
using Tricentis.TCAPIObjects.Objects;

namespace JiraSync
{
    public class ToscaHelpers
    {
        public static TCObjectPropertiesDefinition CreateCustomProperties(String objectToCreateOn, String propertyName)
        {
            TCProject proj = TCAddOn.ActiveWorkspace.GetTCProject();
            TCObjectPropertiesDefinition propDef = proj.ObjectPropertiesDefinitions.FirstOrDefault(pd => pd.Name.ToLower() == objectToCreateOn.ToLower());

            // Check if object exist
            if (propDef != null)
            {
                String[] props = propDef.GetPropertyNames();

                // Check if property already exists, create it otherwise
                if (!props.Contains(propertyName))
                {
                    TCObjectProperty newPrp = propDef.CreateProperty();
                    newPrp.Name = propertyName;
                }
            }
            else
                throw new Exception("Invalid object to create property definition");

            return propDef;
        }

        public static TCObjectPropertiesDefinition CreateIssuesProperties(String objectToCreateOn, String propertyName)
        {
            TCProject proj = TCAddOn.ActiveWorkspace.GetTCProject();
            TCObjectPropertiesDefinition issuesDef = proj.ObjectPropertiesDefinitions.FirstOrDefault(pd => pd.Name.ToLower() == objectToCreateOn.ToLower());

            if (issuesDef == null)
            {   // Create Issues property definition.
                TCComponentFolder tempFolder = (TCComponentFolder)proj.CreateComponentFolder();
                issuesDef = tempFolder.CreatePropertyDefinition("Issue");
                proj.Move(issuesDef);
                tempFolder.Delete(MsgBoxResult_OkCancel.Ok, MsgBoxResult_YesNo.Yes);
            }

            // Check if object exist
            if (issuesDef != null)
            {
                String[] props = issuesDef.GetPropertyNames();

                // Check if property already exists, create it otherwise
                if (!props.Contains(propertyName))
                {
                    TCObjectProperty newPrp = issuesDef.CreateProperty();
                    newPrp.Name = propertyName;
                }
            }

            return issuesDef;
        }

        public static Requirement FindRequirementByJiraProperty(TCObject obj, string jiraTicketNumber)
        {
            TCObject o = obj.Search("=>SUBPARTS:Requirement[" + Global.JiraTicketAttributeName + "==\"" + jiraTicketNumber + "\"]").FirstOrDefault();

            if (o == null)
                return null;
            else
                return (Requirement)o;
        }

        public class RequirementHelpers
        {
            public static Requirement CreateRequirement(RequirementSet parent, String title, Dictionary<String, String> properties)
            {
                Requirement req = parent.CreateRequirement();
                req.Name = title;

                foreach (KeyValuePair<String, String> prop in properties)
                {
                    req.SetAttibuteValue(prop.Key, prop.Value);
                }

                return req;
            }

            public static Requirement CreateRequirement(Requirement parent, String title, Dictionary<String, String> properties)
            {
                Requirement req = parent.CreateRequirement();
                req.Name = title;

                foreach (KeyValuePair<String, String> prop in properties)
                {
                    req.SetAttibuteValue(prop.Key, prop.Value);
                }

                return req;
            }

            public static Requirement UpdateRequirement(Requirement req, String title, Dictionary<String, String> properties)
            {
                req.Name = title;

                foreach (KeyValuePair<String, String> prop in properties)
                {
                    req.SetAttibuteValue(prop.Key, prop.Value);
                }

                return req;
            }
        }
    }
}