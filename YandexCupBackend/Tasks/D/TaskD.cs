using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YandexCupBackend
{
    enum Verdict
    {
        OK, CE, WA, TL, RE
    }
    class Submission
    {
        private Submission(int timestamp, char problem, Verdict verdict)
        {
            Timestamp = timestamp;
            Problem = problem;
            Verdict = verdict;
        }

        public static Submission FromXml(XElement node)
        {
            var timestamp = int.Parse(node.Attribute("timestamp").Value);
            var problem = node.Attribute("problem").Value.First(c => char.IsLetter(c) && char.IsUpper(c) && c <= 'Z' && c >= 'A');
            if (!Enum.TryParse(node.Attribute("verdict").Value, true, out Verdict verdict))
            {
                throw new Exception("cannot parse submission from xml");
            }
            return new Submission(timestamp, problem, verdict);
        }

        public int Timestamp { get; }
        public char Problem { get; }
        public Verdict Verdict { get; }
    }
    class Participant
    {
        public Participant(string login, List<Submission> logs)
        {
            Login = login ?? throw new ArgumentNullException(nameof(login));
            Logs = logs ?? throw new ArgumentNullException(nameof(logs));
        }

        public string Login { get; }
        private List<Submission> Logs { get; }

        public class Result
        {
            public Result(int fine, int completedTasksCount, Participant participant)
            {
                Fine = fine;
                CompletedTasksCount = completedTasksCount;
                this.Participant = participant;
            }
            public Participant Participant { get; }

            public int Fine { get; }
            public int CompletedTasksCount { get; }
        }

        private static int ComputeFine(IGrouping<char, Submission> group)
        {
            var firstOkTimestamp = group.Where(s => s.Verdict == Verdict.OK).Min(s => s.Timestamp);
            var errorsCount = group.Count(s =>  s.Verdict != Verdict.CE && s.Timestamp <= firstOkTimestamp && s.Verdict != Verdict.OK); //
            return firstOkTimestamp + errorsCount * 20;
        }

        private int ComputeFine()
        {
            var completedProblems = this.Logs.Where(log => log.Verdict == Verdict.OK)
                .Select(log => log.Problem)
                .ToHashSet();
            var fine = Logs.Where(task => completedProblems.Contains(task.Problem))
                .GroupBy(task => task.Problem)
                .Select(ComputeFine)
                .Sum();
            return fine;
        }

        public Result ComputeResult()
        {
            var completedTasksCount = Logs.Count(log => log.Verdict == Verdict.OK);
            var fine = ComputeFine();
            return new Result(fine, completedTasksCount, this);
        }
    }
    class ServerClient
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string _serverAddress;
        private readonly string _contest;
        private readonly Encoding _russianEncoding;
        public ServerClient(string contest, string serverAddress = "http://127.0.0.1:7777")
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _russianEncoding = Encoding.GetEncoding("windows-1251");
            this._contest = EncodeWin1251String(contest);
            this._serverAddress = serverAddress;
        }

        private string DecodeWin1251String(string str) => Encoding.UTF8.GetString(_russianEncoding.GetBytes(str));
        private string EncodeWin1251String(string str) => _russianEncoding.GetString(Encoding.UTF8.GetBytes(str));

        public async Task<List<string>> GetLogins()
        {
            const string localPath = "view/participants";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{this._serverAddress}/{localPath}?contest={this._contest}");
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            var document = XDocument.Parse(content);
            var participants = document.Root.Descendants("participant")
                .Select(node => node.FirstAttribute.Value.ToString())
                .Select(DecodeWin1251String)
                .ToList();

            return participants;
        }
        private async Task<List<Submission>> GetSubmissions(string login)
        {
            login = EncodeWin1251String(login);
            const string localPath = "view/submissions";
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{this._serverAddress}/{localPath}?contest={this._contest}&login={login}");
            var response = await _client.SendAsync(requestMessage);
            var content = await response.Content.ReadAsStringAsync();
            var document = XDocument.Parse(content);
            var submissions = document.Root.Descendants("submission")
                .Select(Submission.FromXml)
                .ToList();

            return submissions;
        }
        public async Task<Participant> GetParticipant(string login)
        {
            var submissions = await GetSubmissions(login);
            return new Participant(login, submissions);
        }
        public async Task<List<Participant>> GetParticipants(List<string> logins)
        {
            var participants = new List<Participant>();
            foreach (var login in logins)
            {
                var submissions = await GetSubmissions(login);
                participants.Add(new Participant(login, submissions));
            }

            return participants;
        }
    }
    class TaskD
    {
        static async Task<List<Participant>> GetParticipants()
        {
            var contest = Console.ReadLine();
            var client = new ServerClient(contest);
            var participantLogins = await client.GetLogins();
            var participants = await client.GetParticipants(participantLogins.Distinct(StringComparer.Ordinal).ToList());
            return participants;
        }

        static List<string> GetWinners(List<Participant> participants)
        {
            var participantResults = participants.Select(p => p.ComputeResult())
                .ToList();

            var maxCompletedTasksCount = participantResults.Max(r => r.CompletedTasksCount);
            participantResults =
                participantResults
                    .Where(r => r.CompletedTasksCount == maxCompletedTasksCount)
                    .ToList();
            var minFine = participantResults.Min(r => r.Fine);
            var winners = participantResults.Where(r => r.Fine <= minFine)
                //       .Where(r => r.CompletedTasksCount > 0) //---------------------------------!!!

                .Select(r => r.Participant.Login)
                .ToList();
            return winners;
        }

        public static async Task Solve()
        {
            var participants = await GetParticipants();
            var winners = GetWinners(participants);

            winners.Sort(StringComparer.Ordinal);
            Console.WriteLine(winners.Count);
            Console.WriteLine(string.Join(Environment.NewLine, winners));
        }
    }
}
