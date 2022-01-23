using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace TestUtilities
{
    public class Rule
    {
        public IList<Func<HttpRequest, bool>> Predictions { get; set; }
        public Action<HttpResponse> Action { get; set; }
        public RuleSet RuleSet { get; set; }

        public Rule(RuleSet ruleSet)
        {
            RuleSet = ruleSet;
            Predictions = new List<Func<HttpRequest, bool>>();
        }

        public bool IsValid()
        {
            return Action != null;
        }

        public Rule AddRule()
        {
            if (Action == null)
            {
                throw new InvalidOperationException("Previous rule not completed");
            }
            return RuleSet.AddRule();
        }

        public Rule WhenGet()
        {
            Predictions.Add((request) =>
            {
                return request.Method == "GET";
            });
            return this;
        }

        public Rule WhenUrlMatch(string pattern)
        {
            Predictions.Add((request) =>
            {
                var actualUrl = UriHelper.GetDisplayUrl(request);
                return Regex.IsMatch(actualUrl, pattern);
            });
            return this;
        }

        public Rule WhenHeaderMatch(string key, string value)
        {
            Predictions.Add((request) =>
            {
                if (!request.Headers.TryGetValue(key, out var actual))
                {
                    return false;
                }
                return actual.Contains(value);
            });
            return this;
        }

        public Rule WhenAuthorizationMatch(string token)
        {
            return WhenHeaderMatch("Authorization", token);
        }

        public Rule SetBadRequest(string message = "Bad Request")
        {
            Action = async response =>
            {
                response.StatusCode = 400;
                await response.WriteAsync(message);
            };
            return this;
        }

        public Rule SetOkResponse(string output)
        {
            Action = async response =>
            {
                await response.WriteAsync(output);
            };
            return this;
        }
    }
}
