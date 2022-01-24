using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Dubstep.TestUtilities
{
    public class RuleSet
    {
        public List<Rule> Rules { get; private set; }
        /// <summary>
        /// The fallback action when no rule matches
        /// By default it's a BadRequest response
        /// </summary>
        public Action<HttpResponse> DefaultAction { get; private set; }

        public RuleSet()
        {
            Rules = new List<Rule>();
            DefaultAction = async response =>
            {
                response.StatusCode = 404;
                await response.WriteAsync("Simon says Not Match");
            };
        }

        /// <summary>
        /// Create a new Rule and add it to the rule list
        /// </summary>
        public Rule AddRule()
        {
            var rule = new Rule(this);
            Rules.Add(rule);
            return rule;
        }

        /// <summary>
        /// Set fallback action if no rule matches
        /// </summary>
        public RuleSet SetDefaultAction(Action<HttpResponse> action)
        {
            DefaultAction = action;
            return this;
        }
    }
}
