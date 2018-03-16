using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using VstsExtensions.Core.Helpers;

namespace VstsExtensions.Core
{
    public class DuplicateRuleService : IDuplicateRuleService
    {
        private IOrganizationService Service { get; set; }
        private IFileManager FileManager { get; }

        public DuplicateRuleService(IOrganizationService service, IFileManager fileManager = null)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            FileManager = fileManager ?? new FileManager();
        }

        public void StorePublishedRules(List<Guid> rules, string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }

            Console.WriteLine($"Storing rules in working directory at {directory}");

            var json = JsonConvert.SerializeObject(rules, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            FileManager.StoreJsonConfig(json, "PublishedDuplicateDetectionRules", directory);
        }

        public List<Guid> GetRulesFromConfig(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentNullException(nameof(directory));
            }

            var json = FileManager.GetJsonConfig("PublishedDuplicateDetectionRules", directory);
            var rules = JsonConvert.DeserializeObject<List<Guid>>(json);

            Console.Write($"Retrieved {rules?.Count} rules from config file.");

            return rules;
        }

        public void PublishRules(List<Guid> rules)
        {
            if (rules != null)
            {
                foreach (var i in rules)
                {
                    try
                    {
                        Service.Execute(new PublishDuplicateRuleRequest { DuplicateRuleId = i });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to publish duplicate detection rule with ID: {i.ToString()}. Error: {ex}");
                    }
                }
            }
        }

        public List<Guid> RetrievePublishedRules()
        {
            Console.WriteLine("Retrieving published duplicate detection rules from CRM.");

            var query = new QueryExpression { EntityName = "duplicaterule", ColumnSet = new ColumnSet("duplicateruleid") };
            query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)Status.Published);

            var rules = Service?.RetrieveMultiple(query);

            Console.WriteLine($"Retrieved {rules?.Entities?.Count ?? 0} published duplicate detection rules from CRM.");

            return rules?.Entities?.Select(i => i.Id)?.ToList() ?? new List<Guid>();
        }

        public enum Status
        {
            Published = 2
        }
    }
}