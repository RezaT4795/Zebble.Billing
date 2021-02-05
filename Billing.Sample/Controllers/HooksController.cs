﻿namespace Zebble.Billing.Sample
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [ApiController]
    [Route("hooks")]
    public class HooksController : ControllerBase
    {
        readonly IRootHookInterceptor _rootHookInterceptor;

        public HooksController(IRootHookInterceptor rootHookInterceptor)
        {
            _rootHookInterceptor = rootHookInterceptor;
        }

        [HttpPost("intercept/{platform}")]
        public async Task<string> Intercept([FromRoute] SubscriptionPlatform platform)
        {
            await _rootHookInterceptor.Intercept(platform);

            return $"Trigerred hook intercepted. ({platform})";
        }
    }
}
