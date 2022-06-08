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
        internal static volatile bool IsDebug = false;

        internal static DBHelper DB;
        internal static TelegramBotClient Bot;

        /// <summary>口塞内存队列 (int输入RBQStatus的主键ID)</summary>
        internal static ConcurrentDictionary<int, RBQList> List;
        /// <summary>进群验证队列 (long输入用户的Id)</summary>
        internal static ConcurrentDictionary<long, WaitBan> BanList;

        #region 配置
        /// <summary>Debug管理员的用户Id</summary>
        internal static long DebugUserId = 1324338125;
        /// <summary>用户验证超时时间(毫秒)</summary>
        internal static double UserVerifyTime = 120000;
        /// <summary>口塞锁定时间(分钟)</summary>
        internal static int LockTime = 10;
        /// <summary>启动后等待多久时间用于忽略消息(毫秒)</summary>
#if DEBUG
        internal static int WaitTime = 1000;
#else
        internal static int WaitTime = 10000;
#endif

        /// <summary>版本号(主要.次要.功能.修订)</summary>
        internal static string Version = "1.0.5.6";

        internal static readonly string HelpTxt =
            "/count - 查询口塞次数, 只能在群组内使用.\n" +
            "/help - 显示此帮助.\n" +
            "/gag - 对消息回复会给被回复的人「加固」口塞, 您只能对同一个人加固一次口塞. 如果您帮一个人「佩戴」过口塞, 那么您不能在对方口塞未结束之前再次「佩戴」或「加固」口塞.\n" +
            "/gag 口塞名称 - 给自己「佩戴」口塞, 对消息回复会给被回复的人「佩戴」口塞.\n" +
            "/gag on - 允许其他人给自己「佩戴」口塞, 为了防止骚扰默认是不允许的.\n" +
            "/gag off - 不允许其他人给自己「佩戴」口塞, 如果已经处于「佩戴」状态, 不会影响当前已经「佩戴」中的口塞.\n" +
            "/rbqpoint - 查询自己的「绒度」. 对用户发送的消息回复会查询被回复的人的「绒度」.\n" +
            "/list - 显示口塞列表.\n" +
            "/ping - 检查Bot是否在线(这是一个管理员命令, 只能由群组的管理员使用).\n" +
            "/about - 查看有关本 Bot 本身的相关信息 (如玩法、介绍、隐私权、许可、反馈等).\n";

        internal static readonly string AboutTxt =
            "================Bot功能================\n" +
            "它是一个娱乐用的Bot  能让群里指定的用户\n" +
            "短时间内只能发送包含 「指定字符」的消息\n" +
            "发送非指定内容会被删  但仅限于文字/贴纸\n" +
            "================作用范围===============\n" +
            "「绒度」- 在本 Bot 所在的任何群组通用\n" +
            "「开关」- 在本 Bot 所在的任何群组通用\n" +
            "「开关」- 仅对「口塞」功能有效\n" +
            $"「口塞」- 仅在指定群组适用 如果 {Program.LockTime} 分钟\n" +
            "没有「挣扎」或者「加固」操作将会自动解除\n" +
            "若有「挣扎」或者「加固」操作则会重新计时\n" +
            "================挣扎方法===============\n" +
            "戴上口塞后, 你接下来发送的消息中\n" +
            "必须包含且只包含以下文字(一个或多个)\n" +
            "文字: '<code>呜</code>' '<code>哈</code>' '<code>啊</code>' '<code>唔</code>' '<code>嗯</code>' '<code>呃</code>' '<code>哦</code>' '<code>嗷</code>' '<code>呕</code>' '<code>噢</code>' '<code>喔</code>'\n" +
            "且* 同时* 包含且只包含以下符号(一个和多个): \n" +
            "符号: '<code>.</code>' '<code>…</code>' '<code>,</code>' '<code>，</code>' '<code>!</code>' '<code>！</code>' '<code>?</code>' '<code>？</code>'\n" +
            "不符合上述规则的文字消息将无法发送\n" +
            "(多媒体信息除外, 同时也不会计算「绒度」)\n" +
            "只有在挣脱之后(发送指定次数符合规则的消息)\n" +
            "才能恢复到自由发言的状态, 并获得一个冷却时间\n" +
            "「绒度」是发送符合规则的消息一次获得的分数\n" +
            "(不是按字符数量计算), 自己的「绒度」会增加 1\n" +
            "如果发送了不符合规则的消息则不会增加绒度\n" +
            "例如不符合规则的文字消息或贴纸消息\n" +
            "正确示例:「唔…！」,「哈…啊…」\n" +
            "错误示例:「怎么回事」,「呜」\n" +
            "当然您也可以直接 @本Bot 输入一个空格\n" +
            "「<code>@RBQExBot</code>」\n" +
            "然后选择「说猫话」来快速输入符合规则的消息\n" +
            "被戴口塞了的话最好在第二个人加固之前逃脱哦\n";
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
                        if (DB.GetRBQCanLock(i.GroupId, i.RBQId) != true)
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
