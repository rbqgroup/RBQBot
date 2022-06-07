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
    class Program
    {
        internal static DateTime StartTime;

        internal static DBHelper DB;
        internal static TelegramBotClient Bot;

        /// <summary>口塞内存队列 (int输入RBQStatus的主键ID)</summary>
        internal static ConcurrentDictionary<int, RBQList> List;
        /// <summary>进群验证队列 (long输入用户的Id)</summary>
        internal static ConcurrentDictionary<long, WaitBan> BanList;

        #region 配置
        /// <summary>用户验证超时时间(毫秒)</summary>
        internal static double UserVerifyTime = 120000;
        /// <summary>口塞锁定时间(分钟)</summary>
        internal static int LockTime = 10;
        /// <summary>启动后等待多久时间用于忽略消息(毫秒)</summary>
        internal static int WaitTime = 10000;

        /// <summary>版本号(主要.次要.功能.修订)</summary>
        internal static string Version = "1.0.4.0";
        #endregion

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

            using (var cts = new CancellationTokenSource())
            {
                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };

                #region 机器人启动后忽略WaitTime时间的消息
                using (var x = new CancellationTokenSource())
                {
                    Bot.StartReceiving(
                        Handlers.HandleUpdateAsyncIgnore,
                        Handlers.HandleErrorAsync,
                        receiverOptions,
                        x.Token);
                    Thread.Sleep(WaitTime);
                    x.Cancel();
                }
                #endregion

                Bot.StartReceiving(Handlers.HandleUpdateAsync,
                                   Handlers.HandleErrorAsync,
                                   receiverOptions,
                                   cts.Token);

                #region 恢复内存队列
                var rec = DB.GetAllRBQStatus();
                foreach (var i in rec)
                {
                    var tm = new DateTime(i.StartLockTime).AddMinutes(LockTime);
                    if (i.LockCount > 0)
                    {
                        #region 在锁定时间内恢复添加
                        if (DB.GetRBQCanLock(i.GroupId, i.RBQId))
                        {
                            var timeout = (tm - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                            var rbqx = new RBQList(i.Id, i.LockCount, i.GagId, timeout);
                            Program.List.TryAdd(i.Id, rbqx);
                        }
                        #endregion
                        else // 口塞超时 恢复绒布球自由身
                        {
                            i.LockCount = 0;
                            i.GagId = 0;
                            i.FromId = new long[0];
                            i.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                            DB.SetRBQStatus(i);
                        }
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
}
