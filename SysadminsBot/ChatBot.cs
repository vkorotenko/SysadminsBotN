using HtmlAgilityPack;
using RestSharp;
using SysadminsBot.Worker;
using System.Text;
using SysadminsBot.Interfaces;

namespace SysadminsBot;

public class ChatBot(Settings settings)
{
    private const string LoginAddress = "https://sysadmins.ru/login.php";
    private readonly string _user = settings.User;
    private readonly string _password = settings.Password;
    private readonly string[] _topics = settings.Topics.Select(topic => topic.Url).ToArray();
    private readonly string[] _skipUsers = settings.SkipUsers;
    private readonly Settings _settings = settings;
    private readonly int _pollingInterval = settings.PollingInterval;
    public string LastUser { get; private set; } = "";
    public string Sid { get; private set; } = "";
    public bool IsLoggedIn { get; set; }
    public async Task TalkAsync()
    {
        await Login();
        if (IsLoggedIn) foreach (var topic in _topics) await ParseTopic(topic);
        Thread.Sleep(_pollingInterval * 1000);
    }

    private RestRequest CreateRequest()
    {

        var request = new RestRequest();
        const string domain = ".sysadmins.ru";
        const string path = "/";
        var dateTime1 = DateTime.Now;
        var unixTimeSeconds = new DateTimeOffset(dateTime1).ToUnixTimeSeconds();

        request.AddCookie("sysadminsnew_sid", Sid, path, domain);
        request.AddCookie("sysadminsnew___lastvisit", unixTimeSeconds.ToString(), path, domain);
        return request;
    }
    private async Task ParseTopic(string topic)
    {
        var client = new RestClient(topic);
        var request = CreateRequest();
        var response = await client.GetAsync(request);


        if (string.IsNullOrWhiteSpace(response.Content)) return;
        var html = new HtmlDocument();
        html.LoadHtml(response.Content);
        var node = html.DocumentNode
            .SelectSingleNode("//td[contains(@class, 'navbig')]");
        var nodes = node.ChildNodes.Where(x => x.Name == "a").ToList();
        var last = nodes[nodes.Count - 2].Attributes["href"].Value;

        var pageUrl = new Uri("https://sysadmins.ru/" + last);

        client = new RestClient(pageUrl);
        request = CreateRequest();
        response = await client.GetAsync(request);
        if (string.IsNullOrWhiteSpace(response.Content)) return;
        html = new HtmlDocument();
        html.LoadHtml(response.Content);


        var tables = html.DocumentNode
            .SelectNodes("//table[contains(@width, '95%')]");
        var table = tables[1];
        var messages = table.ChildNodes;
        var msg = new List<ForumMessage>();
        foreach (var message in messages)
        {
            if (message.Name == "tr" && message.InnerHtml.Contains("javascript:putName("))
            {

                var messageTable = message.SelectSingleNode("td/table");
                var pm = new ForumMessage();

                var body = messageTable.SelectSingleNode("tr/td/span[contains(@class, 'postbody')]");
                pm.Mid = body.Id;
                //https://sysadmins.ru/topic466970-52670.html
                pm.Pid = pageUrl.AbsoluteUri.Replace("https://sysadmins.ru/topic", "").Split("-")[0];

                var rawText = body.ParentNode.InnerText;

                var bodyes = messageTable.SelectNodes("tr/td/span[contains(@class, 'postbody')]");
                var sign = bodyes.Last().InnerText;
                rawText = rawText.Replace(sign, "");

                pm.Body = rawText;
                var nameNode = message.SelectSingleNode("td/span/b/a");

                pm.Author = nameNode.InnerText;

                var datatitle = messageTable.SelectSingleNode("tr/td/span[contains(@class, 'postdetails')]");

                var datel = datatitle.ChildNodes[0].InnerText.Replace("Добавлено: ", "");
                pm.Date = datel;

                var title = datatitle.ChildNodes[2].InnerText.Replace("&nbsp; &nbsp;Заголовок сообщения: ", "");
                pm.Title = title;
                pm.Url = pageUrl.AbsoluteUri;
                msg.Add(pm);
            }
        }

        foreach (var ms in msg)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine("Author: \t" + ms.Author);
            Console.WriteLine("Base URL: \t" + ms.Url);
            Console.WriteLine("Date: \t" + ms.Date);
            Console.WriteLine("Id: \t" + ms.Mid);
            Console.WriteLine("PId: \t" + ms.Pid);
            Console.WriteLine("Title: \t" + ms.Title);
            Console.WriteLine("Body: \t" + ms.Body);
        }

        if (msg.Last().Author != LastUser)
        {
            await AnswerToLast(msg.Last());
        }
    }

    private ByteArrayContent MakeContext(Encoding enc, string message)
    {
        var encodedTextData = enc.GetBytes(message);
        var textContent = new ByteArrayContent(encodedTextData);
        textContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain")
        {
            CharSet = "windows-1251"
        };
        return textContent;
    }
    private async Task AnswerToLast(ForumMessage last)
    {
        if (_skipUsers.Contains(last.Author))
        {
            return;
        }

        IAiInterface module = _settings.Module switch
        {
            "localdeep" => new LocalDeep(),
            "deepseek" => new DeepSeek(),
            _ => new Script()
        };

        var answer = await module.Reply(last, settings);
        
        var encoding = Encoding.GetEncoding("Windows-1251");
        using var ctx = new MultipartFormDataContent();
        ctx.Add(MakeContext(encoding, answer.Answer), "message"); // "textField" is the form field name
        ctx.Add(MakeContext(encoding, "on"), "attach_sig");
        ctx.Add(MakeContext(encoding, "on"), "notify");
        ctx.Add(MakeContext(encoding, Sid), "sid");
        ctx.Add(MakeContext(encoding, "reply"), "mode");
        ctx.Add(MakeContext(encoding, last.Pid), "t");
        ctx.Add(MakeContext(encoding, "%CE%F2%EF%F0%E0%E2%E8%F2%FC+%28Ctrl%2BEnter%29"), "post");
        var address = "https://sysadmins.ru/posting.php?mode=reply&t=" + last.Pid;


        // Send the POST request
        using var client = new HttpClient();
        var rsp = await client.PostAsync(address, ctx);
        // Check the response
        if (rsp.IsSuccessStatusCode)
        {
            Console.WriteLine("Add comment to: " + last.Body);
            Console.WriteLine("Answer is: " + answer.Answer);
            return;
        }
        Console.WriteLine("Error: " + rsp.StatusCode);
    }

    private async Task Login()
    {
        var values = new Dictionary<string, string>
        {
            { "username", _user },
            { "password", _password },
            {"redirect",""},
            {"login","%C2%F5%EE%E4"}
        };

        var content = new FormUrlEncodedContent(values);
        using var client = new HttpClient();
        var response = await client.PostAsync(LoginAddress, content);

        var buffer = await response.Content.ReadAsByteArrayAsync();

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("Windows-1251");
        var body = encoding.GetString(buffer, 0, buffer.Length);

        if (body.Contains($"Выход [ {_user} ]"))
        {
            if (!IsLoggedIn)
            {
                var rm = response.RequestMessage;
                Sid = response.RequestMessage.RequestUri.AbsoluteUri.Split("sid=")[1];
                IsLoggedIn = true;
            }

        };
    }
}