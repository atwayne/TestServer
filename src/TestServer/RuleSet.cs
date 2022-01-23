using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace TestUtilities
{
    public class RuleSet
    {
        public List<Rule> Rules { get; private set; }
        public Action<HttpResponse> DefaultAction { get; private set; }

        public RuleSet()
        {
            Rules = new List<Rule>();
            DefaultAction = async response =>
            {
                response.StatusCode = 404;
                await response.WriteAsync("Simons says Not Match");
            };
        }

        public Rule AddRule()
        {
            var rule = new Rule(this);
            Rules.Add(rule);
            return rule;
        }

        public RuleSet SetDefaultAction(Action<HttpResponse> action)
        {
            DefaultAction = action;
            return this;
        }
    }
}
