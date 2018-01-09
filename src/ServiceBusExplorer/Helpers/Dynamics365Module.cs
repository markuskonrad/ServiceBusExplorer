using Microsoft.ServiceBus.Messaging;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.ServiceBusExplorer.Helpers
{
    /// <summary>
    /// Dynamics365 Helper Module
    /// </summary>
    public static class Dynamics365Module
    {
        /// <summary>
        /// Returns message text if the message was sent by dynamics in the correct format.
        /// </summary>
        /// <param name="messageToRead">The borkedered message to be handled.</param>
        /// <returns>the Message text</returns>
        public static string GetMessageTextForDynamics365(BrokeredMessage messageToRead)
        {
            StringBuilder messageTextStringBuilder = new StringBuilder();
            RemoteExecutionContext executionContext = null;
            try
            {
                executionContext = messageToRead.GetBody<RemoteExecutionContext>();
            }
            catch (Exception)
            {
                messageTextStringBuilder.AppendLine("Deserialize body to RemoteExecutionContext (Dynamics365) does not work - Not a Dynamics365 message or unsupported format");
            }

            messageTextStringBuilder.AppendLine("--- Core context values ---");
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "MessageName", executionContext.MessageName));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "PrimaryEntityName", executionContext.PrimaryEntityName));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "PrimaryEntityId", executionContext.PrimaryEntityId));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "Stage", executionContext.Stage));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "Depth", executionContext.Depth));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "RequestId", executionContext.RequestId));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "UserId", executionContext.UserId));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "InitiatingUserId", executionContext.InitiatingUserId));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "BusinessUnitId", executionContext.BusinessUnitId));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "CorrelationId", executionContext.CorrelationId));
            messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, "IsInTransaction", executionContext.IsInTransaction));
            messageTextStringBuilder.AppendLine();

            messageTextStringBuilder.AppendLine("--- Attributes of Primary Entity ---");
            Entity primaryEntity = null;
            if (executionContext.InputParameters != null &&
                executionContext.InputParameters.ContainsKey("Target") &&
                executionContext.InputParameters["Target"] is Entity)
            {
                primaryEntity = (Entity)executionContext.InputParameters["Target"];
                Dictionary<string, string> printableDynamics365Attributes = Dynamics365Module.GetPrintableAttributes(primaryEntity);
                foreach (var attribute in printableDynamics365Attributes)
                {
                    messageTextStringBuilder.AppendLine(string.Format(Constants.ConcatStringFormatTwoValues, attribute.Key, attribute.Value));
                }
            }
            return messageTextStringBuilder.ToString();
        }

        /// <summary>
        /// Converts the given Dynamics365 attributes of the Entity object to a 
        /// Dictionary with the printable versio of the Dynamics365 values.
        /// Key value is the same as in original AttributeCollection.
        /// </summary>
        /// <param name="record">The relevant Dynamics365 Entity</param>
        /// <returns>The printable attribute values dictionary</returns>
        public static Dictionary<string, string> GetPrintableAttributes(Entity record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("Dynamics365 Entity object was null");
            }

            Dictionary<string, string> printableAttributes = new Dictionary<string, string>();

            foreach (var attributeKeyValuePair in record.Attributes)
            {
                printableAttributes.Add(attributeKeyValuePair.Key, GetPrintableValueFromValue(attributeKeyValuePair.Value));
            }

            return printableAttributes;
        }

        /// <summary>
        /// Returns printable version of the provided attribute value.
        /// E.g. int value 1 will be "1", Entity Reference will be "[logicalname | id]", 
        /// Optionset 900000000 will be "900000000", DateTime will be UTC format ""
        /// </summary>
        /// <param name="attributeValue"></param>
        /// <returns>The printable (string) of the attribute value</returns>
        private static string GetPrintableValueFromValue(object attributeValue)
        {
            if (attributeValue == null)
            {
                return string.Empty;
            }
            else if (attributeValue.GetType() == typeof(OptionSetValue))
            {
                return (attributeValue as OptionSetValue).Value.ToString();
            }
            else if (attributeValue.GetType() == typeof(EntityReference))
            {
                EntityReference entityReference = (attributeValue as EntityReference);
                return string.Format("{{ \"LogicalName\": \"{0}\", \"Id\": \"{1}\" }}", entityReference.LogicalName, entityReference.Id);
            }
            else if (attributeValue.GetType() == typeof(DateTime))
            {
                return ((DateTime)attributeValue).ToString(Constants.DateTimeUtcFormatString);
            }
            else if (attributeValue.GetType() == typeof(Money))
            {
                return (attributeValue as Money).Value.ToString();
            }
            else
            {
                return attributeValue.ToString();
            }
        }
    }
}
