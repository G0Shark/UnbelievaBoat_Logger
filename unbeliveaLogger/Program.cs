using HtmlAgilityPack;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

class User
{
    public string nickname = "";
    public int balance = 0;
    public User(string nickname, int balance)
    {
        this.nickname = nickname;
        this.balance = balance;
    }
}

class Program
{

    public static string sigmalol = "";
    public static Dictionary<string, int> old = new Dictionary<string, int>(); //прошлый
    public static Dictionary<string, int> now = new Dictionary<string, int>(); //следующий
    public static int how_much = 1000;
    static void Main(string[] args)
    {
        string url = "https://unbelievaboat.com/leaderboard/1043887403355615323/widget"; // Замените на URL сайта, который вы хотите скачать
        string filePath = @".\sigmas.html"; // Замените на путь к файлу, куда вы хотите сохранить HTML

        while (true)
        {
            Thread trd = new Thread(() => { sigmalol = SavePageAfterLoad(url); });
            trd.Start();

            int sec = 300;
            while (sec > 0)
            {
                if (sec > 60)
                {
                    Console.Title = $"Осталось {sec/60} минут и {sec-(sec/60*60)} секунд";
                }
                else
                {
                    Console.Title = $"Осталось {sec} секунд";
                }
                Thread.Sleep(1000);
                sec--;
            }

            trd.Join();
            now = FindDivWithClasses(sigmalol);

            PrintOtlychiya(old, now);

            old = now;
            now = new Dictionary<string, int>();
        }
    }

