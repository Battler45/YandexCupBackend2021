using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace YandexCupBackend
{
    class MewServerClient
    {
        private readonly HttpClient _client = new HttpClient();
        private const string ServerAddress = "http://127.0.0.1:7777/";
        private const string RequestHeader = "X-Cat-Variable";
        private const string ResponseHeader = "X-Cat-Value";
        private readonly HttpMethod _httpMethodType = new HttpMethod("MEW");

        public async Task<List<string>> SendMewRequest(IEnumerable<string> names)
        {
            using var requestMessage = new HttpRequestMessage(_httpMethodType, ServerAddress);
            requestMessage.Headers.Add(RequestHeader, names);
            var response = await _client.SendAsync(requestMessage);
            var values = response.Headers.GetValues(ResponseHeader).ToList();

            return values;
        }
    }
    class TaskA
    {
        private static List<string> ReadVariables()
        {
            return new List<string> { Console.ReadLine(), Console.ReadLine(), Console.ReadLine(), Console.ReadLine() };
        }

        static List<string> GetVariablesValues(List<string> secondThirdValues, List<string> secondFourthValues, List<string> firstThirdFourth)
        {
            string first, second, third, fourth;
            var isPossibleGetSecondValue = secondThirdValues.Intersect(secondFourthValues).Count() == 1;
            if (!isPossibleGetSecondValue)
            {
                //v3 = v4
                third = firstThirdFourth
                    .GroupBy(v => v)
                    .First(g => g.Count() > 1).Key;
                fourth = third;
                first = firstThirdFourth.Except(new[] { third }).FirstOrDefault() ?? third;
                second = secondThirdValues.Except(new[] { third }).FirstOrDefault() ?? third;
            }
            else
            {
                second = secondThirdValues.Intersect(secondFourthValues).First();
                third = secondThirdValues.Except(new[] { second }).FirstOrDefault() ?? second;
                fourth = secondFourthValues.Except(new[] { second }).FirstOrDefault() ?? second;
                var groupedFirstThirdFourth = firstThirdFourth.GroupBy(v => v)
                    .ToDictionary(valueGroup => valueGroup.Key, g => g.Count());
                groupedFirstThirdFourth[third] -= 1;
                groupedFirstThirdFourth[fourth] -= 1;
                first = groupedFirstThirdFourth.First(g => g.Value > 0).Key;
            }
            return new List<string> { first, second, third, fourth };
        }

        public static async Task Solve()
        {
            var variables = ReadVariables();
            var mewClient = new MewServerClient();
            var secondThirdValues = await mewClient.SendMewRequest(new[] { variables[1], variables[2] });
            var secondFourthValues = await mewClient.SendMewRequest(new[] { variables[1], variables[3] });
            var firstThirdFourth = await mewClient.SendMewRequest(new []{ variables[0], variables[2], variables[3] });
            var answer = GetVariablesValues(secondThirdValues, secondFourthValues, firstThirdFourth);
            Console.WriteLine(string.Join(Environment.NewLine, answer));
        }
    }
}
