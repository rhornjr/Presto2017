using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PrestoCommon.Entities;
using PrestoCommon.Exceptions;

namespace PrestoCommon.Misc
{
    public static class VariableGroupResolver
    {
        public static IEnumerable<CustomVariable> Resolve(ApplicationWithOverrideVariableGroup appWithGroup, ApplicationServer server)
        {
            if (appWithGroup == null) { throw new ArgumentNullException("appWithGroup"); }
            if (server == null) { throw new ArgumentNullException("server"); }

            int numberOfProblemsFound = 0;
            bool variableFoundMoreThanOnce = false;
            var resolvedCustomVariables = new List<CustomVariable>();

            // This is normally set when calling Install(), but since we're not doing that
            // here, set it explicitly.
            ApplicationWithOverrideVariableGroup.SetInstallationStartTimestamp(DateTime.Now);

            foreach (TaskBase task in appWithGroup.Application.MainAndPrerequisiteTasks)
            {
                if (variableFoundMoreThanOnce) { break; }

                foreach (string taskProperty in task.GetTaskProperties())
                {
                    if (variableFoundMoreThanOnce) { break; }

                    string key = string.Empty;
                    string value = string.Empty;

                    foreach (Match match in CustomVariableGroup.GetCustomVariableStringsWithinBiggerString(taskProperty))
                    {
                        // Don't add the same key more than once.
                        if (resolvedCustomVariables.Any(x => x.Key == match.Value)) { continue; }

                        try
                        {
                            key = match.Value;
                            // Note, we pass true so that encrypted values stay encrypted. We don't want the user to see encrypted values.
                            value = CustomVariableGroup.ResolveCustomVariable(match.Value, server, appWithGroup, true);
                        }
                        catch (CustomVariableMissingException)
                        {
                            numberOfProblemsFound++;
                            value = "** NOT FOUND **";
                        }
                        catch (CustomVariableExistsMoreThanOnceException)
                        {
                            variableFoundMoreThanOnce = true;
                            numberOfProblemsFound++;
                            value = "** MORE THAN ONE FOUND **";
                            resolvedCustomVariables.Clear();
                            break;
                        }

                        resolvedCustomVariables.Add(new CustomVariable() { Key = key, Value = value });
                    }
                }
            }

            return resolvedCustomVariables;
        }
    }
}
