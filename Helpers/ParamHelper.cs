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
            var missingParams = (commandParams == null) ? expectedParams : 
                expectedParams.Where(param => !commandParams.ContainsKey(param)
                    || string.IsNullOrWhiteSpace(commandParams[param].ToString()));
            
            if (missingParams.Any())
            {
                var missingParamString = string.Join(", ", missingParams);
                throw new ArgumentException($"Missing the following required parameters: {missingParamString}");
            }
        }
    }
}