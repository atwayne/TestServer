using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Dubstep.TestUtilities
{
    public class Rule
    {
        public IList<Func<HttpRequest, bool>> Predictions { get; set; }
        public Action<HttpResponse> Action { get; set; }
        public RuleSet RuleSet { get; set; }
        public int? MaxMatchCount { get; private set; }
        private int MatchCount { get; set; }

        public Rule(RuleSet ruleSet)
        {
            RuleSet = ruleSet;
            Predictions = new List<Func<HttpRequest, bool>>();
        }

        public bool IsActive()
        {
            if (Action == null)
            {
                return false;
            }

            return !MaxMatchCount.HasValue || MatchCount < MaxMatchCount;
        }

        /// <summary>
        /// Complete current Rule and add a new rule on parent RuleSet
        /// </summary>
        public Rule AddRule()
        {
            return RuleSet.AddRule();
        }

        /// <summary>
        /// Add a rule that the HttpRequest.Method is GET
        /// </summary>
        public Rule WhenGet()
        {
            Predictions.Add((request) =>
            {
                return request.Method == "GET";
            });
            return this;
        }

        /// <summary>
        /// Add a rule that the HttpRequest.Method is POST
        /// </summary>
        public Rule WhenPost()
        {
            Predictions.Add((request) =>
            {
                return request.Method == "POST";
            });
            return this;
        }

        /// <summary>
        /// Add a rule that the request url matches a regex expression
        /// </summary>
        /// <param name="pattern">a regex expression</param>
        public Rule WhenUrlMatch(string pattern)
        {
            Predictions.Add((request) =>
            {
                var actualUrl = UriHelper.GetDisplayUrl(request);
                return Regex.IsMatch(actualUrl, pattern);
            });
            return this;
        }

        /// <summary>
        /// Add a rule that a http request header has an expected value
        /// </summary>
        /// <param name="key">The name of the header</param>
        /// <param name="value">The expected value</param>
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

        /// <summary>
        /// Add a rule that a http request has an expected Authorization header
        /// </summary>
        /// <param name="token">The expected Authorization header</param>
        public Rule WhenAuthorizationMatch(string token)
        {
            return WhenHeaderMatch("Authorization", token);
        }

        /// <summary>
        /// Set the action of the current rule to BadRequest
        /// </summary>
        /// <param name="message">Default: Bad Request</param>
        public Rule SetBadRequest(string message = "Bad Request")
        {
            Action = async response =>
            {
                response.StatusCode = 400;
                await response.WriteAsync(message);
            };
            return this;
        }

        /// <summary>
        /// Set the action of the current rule to write a response
        /// </summary>
        /// <param name="output">The plain text of the response</param>
        public Rule SetOkResponse(string output)
        {
            Action = async response =>
            {
                await response.WriteAsync(output);
            };
            return this;
        }

        /// <summary>
        /// Set how much request could apply to this rule
        /// </summary>
        public Rule SetMaxMatchCount(int? maxMatchCount)
        {
            MaxMatchCount = maxMatchCount;
            return this;
        }

        /// <summary>
        /// Increase the count this rule applies
        /// </summary>
        internal void SetMatchCount()
        {
            MatchCount++;
        }
    }
}
