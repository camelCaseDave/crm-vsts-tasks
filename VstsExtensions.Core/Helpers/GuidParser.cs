using Microsoft.Xrm.Sdk;
using System;

namespace VstsExtensions.Core.Helpers
{
    public static class GuidParser
    {
        public static Guid TryParseIdOrThrow(string id)
        {
            Console.WriteLine($"Trying to parse id: {id}");

            var isValid = Guid.TryParse(id, out var parsedId);

            if (isValid)
            {
                return parsedId;
            }
            else
            {
                throw new InvalidPluginExecutionException($"Id: {id} is not valid.");
            }
        }

        public static Guid? TryParseIdOrLog(string id, string errorMessage)
        {
            Console.WriteLine($"Trying to parse id: {id}");

            var isValid = Guid.TryParse(id, out var parsedId);

            if (isValid)
            {
                return parsedId;
            }
            else
            {
                Console.WriteLine($"Id: {id} is not valid.");
                return null;
            }
        }
    }
}