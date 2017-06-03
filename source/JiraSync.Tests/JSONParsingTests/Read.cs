using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JiraSync.Tests.JSONParsingTests
{
    /// <summary>
    /// Reading JSON Tests
    /// </summary>
    [TestClass]
    public class Read
    {
        public Read()
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
        public void ValidateReadJSON()
        {
            var jira = new JiraService.Jira("", "", "");
            var issueSrevice = jira.GetIssueService();
            var issue = issueSrevice.GetAsync("").Result;
            string name = issue.GetValueByPath("$.fields.issuetype.name");
            Assert.IsTrue(name == "Story");
            //
            // TODO: Add test logic here
            //
        }
    }
}
