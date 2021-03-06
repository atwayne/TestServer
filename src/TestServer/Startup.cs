using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dubstep.TestUtilities
{
    internal class Startup
    {
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public virtual void Configure(IApplicationBuilder app, RuleSet ruleSet)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/", async context =>
                {
                    var matchFound = false;
                    foreach (var rule in ruleSet.Rules)
                    {
                        if (!rule.IsActive())
                        {
                            continue;
                        }

                        var match = rule.Predictions.All(prediction => prediction(context.Request));
                        if (match)
                        {
                            rule.SetMatchCount();
                            await Task.Run(() =>
                            {
                                rule.Action(context.Response);
                            });
                            matchFound = true;
                            break;
                        }
                    }
                    if (!matchFound)
                    {
                        await Task.Run(() =>
                        {
                            ruleSet.DefaultAction(context.Response);
                        });
                    }
                });
            });
        }
    }
}
