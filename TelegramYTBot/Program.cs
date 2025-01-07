using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using System.Net;

var ytdl = new YoutubeDL();
ytdl.OutputFolder = "downloads";
// replace YOUR_BOT_TOKEN below, or set your TOKEN in Project Properties > Debug > Launch profiles UI > Environment variables
var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? "YOUR_BOT_TOKEN";

var listener = new HttpListener();
listener.Prefixes.Add("http://127.0.0.1:7860/");
listener.Start();
Task.Run(async() => { await ListenAsync(); });
async Task ListenAsync() {
    while (true)
    {
        Console.WriteLine("LA");
        var context = await listener.GetContextAsync();
        context.Response.StatusCode = (int)HttpStatusCode.OK;
    }
}

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMe();
await bot.DeleteWebhook();          // you may comment this line if you find it unnecessary
await bot.DropPendingUpdates();     // you may comment this line if you find it unnecessary
bot.OnError += OnError;
bot.OnMessage += OnMessage;

Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
cts.Cancel(); // stop the bot

async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
}

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is not { } text)
        Console.WriteLine($"Received a message of type {msg.Type}");
    await OnTextMessage(msg);
}

async Task OnTextMessage(Message msg) // received a text message that is not a command
{
    Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
    if (msg.Text == "/start")
    {
        await bot.SendMessage(msg.Chat, "Добро пожаловать в жестокую африку. " +
            "Скинь мне ссылку на видео чтобы я тебе его скачал. Для этого одолжы мне свою почку. " +
            "Это шутка. ГОНИ НАЛОГИ. Это ссылка на канал мого создателя имя которого тебе никогда не знать " +
            "https://t.me/ftcttdft77gygi");
    }
    else {
        await bot.SendMessage(msg.Chat, "Подожди. Я скачиваю видео.");
        try 
        {
            var res = await ytdl.RunVideoDownload(msg.Text, "bestvideo+bestaudio/best", DownloadMergeFormat.Mp4);
            using (FileStream fs = System.IO.File.OpenRead(res.Data))
            {
                await bot.SendVideo(msg.Chat, InputFile.FromStream(fs));
            }
            System.IO.File.Delete(res.Data);
        }
        catch (Exception ex) 
        {
            await bot.SendMessage(msg.Chat, "Ошибка загрузки видео.");
        }
    }
}