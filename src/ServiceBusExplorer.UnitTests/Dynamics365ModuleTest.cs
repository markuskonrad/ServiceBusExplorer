using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Azure.ServiceBusExplorer.Helpers;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace ServiceBusExplorer.UnitTests
{
    [TestClass]
    public class Dynamics365ModuleTest
    {
        /// <summary>
        /// Tests if the expected values are returned by GetPrintableAttributes
        /// 
        /// Conditions:
        /// Entity contains all relevant attribute types.
        /// 
        /// Expected result:
        /// printableAttributes contains the correct string values for given input Entity.
        /// </summary>
        [TestMethod]
        public void Dynamics365Module_GetPrintableValueFromValue_Default()
        {
            // Arrange
            Entity record = new Entity();
            DateTime currentDateTimeUtc = DateTime.UtcNow;
            EntityReference contactLookup = new EntityReference("contact", Guid.NewGuid());
            record["accountname"] = "hello";
            record["territorycode"] = new OptionSetValue(1);
            record["ownerid"] = Guid.NewGuid();
            record["contactid"] = contactLookup;
            record["donotfax"] = true;
            record["opendeals"] = 12345;
            record["modifiedon"] = currentDateTimeUtc;
            record["creditlimit"] = new Money(100);

            // Act
            Dictionary<string, string> printableAttributes = Dynamics365Module.GetPrintableAttributes(record);

            // Assert
            Assert.IsNotNull(printableAttributes);
            Assert.AreEqual(8, printableAttributes.Count);
            Assert.AreEqual("hello", printableAttributes["accountname"]);
            Assert.AreEqual("1", printableAttributes["territorycode"]);
            Assert.AreEqual(((Guid)record["ownerid"]).ToString(), printableAttributes["ownerid"]);
            Assert.AreEqual(string.Format("{{ \"LogicalName\": \"{0}\", \"Id\": \"{1}\" }}", contactLookup.LogicalName, contactLookup.Id), printableAttributes["contactid"]);
            Assert.AreEqual("True", printableAttributes["donotfax"]);
            Assert.AreEqual("12345", printableAttributes["opendeals"]);
            Assert.AreEqual(currentDateTimeUtc.ToString(Constants.DateTimeUtcFormatString), printableAttributes["modifiedon"]);
            Assert.AreEqual("100", printableAttributes["creditlimit"]);
        }
    }
}
