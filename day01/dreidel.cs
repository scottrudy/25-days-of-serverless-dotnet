using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;

namespace day01 {
    public static class dreidel     {
        private static readonly Random _random = new Random();
        private static object _lock  = new object();
        private static readonly  string[] sides = new string[] { "נ (nun)", "ג (gimel)", "ה (hei)", "ש (shin)" };

        [FunctionName("dreidel")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "spin")] HttpRequest req)  {
            return (ActionResult)new OkObjectResult($"{sides[GetRandomValue(sides.Length)]}");
        }

        private static int GetRandomValue(int max) {
            lock (_lock) {
                return _random.Next(max);
            }
        }
    }
}