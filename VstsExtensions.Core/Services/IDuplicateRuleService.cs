using System;
using System.Collections.Generic;

namespace VstsExtensions.Core
{
    public interface IDuplicateRuleService
    {
        void StorePublishedRules(List<Guid> rules, string directory);

        List<Guid> GetRulesFromConfig(string directory);

        void PublishRules(List<Guid> rules);

        List<Guid> RetrievePublishedRules();
    }
}