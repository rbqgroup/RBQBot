using MihaZupan;

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
//using Telegram.Bot.Extensions.Polling;

namespace RBQBot
{
    /// <summary>封装的自动超时移除</summary>
    public class WaitBan
    {
        ITelegramBotClient botClient;

        System.Timers.Timer tm;
        public int FailCount;
        public long ChatId;
        public long UserId;
        public int CallbackMsgId;

        /// <summary>定时自动移除未验证用户</summary>
        /// <param name="chatId">群组ID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="callBackMsgId">验证消息的ID</param>
        /// <param name="timeout">超时时间(毫秒)</param>
        public WaitBan(long chatId, long userId, int callBackMsgId, double timeout, ITelegramBotClient botClient)
        {
            FailCount = 0;
            ChatId=chatId;
            UserId=userId;
            CallbackMsgId = callBackMsgId;
            this.botClient=botClient;

            tm = new System.Timers.Timer();
            tm.AutoReset = false;
            tm.Interval = timeout;
            tm.Elapsed +=Tm_Elapsed;
            tm.Start();
        }

        /// <summary>停止计时器</summary>
        public void Stop() => tm.Stop();

        private void Tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            botClient.BanChatMemberAsync(ChatId, UserId);
            botClient.UnbanChatMemberAsync(ChatId, UserId);

            botClient.DeleteMessageAsync(ChatId, CallbackMsgId);
            botClient.SendTextMessageAsync(
                chatId: ChatId,
                text: $"<a href=\"tg://user?id={UserId}\">Ta</a> 由于验证超时,已被移出本群.",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                disableNotification: true);

            Program.BanList.TryRemove(UserId, out _);
        }
    }

    class Program
    {
        internal static DBHelper DB;
        internal static TelegramBotClient Bot;

        /// <summary>口塞内存队列 (int输入RBQStatus的主键ID)</summary>
        internal static ConcurrentDictionary<int, RBQList> List;
        /// <summary>进群验证队列 (long输入用户的Id)</summary>
        internal static ConcurrentDictionary<long, WaitBan> BanList;

        /// <summary>用户验证超时时间(毫秒)</summary>
        internal static double UserVerifyTime = 120000;
        /// <summary>口塞锁定时间(分钟)</summary>
        internal static int LockTime = 10;

        internal static string Version = "1.0.1.5";
        internal static DateTime StartTime;

        static void Main(string[] args)
        {
            StartTime = DateTime.UtcNow.AddHours(8);

            List = new ConcurrentDictionary<int, RBQList>();
            BanList = new ConcurrentDictionary<long, WaitBan>();

            DB = new DBHelper();

            #region 初始化默认口球列表
            if (DB.GetGagItemCount() == 0)
            {
                DB.AddGagItem("胡萝卜口塞", 0, 1, true, true, null, null, null, null);
                DB.AddGagItem("口塞球", 5, 3, true, true, null, null, null, null);
                DB.AddGagItem("充气口塞球", 15, 10, true, true, null, null, null, null);
                DB.AddGagItem("深喉口塞", 25, 20, true, true, null, null, null, null);
                DB.AddGagItem("金属开口器", 50, 45, true, true, null, null, null, null);
                DB.AddGagItem("炮机口塞", 100, 80, true, false, null, null, null, null);
                DB.AddGagItem("超级口塞", 1000, 120, false, false, null, null, null, null);
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

            #region 恢复内存队列
            var rec = DB.GetAllRBQStatus();
            foreach (var i in rec)
            {
                var tm = new DateTime(i.StartLockTime).AddMinutes(10);
                if (i.LockCount > 0 && DateTime.UtcNow.AddHours(8) < tm)
                {
                    var timeout = (tm - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                    var rbqx = new RBQList(i.Id, i.LockCount, i.GagId, timeout);
                    Program.List.TryAdd(i.Id, rbqx);
                }
            }
            #endregion

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