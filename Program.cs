using MihaZupan;

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
//using Telegram.Bot.Extensions.Polling;

namespace RBQBot
{
    public class WaitBan
    {
        System.Timers.Timer tm;
        public int FailCount;
        public long ChatId;
        public long UserId;
        public int CallbackMsgId;
        object obj;
        ITelegramBotClient botClient;

        public WaitBan(long chatId, long userId, int callBackMsgId, double timeout, object userBanList, ITelegramBotClient botClient)
        {
            FailCount = 0;
            ChatId=chatId;
            UserId=userId;
            CallbackMsgId = callBackMsgId;
            obj=userBanList;
            this.botClient=botClient;

            tm = new System.Timers.Timer();
            tm.AutoReset = false;
            tm.Interval = timeout;
            tm.Elapsed +=Tm_Elapsed;
            tm.Start();
        }

        public void Stop() => tm.Stop();

        private void Tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var list = (ConcurrentDictionary<long, WaitBan>)obj;

            botClient.BanChatMemberAsync(ChatId, UserId);
            botClient.UnbanChatMemberAsync(ChatId, UserId);

            botClient.DeleteMessageAsync(ChatId, CallbackMsgId);
            botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: $"<a href=\"tg://user?id={UserId}\">Ta</a> 由于验证超时,已被移出本群.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableNotification: true);

            list.TryRemove(UserId, out WaitBan _);
        }
    }

    class Foo
    {
        public string GetAssemblyVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
    }

    class Program
    {
        internal static DBHelper DB;
        internal static TelegramBotClient Bot;
        /// <summary>int是RBQStatus的主键ID, RBQList是RBQStatus的衍生封装</summary>
        internal static ConcurrentDictionary<int, RBQList> List;
        internal static ConcurrentDictionary<long, WaitBan> BanList;
        /// <summary>用户验证超时时间(毫秒)</summary>
        internal static double UserVerifyTime = 120000;
        /// <summary>口塞锁定时间(分钟)</summary>
        internal static int LockTime = 10;
        internal static string Version = "1.0.1.0";
        internal static DateTime StartTime;

        static void Main(string[] args)
        {
            StartTime = DateTime.UtcNow.AddHours(8);
            List = new ConcurrentDictionary<int, RBQList>();
            BanList = new ConcurrentDictionary<long, WaitBan>();

            #region 生成绒话的测试代码
            //string[] charta = {
            //    "呜",
            //    "哈",
            //    "啊", "啊", "啊", "啊", "啊",
            //    "啊啊", "啊啊", "啊啊", "啊啊",
            //    "啊啊啊", "啊啊啊", "啊啊啊",
            //    "唔",
            //    "嗯", "嗯", "嗯", "嗯",
            //    "嗯嗯", "嗯嗯",
            //    "呃", "呃呃",
            //    "哦", "哦哦", "哦哦哦",
            //    "嗷", "嗷嗷",
            //    "呕",
            //    "噢",
            //    "喔", "喔喔", "喔喔喔",

            //    "唔嗯", "唔嗯",
            //    "唔啊",
            //};
            //string[] chartb = {
            //    "…", "…", "…", "…", "…", "…",
            //    "……", "……", "……", "……",
            //    "………", "………",
            //    "！", "！！",
            //    "？", "？？",
            //    "！？", "？！",
            //    "，", "，，"
            //};

            //var R = new Random();
            //var sb = new System.Text.StringBuilder();

            //for (int i = 0; i < 7; i++)
            //{
            //    sb.Append(charta[R.Next(0, charta.Length)]);
            //    if (i % 2 == 1)
            //    {
            //        sb.Append(chartb[R.Next(0, chartb.Length)]);
            //    }
            //}
            //var a = sb.ToString();
            //Console.WriteLine(a);
            //Console.WriteLine(Handlers.TypeProcess(a));
            //Console.ReadLine();
            #endregion

            DB = new DBHelper();
#if DEBUG
            //DB.AddGagItem("test1", 1, 1, null, null, null, null);
            //DB.AddGagItem("test2", 10, 10, null, null, null, null);
#endif
            #region 恢复内存队列
            var rec = DB.GetAllRBQStatus();
            foreach (var i in rec)
            {
                var tm = new DateTime(i.StartLockTime).AddMinutes(10);
                if (i.LockCount > 0 && DateTime.UtcNow.AddHours(8) < tm)
                {
                    var timeout = (tm - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                    var rbqx = new RBQList(i.Id, i.LockCount, i.GagId, timeout, Program.List);
                    Program.List.TryAdd(i.Id, rbqx);
                }
            }
            #endregion

            var proxy = new HttpToSocks5Proxy("127.0.0.1", 55555);
            var httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = true });

            Bot = new TelegramBotClient("", httpClient);

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(Handlers.HandleUpdateAsync,
                               Handlers.HandleErrorAsync,
                               receiverOptions,
                               cts.Token);

            Console.WriteLine("Running...");
            while (true)
            {
                Console.ReadLine();
                Console.Clear();
            }

            // Send cancellation request to stop bot
#pragma warning disable CS0162 // 检测到无法访问的代码
            cts.Cancel();
#pragma warning restore CS0162 // 检测到无法访问的代码
        }
    }
}