    public static void PrintOtlychiya(Dictionary<string, int> one, Dictionary<string, int> two)
    {
        string dt = TimeForCon();
        string tmmp = "Было загруженно " + two.Count + " участников";

        for (int i2 = 0; i2 < 70 - tmmp.Length; i2++)
        {
            tmmp += " ";
        }

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(dt);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("]: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(tmmp + $"({one.Count} => {two.Count})\n");
        for (int i = 0; i < one.Count; i++)
        {
            try
            {
                if (one.ElementAt(i).Value < two[one.ElementAt(i).Key])
                {
                    int otl = two[one.ElementAt(i).Key] - one.ElementAt(i).Value;
                    string tmp = "";
                    string tmp2 = one.ElementAt(i).Key + " получил " + otl + " валюты";

                    for (int i2 = 0; i2 < 70 - tmp2.Length; i2++)
                    {
                        tmp += " ";
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(dt);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("]: ");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(one.ElementAt(i).Key);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" получил ");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(two[one.ElementAt(i).Key] - one.ElementAt(i).Value);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" валюты" + tmp);
                    Console.Write($"({one.ElementAt(i).Value} => {two[one.ElementAt(i).Key]})\n");

                    SaveInfToFile($"[{dt}]: {one.ElementAt(i).Key} получил {two[one.ElementAt(i).Key] - one.ElementAt(i).Value} валюты");

                }
                if (one.ElementAt(i).Value > two[one.ElementAt(i).Key])
                {
                    int otl = one.ElementAt(i).Value - two[one.ElementAt(i).Key];
                    string tmp = "";
                    string tmp2 = one.ElementAt(i).Key + " потерял " + otl + " валюты";

                    for (int i2 = 0; i2 < 69 - tmp2.Length; i2++)
                    {
                        tmp += " ";
                    }

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(dt);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("]: ");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(one.ElementAt(i).Key);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" потерял ");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(two[one.ElementAt(i).Key] - one.ElementAt(i).Value);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" валюты" + tmp);
                    Console.Write($"({one.ElementAt(i).Value} => {two[one.ElementAt(i).Key]})\n");

                    SaveInfToFile($"[{dt}]: {one.ElementAt(i).Key} потерял {one[two.ElementAt(i).Key] - two.ElementAt(i).Value} валюты");

                }
            } catch(Exception e) {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(dt);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("]: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(e.Message);
            }
        }
    }

    public static Dictionary<string, int> FindDivWithClasses(string htmlCode)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlCode);

        // Ищем div с классами "text-center", "px-1", и "col-5"
        var nicks = document.DocumentNode.Descendants("div")
                                       .Where(d => d.GetAttributeValue("class", "") == "text-left username col-4");

        var vals = document.DocumentNode.Descendants("div")
                                       .Where(d => d.GetAttributeValue("class", "") == "text-center px-1 col-5");

        Dictionary<string, int> users = new Dictionary<string, int>();    

        for ( var i = 0; i < vals.Count(); i++ )
        {
            //Console.WriteLine(nicks.ElementAt(i).ChildNodes.First().InnerHtml.Trim() + " | " + int.Parse(RemoveNbsp(vals.ElementAt(i).ChildNodes.First().ChildNodes.Last().InnerHtml.Trim())));
            try
            {
                users.Add(nicks.ElementAt(i).ChildNodes.First().InnerHtml.Trim(), int.Parse(RemoveNbsp(vals.ElementAt(i).ChildNodes.First().ChildNodes.Last().InnerHtml.Trim())));
            } catch { }
        }

        return users;
    }
    static async Task CheckForUpdates()
    {
        string currentVersion = "1.0.0"; // Замените на текущую версию вашего приложения
        string githubToken = "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"; // Замените на ваш токен доступа к GitHub
        string repoOwner = "G0Shark_"; // Замените на владельца репозитория
        string repoName = "UnbelivaLeaderboardLogger"; // Замените на название репозитория

        // Получаем информацию о последнем релизе
        string releasesUrl = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest?access_token={githubToken}";
        using (HttpClient httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync(releasesUrl);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic releaseData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                string latestVersion = releaseData.tag_name; // Получаем номер версии релиза

                if (latestVersion != currentVersion)
                {
                    Console.WriteLine("New update available.");

                    // Скачиваем обновление
                    foreach (var asset in releaseData.assets)
                    {
                        string downloadUrl = asset.browser_download_url;
                        string fileName = Path.GetFileName(new Uri(downloadUrl).AbsolutePath);
                        using (HttpResponseMessage downloadResponse = await httpClient.GetAsync(downloadUrl))
                        {
                            if (downloadResponse.IsSuccessStatusCode)
                            {
                                using (FileStream fileStream = File.OpenWrite(fileName))
                                {
                                    await downloadResponse.Content.CopyToAsync(fileStream);
                                }
                                Console.WriteLine($"Downloaded: {fileName}");
                            }
                        }
                    }

                    // Перезапускаем приложение с новой версией
                    Environment.Exit(0);
                }
            }
        }
    }
    public static string RemoveNbsp(string input)
    {
        // Регулярное выражение для поиска символов &nbsp;
        string pattern = "&nbsp;";



        // Замена найденных символов &nbsp; на пустую строку
        return Regex.Replace(input, pattern, "");
    }

    static string TimeForCon()
    {
        StringBuilder ret = new StringBuilder();

        if (DateTime.Now.TimeOfDay.Hours < 10)
        {
            ret.Append("0" + DateTime.Now.TimeOfDay.Hours);
        }
        else
        {
            ret.Append(DateTime.Now.TimeOfDay.Hours);
        }

        ret.Append(":");

        if (DateTime.Now.TimeOfDay.Minutes < 10)
        {
            ret.Append("0" + DateTime.Now.TimeOfDay.Minutes);
        }
        else
        {
            ret.Append(DateTime.Now.TimeOfDay.Minutes);
        }

        ret.Append(":");

        if (DateTime.Now.TimeOfDay.Seconds < 10)
        {
            ret.Append("0" + DateTime.Now.TimeOfDay.Seconds);
        }
        else
        {
            ret.Append(DateTime.Now.TimeOfDay.Seconds);
        }

        return ret.ToString();
    }

    static string TimeToFile()
    {
        return DateTime.Now.Hour + "H" + DateTime.Now.Minute + "M" + DateTime.Now.Second + "S";
    }
    static void SaveInfToFile(string sigma)
    {
        try
        {
            if (!Directory.Exists("./logs"))
            {
                Directory.CreateDirectory("./logs");
            }

            if (!File.Exists("./logs/" + DateTime.Now.Year + "Y" + DateTime.Now.Month + "M" + DateTime.Now.Day + "D.log"))
            {
                File.Create("./logs/" + DateTime.Now.Year + "Y" + DateTime.Now.Month + "M" + DateTime.Now.Day + "D.log").Close();
            }

            File.AppendAllText("./logs/" + DateTime.Now.Year + "Y" + DateTime.Now.Month + "M" + DateTime.Now.Day + "D.log", sigma + "\n");
        } catch(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[FILEMNGR]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.ToString());
        }
    }
    static string SavePageAfterLoad(string url)
    {
        ChromeDriverService service = ChromeDriverService.CreateDefaultService();
        service.EnableVerboseLogging = false; // Отключаем подробное логирование
        service.SuppressInitialDiagnosticInformation = true;
        service.HideCommandPromptWindow = true;

        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless");
        chromeOptions.AddArgument("--silent");
        chromeOptions.AddArgument("--disable-gpu");
        chromeOptions.AddArgument("--log-level=3");
        chromeOptions.AddArgument("--disable-extensions");
        chromeOptions.AddArgument("test-type");// Для запуска в режиме headless

        using (var driver = new ChromeDriver(service, chromeOptions))
        {
            driver.Navigate().GoToUrl(url);

            // Даем время на загрузку всех элементов страницы
            System.Threading.Thread.Sleep(10000); // Ждем 10 секунд
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"window.scrollBy(0, 3000)");

            System.Threading.Thread.Sleep(30000); // Ждем 10 секунд
            js.ExecuteScript($"window.scrollBy(0, 3000)");

            System.Threading.Thread.Sleep(60000); // Ждем 10 секунд
            js.ExecuteScript($"window.scrollBy(0, 3000)");

            System.Threading.Thread.Sleep(80000); // Ждем 10 секунд
            js.ExecuteScript($"window.scrollBy(0, 3000)");

            System.Threading.Thread.Sleep(120000); // Ждем 10 секунд
            js.ExecuteScript($"window.scrollBy(0, 3000)");

            // Сохраняем HTML страницы
            return driver.PageSource;
        }
    }
}