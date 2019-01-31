using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Titanoboa
{
    public static class ParamHelper
    {
        /*
         * Checks the the given parameters exist in the JSON parmeter object. Throws an
         * ArgumentException if they don't.
         */
        public static void ValidateParamsExist(JObject commandParams, params string[] expectedParams)
        {
            var missingParams = expectedParams.Where(param => !commandParams.ContainsKey(param));
            
            if (missingParams.Any())
            {
                var missingParamString = string.Join(", ", missingParams);
                throw new ArgumentException($"Missing the following required parameters: {missingParamString}");
            }
        }
    }
}