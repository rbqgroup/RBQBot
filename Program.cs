using MihaZupan;

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
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

        internal static System.Timers.Timer msgCountTm;
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
        internal static string Version = "1.1.8.0";

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
            "/about - 查看有关本 Bot 本身的相关信息 (如玩法、介绍、隐私权、许可、反馈等).\n" +
            "/privacy - 关于隐私的信息";

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

        internal static readonly string PrivacyTxt =
            "本 bot 不会主动收集聊天内容, 但会存在聊天计数器收集用户id和发送消息次数(非内存易失), 并且每天会重置.\n" +
            "本 bot 只有在出错时和错误操作时会输出可能存在的相关信息, 例如谁调用了这条命令, 详细的堆栈(可能包含大量信息), 以用作错误调试, 但会在使用后删除.\n" +
            "本 bot 不会与任何第三方API交互获取通信.\n" +
            "本 bot 具有管理员权限, 仅为实现玩法和功能, 不会进行滥权(排除群组内的管理员滥用), 如有非/help命令中的异常管理员操作, 请与作者反馈.\n" +
            "由 MintNeko 维护的实例将不会使用任何位于国内机房或公司位于中国内地的云服务运营商. 自建实例的话也强烈建议如此, 虽然有使用 Socks5 代理功能, 但并不推荐使用. \n" +
            "本着隐私和透明度, 本程序完全开源, 但还请准守以下规定: \n" +
            "许可: 在使用本程序或源代码时请遵守 MIT License 并且禁止违反当地法律之用途.\n" +
            "源代码: https://github.com/RBQGroup/RBQBot \n" +
            "不保证服务质量: 本 bot 可能会随时进入维护甚至生产环境在线开发并不另行通知, 如果突然停止响应请稍后再试, 因为请求可能会产生堆积而在恢复服务后同时执行而发生预期外的行为.\n" +
            "滥用警告: 对本 bot 的滥用行为可能会被管理员加入黑名单而被禁止使用本 bot 中的任何命令. 本 bot 具有积分功能, 但严禁为了刷分而在群组中刷屏, 这与增加群聊乐趣的初衷本末倒置.\n";
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

                Console.WriteLine($"Running, Please Wait {WaitTime}MS To Process Message");
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

                #region 创建凌晨输出群活跃信息
                msgCountTm = new System.Timers.Timer();
                msgCountTm.AutoReset = false;
                msgCountTm.Elapsed += MsgCountTm_Elapsed;
                msgCountTm.Interval = (DateTime.Today.AddDays(1) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                msgCountTm.Start();
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

        public static void MsgCountTm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var lists = DB.GetAllMessageCounts();
            var count = DB.GetMessageCountTableCount();

            var list = new Model.MsgCount[count];
            var group = new long[count];
            int index = 0;

            foreach (var i in lists)
            {
                bool haveGroup = false;
                int downC = -1;
                for (int i2 = 0; i2 < group.Length; i2++)
                {
                    if (group[i2] == i.GroupId) { haveGroup = true; break; }
                    if (group[i2] == 0 && downC == -1) downC = i2;
                }
                if (haveGroup == false) { group[downC] = i.GroupId; downC = -1; }
                list[index].GroupId = i.GroupId;
                list[index].UserId = i.UserId;
                list[index].Count = i.Count;
                index++;
            }

            if (list.Length > 0)
            {
                DB.DropMessageCountTable();

                for (int i = 0; i < group.Length; i++)
                {
                    if (group[i] == 0) break;
                    var kvp = new Model.MsgCountX[5];
                    var temp = new Model.MsgCountX[5];
                    for (int i2 = 0; i2 < list.Length; i2++)
                    {
                        if (list[i2].GroupId == group[i])
                        {
                            for (int i3 = 0; i3 < kvp.Length; i3++)
                            {
                                if (list[i2].Count >= kvp[i3].Count)
                                {
                                    if (i3+1 > kvp.Length)
                                    {
                                        kvp[i3].UserId = list[i2].UserId;
                                        kvp[i3].Count = list[i2].Count;
                                        break;
                                    }
                                    if (kvp[i3].Count == 0)
                                    {
                                        kvp[i3].UserId = list[i2].UserId;
                                        kvp[i3].Count = list[i2].Count;
                                        break;
                                    }

                                    Array.Copy(kvp, i3, temp, 0, kvp.Length - i3);
                                    kvp[i3].UserId = list[i2].UserId;
                                    kvp[i3].Count = list[i2].Count;
                                    if (i3==0) Array.Copy(temp, 0, kvp, i3 + 1, kvp.Length - i3 - 1);
                                    else Array.Copy(temp, i3, kvp, i3, kvp.Length - i3);

                                    break;
                                }
                            }
                        }
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine(DB.GetAllowMsgCountStr(group[i]));
                    sb.AppendLine();
                    sb.AppendLine("昨天群里最活跃的人是: ");

                    for (int si = 0; si < kvp.Length; si++)
                    {
                        if (kvp[si].UserId == 0) break;
                        var result = Bot.GetChatMemberAsync(group[i], kvp[si].UserId).Result;
                        sb.AppendLine($"TOP {si+1} :  <a href=\"tg://user?id={kvp[si].UserId}\"><b><u>{result.User.FirstName} {result.User?.LastName}</b></u></a>  ({kvp[si].Count})");
                    }

                    Bot.SendTextMessageAsync(
                        chatId: group[i],
                        disableNotification: true,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                        text: sb.ToString());
                }
            }

            msgCountTm.Interval = (DateTime.Today.AddDays(1) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
            msgCountTm.Start();
        }
    }
}
