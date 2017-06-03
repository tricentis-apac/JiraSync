using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace JiraSync.Tests.JSONParsingTests
{
    /// <summary>
    /// Reading JSON Tests
    /// </summary>
    [TestClass]
    public class Write
    {
        public Write()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void WritePropertyOfExistingChild()
        {
            var issue = new JiraService.Issue.Issue
            {
                fields = new JiraService.Issue.IssueFields
                {
                    description = "derp",
                    status = new JiraService.Issue.Field.StatusField
                    {
                        name = "To Do"
                    }
                }
            };
            issue.SetValueByPath("$.fields.issuetype.name","Story");
            string result = issue.SerializeObject(new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
            var resultantIssue = JsonConvert.DeserializeObject<JiraService.Issue.Issue>(result, new JiraService.Issue.IssueConverter());
            Assert.IsTrue(resultantIssue.fields.issuetype.name == "Story");
            //
            // TODO: Add test logic here
            //
        }
        [TestMethod]
        public void WriteNewProperty()
        {
            var issue = new JiraService.Issue.Issue
            {
                key = "TEST"
            };
            issue.SetValueByPath("$.fields.issuetype.name", "Story");
            string result = issue.SerializeObject(new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
            var resultantIssue = JsonConvert.DeserializeObject<JiraService.Issue.Issue>(result, new JiraService.Issue.IssueConverter());
            Assert.IsTrue(resultantIssue.fields.issuetype.name == "Story");
            //
            // TODO: Add test logic here
            //
        }
        [TestMethod]
        public void WritePropertyOnEmptyObject()
        {
            var issue = new JiraService.Issue.Issue
            {
            };
            issue.SetValueByPath("$.fields.issuetype.name", "Story");
            string result = issue.SerializeObject(new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
            var resultantIssue = JsonConvert.DeserializeObject<JiraService.Issue.Issue>(result, new JiraService.Issue.IssueConverter());
            Assert.IsTrue(resultantIssue.fields.issuetype.name == "Story");
            //
            // TODO: Add test logic here
            //
        }

        [TestMethod]
        public void OverwriteExistingProperty()
        {
            var issue = new JiraService.Issue.Issue
            {
                fields = new JiraService.Issue.IssueFields
                {
                    description = "derp",
                    status = new JiraService.Issue.Field.StatusField
                    {
                        name = "To Do"
                    }
                }
            };
            issue.SetValueByPath("$.fields.status.name","In Progress");
            string result = issue.SerializeObject(new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
            var resultantIssue = JsonConvert.DeserializeObject<JiraService.Issue.Issue>(result, new JiraService.Issue.IssueConverter());
            Assert.IsTrue(resultantIssue.fields.status.name == "In Progress");
            //
            // TODO: Add test logic here
            //
        }
        [TestMethod]
        public void WriteNewRootProperty()
        {
            var issue = new JiraService.Issue.Issue
            {
                fields = new JiraService.Issue.IssueFields
                {
                    description = "derp",
                    status = new JiraService.Issue.Field.StatusField
                    {
                        name = "To Do"
                    }
                }
            };
            issue.SetValueByPath("$.key", "TD-6");
            string result = issue.SerializeObject(new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
            var resultantIssue = JsonConvert.DeserializeObject<JiraService.Issue.Issue>(result, new JiraService.Issue.IssueConverter());
            Assert.IsTrue(resultantIssue.key == "TD-6");
            //
            // TODO: Add test logic here
            //
        }
        [TestMethod]
        public void OverwriteExistingRootProperty()
        {
            var issue = new JiraService.Issue.Issue
            {
                key = "TD-7",
                fields = new JiraService.Issue.IssueFields
                {
                    description = "derp",
                    status = new JiraService.Issue.Field.StatusField
                    {
                        name = "To Do"
                    }
                }
            };
            issue.SetValueByPath("$.key", "TD-6");
            string result = issue.SerializeObject(new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });
            var resultantIssue = JsonConvert.DeserializeObject<JiraService.Issue.Issue>(result, new JiraService.Issue.IssueConverter());
            Assert.IsTrue(resultantIssue.key == "TD-6");
            //
            // TODO: Add test logic here
            //
        }



    }
}
