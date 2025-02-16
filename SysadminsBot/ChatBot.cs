using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting.Server;
using RestSharp;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;

namespace SysadminsBot;

public class ChatBot(string user, string password, string[] topics, string[] skipUsers, string apiKey)
{
    private const string Area = "https://sysadmins.ru/forum1.html";
    private const string LoginAddress = "https://sysadmins.ru/login.php";
    private readonly string _user = user;
    private readonly string _password = password;
    private readonly string[] _topics = topics;
    private readonly string[] _skipUsers = skipUsers;
    private readonly HttpClient _client = new HttpClient();
    private readonly DeepSeekResult _deepSeek = new DeepSeekResult(apiKey);
    public string LastUser { get; private set; } = "";
    public string Sid { get; private set; }
    public bool IsLoggedIn { get; set; }
    public async Task TalkAsync()
    {

        await Login();
        if (IsLoggedIn)
        {
            var client = new RestClient(Area);
            var request = new RestRequest();
            var domain = ".sysadmins.ru";
            var path = "/";
            //1739004710
            var dateTime1 = DateTime.Now;
            var unixTimeSeconds = new DateTimeOffset(dateTime1).ToUnixTimeSeconds();

            request.AddCookie("sysadminsnew_sid", Sid, path, domain);
            request.AddCookie("sysadminsnew___lastvisit", unixTimeSeconds.ToString(), path, domain);
            var response = await client.GetAsync(request);
            // ParseHtml(response.Content);

            foreach (var topic in _topics)
            {
                await ParseTopic(topic);
            }
        }
        else
        {
            await Login();
        }
        Thread.Sleep(1000);
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
        var hrefs = node.ChildNodes.Where(x => x.Name == "a").ToList();
        var last = hrefs[hrefs.Count - 2].Attributes["href"].Value;

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

                var mtable = message.SelectSingleNode("td/table");
                var pm = new ForumMessage();

                var body = mtable.SelectSingleNode("tr/td/span[contains(@class, 'postbody')]");
                pm.Mid = body.Id;
                //https://sysadmins.ru/topic466970-52670.html
                pm.Pid = pageUrl.AbsoluteUri.Replace("https://sysadmins.ru/topic", "").Split("-")[0];



                var bodyes = mtable.SelectNodes("tr/td/span[contains(@class, 'postbody')]");

                for (var i = 0; i < bodyes.Count; i++)
                {
                    var b = bodyes[i];
                    if (!string.IsNullOrWhiteSpace(b.InnerText) && i < bodyes.Count - 1)
                        pm.Body += b.InnerText + "\r\n";
                }
                var nameNode = message.SelectSingleNode("td/span/b/a");

                pm.Author = nameNode.InnerText;

                var datatitle = mtable.SelectSingleNode("tr/td/span[contains(@class, 'postdetails')]");

                var datel = datatitle.ChildNodes[0].InnerText.Replace("Добавлено: ", "");
                pm.Date = datel;

                var title = datatitle.ChildNodes[2].InnerText.Replace("&nbsp; &nbsp;Заголовок сообщения: ", "");
                pm.Title = title;
                msg.Add(pm);
            }
        }

        foreach (var ms in msg)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine("Author: \t" + ms.Author);
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

        var encoding = Encoding.GetEncoding("Windows-1251");
        var chatResponse = await _deepSeek.Reply(last.Body);
        var msg = $"{last.Body}";

        using var ctx = new MultipartFormDataContent();
        ctx.Add(MakeContext(encoding,msg), "message"); // "textField" is the form field name
        ctx.Add(MakeContext(encoding, "on"), "attach_sig");
        ctx.Add(MakeContext(encoding, "on"), "notify");
        ctx.Add(MakeContext(encoding, Sid), "sid");
        ctx.Add(MakeContext(encoding, "reply"), "mode");
        ctx.Add(MakeContext(encoding, last.Pid), "t");
        ctx.Add(MakeContext(encoding, "%CE%F2%EF%F0%E0%E2%E8%F2%FC+%28Ctrl%2BEnter%29"), "post");
        var address = "https://sysadmins.ru/posting.php?mode=reply&t=" + last.Pid;

        if (chatResponse != null)
        {
            // Send the POST request
            var rsp = await _client.PostAsync(address, ctx);
            // Check the response
            if (rsp.IsSuccessStatusCode)
            {
                var responseData = await rsp.Content.ReadAsStringAsync();
                Console.WriteLine("Add comment to: " + last.Body);
                return;
            }
            else
            {
                Console.WriteLine("Error: " + rsp.StatusCode);
            }
        }
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

        var response = await _client.PostAsync(LoginAddress, content);

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