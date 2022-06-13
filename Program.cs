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
        /// <summary>管理员的用户Id</summary>
        internal static long AdminId = 66816867;
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
        internal static string Version = "2.0.0.0";

        internal static readonly string AdminTxt =
            "===========超管命令(需要私聊)==========\n" +
            "<code>/admin GetAllowGroup 分页数</code> - 获取所有允许使用的群组列表 (没写分页数就默认分页)\n" +
            "<code>/admin AddAllowGroup 群组Id</code> - 添加允许使用的群组\n" +
            "<code>/admin DelAllowGroup 群组Id</code> - 删除允许使用的群组\n" +
            "<code>/admin SetAllowGroup 群组Id 允许口塞 允许验证 允许消息计数 欢迎消息</code> - 设置已允许使用的群组功能\n" +
            "<code>/admin AddGagItem 口塞名称</code> - 添加口塞\n" +
            "<code>/admin DelGagItem 口塞名称</code> - 删除口塞\n" +
            "<code>/admin SetGagItem 特定Json</code> - 设置口塞参数\n" +
            "<code>/admin AddRBQPoint 用户Id 绒度</code> - 增加绒布球绒度\n" +
            "<code>/admin DelRBQPoint 用户Id 绒度</code> - 减少绒布球绒度\n" +
            "<code>/admin SetRBQPoint 用户Id 绒度</code> - 设置绒布球绒度\n" +
            "=========群管命令(需要回复消息)========\n" +
            "<code>/admin AddRBQLockCount 次数</code> - 增加绒布球挣脱次数\n" +
            "<code>/admin DelRBQLockCount 次数</code> - 减少绒布球挣脱次数\n" +
            "<code>/admin ClearLockCount</code> - 清零绒布球挣脱次数\n" +
            "<code>/admin SetGag 口塞名</code> - 设置绒布球口塞为指定口塞 (同时会设置次数设置来源为Ta自己)\n" +
            "<code>/admin ClearGagFrom</code> - 清除用户的所有添加/加固口塞来源 (所有人再次能加固口塞)";

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

            #region 初始化默认口塞列表
            if (DB.GetGagItemCount() == 0)
            {
                DB.AddGagItem("巧克力口塞", -100, 1, true, true, null, null, null, null);
                DB.AddGagItem("胡萝卜口塞", 0, 1, true, true, null, null, null, null);
                DB.AddGagItem("口塞球", 5, 3, true, true, null, null, null, null);
                DB.AddGagItem("充气口塞球", 15, 10, true, true, null, null, null, null);
                DB.AddGagItem("深喉口塞", 25, 20, true, true, null, null, null, null);
                DB.AddGagItem("金属开口器", 50, 45, true, true, null, null, null, null);
                DB.AddGagItem("炮机口塞", 100, 80, true, false, null, null, null, null);
                DB.AddGagItem("超级口塞", 1000, 120, false, false, null, null, null, null);
            }
            #endregion

#if DEBUG
            var proxy = new HttpToSocks5Proxy("127.0.0.1", 55555);
            var httpClient = new HttpClient(new HttpClientHandler { Proxy = proxy, UseProxy = true });
            Bot = new TelegramBotClient("", httpClient);
#else
            Bot = new TelegramBotClient("");
#endif

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

                Console.Write("Running... 输入1 查看功能列表\n主菜单 > ");

                ConsoleHelper();

                // Send cancellation request to stop bot
#pragma warning disable CS0162 // 检测到无法访问的代码
                cts.Cancel();
#pragma warning restore CS0162 // 检测到无法访问的代码
            }
        }

        #region 控制台操作
        private static readonly string ConsoleCommandList =
            "c 清屏\n" +
            "1 命令列表\n" +
            "2 允许群组控制\n" +
            "3 口塞列表控制\n" +
            "* 对所有已允许群组广播消息\n" +
            "0 退出程序";

        private static readonly string AllowGroupCommandList =
            "c 清屏\n" +
            "1 命令列表\n" +
            "2 输出已允许的群组列表\n" +
            "3 添加允许群组\n" +
            "4 删除允许群组\n" +
            "5 设置已允许的群组功能\n" +
            "9 返回上级";

        private static readonly string GagItemCommandList =
            "c 清屏\n" +
            "1 命令列表\n" +
            "2 输出口塞列表\n" +
            "3 添加口塞\n" +
            "4 删除口塞\n" +
            "5 设置已有口塞参数\n" +
            "9 返回上级";

        private static void ConsoleHelper()
        {
            var command = Console.ReadLine();
            while (command != "0")
            {
                switch (command)
                {
                    case "c": Console.Clear(); break;
                    case "0": Environment.Exit(0); break;
                    case "1": Console.WriteLine(ConsoleCommandList); break;
                    case "2": AllowGroupOperate(); break;
                    case "3": GagItemOperate(); break;
                    case "*": SendChatMsg(); break;
                    default: Console.WriteLine("未知命令, 请输入 1 查看命令列表."); break;
                }
                Console.WriteLine();
                Console.Write("主菜单 > ");
                command = Console.ReadLine();
            }
        }

        private static void SendChatMsg()
        {
            Console.WriteLine("请输入消息(换行符为\\n): ");
            var msg = Console.ReadLine();
            Console.Write("请输入格式类型(1为HTML 2为Markdown 3为Markdown V2): ");
            var ab = Console.ReadLine();
            while (ab != "1" && ab != "2" && ab != "3")
            {
                Console.Write("输入错误!请重新输入(1为HTML 2为Markdown 3为Markdown V2): ");
                ab = Console.ReadLine();
            }
            var psMod = Telegram.Bot.Types.Enums.ParseMode.Html;
            switch (ab)
            {
                case "1": psMod = Telegram.Bot.Types.Enums.ParseMode.Html; break;
                case "2": psMod = Telegram.Bot.Types.Enums.ParseMode.Markdown; break;
                case "3": psMod= Telegram.Bot.Types.Enums.ParseMode.MarkdownV2; break;
            }
            Console.Write("是否静音发送(y/n): ");
            var yn = Console.ReadLine();
            while (yn != "y" && yn != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                yn = Console.ReadLine();
            }
            var groups = DB.GetAllowGroups();
            foreach (var i in groups)
            {
                Bot.SendTextMessageAsync(
                    chatId: i.GroupId,
                    disableNotification: yn == "y" ? true : false,
                    parseMode: psMod,
                    text: msg);
            }
            Console.WriteLine("主菜单 > 广播完成!");
        }

        #region 允许群组控制
        private static void AllowGroupOperate()
        {
            Console.Write("请输入 1 查看功能列表\n主菜单 > 允许群组控制 > ");
            var command = Console.ReadLine();
            while (command != "9")
            {
                switch (command)
                {
                    case "c": Console.Clear(); break;
                    case "1":
                        Console.WriteLine(AllowGroupCommandList);
                        break;
                    case "2":
                        PrintAllowGroup();
                        break;
                    case "3":
                        AddAllowGroup();
                        break;
                    case "4":
                        DelAllowGroup();
                        break;
                    case "5":
                        SetAllowGroup();
                        break;
                    case "9": break;
                    default:
                        Console.WriteLine("未知命令, 请输入 1 查看命令列表.");
                        break;
                }
                Console.WriteLine();
                Console.Write("主菜单 > 允许群组控制 > ");
                command = Console.ReadLine();
            }
        }

        private static void PrintAllowGroup()
        {
            var groups = DB.GetAllowGroups();
            foreach (var i in groups)
            {
                try
                {
                    var result = Bot.GetChatAsync(i.GroupId).Result;
                    Console.WriteLine($"群组Id: {i.GroupId} 群组名称: {result?.Title} @{result?.Username} 允许口塞: {i.AllowGag} 允许验证: {i.AllowVerify} 允许消息计数: {i.AllowMsgCount} 每日计数消息: {i.SamplyMsgCountStr}");
                }
                catch (Exception)
                {
                    DB.DelAllowGroup(i.GroupId);
                    Console.WriteLine($"错误! 找不到群组Id为 {i.GroupId} 群组!已移除该群组!");
                }
            }
        }

        private static void AddAllowGroup()
        {
            Console.Write("请输入群组Id: ");
            var id = Convert.ToInt64(Console.ReadLine());
            if (DB.GetAllowGroupExists(id) == true) { Console.WriteLine($"群组已存在!"); return; }

            DB.AddAllowGroup(id);
            var group = DB.GetAllowGroup(id);

            Console.Write("是否允许该群组使用口塞(输入y/n): ");
            var ab = Console.ReadLine();
            while (ab != "y" && ab != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                ab = Console.ReadLine();
            }
            if (ab == "y") group.AllowGag = true; else group.AllowGag = false;

            Console.Write("是否允许该群组使用验证(输入y/n): ");
            ab = Console.ReadLine();
            while (ab != "y" && ab != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                ab = Console.ReadLine();
            }
            if (ab == "y") group.AllowVerify = true; else group.AllowVerify = false;

            Console.Write("是否允许该群组使用消息计数(输入y/n): ");
            ab = Console.ReadLine();
            while (ab != "y" && ab != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                ab = Console.ReadLine();
            }
            if (ab == "y") group.AllowMsgCount = true; else group.AllowMsgCount = false;

            Console.WriteLine("请输入每日消息计数欢迎语: ");
            group.SamplyMsgCountStr = Console.ReadLine();

            DB.SetAllowGroup(group);

            Console.WriteLine($"主菜单 > 允许群组控制 > 添加成功!");
        }

        private static void DelAllowGroup()
        {
            Console.Write("请输入群组Id: ");
            var id = Convert.ToInt64(Console.ReadLine());
            if (DB.GetAllowGroupExists(id) != true) { Console.WriteLine("群组不存在!"); return; }

            DB.DelAllowGroup(id);
            Console.WriteLine("主菜单 > 允许群组控制 > 删除成功!");
        }

        private static void SetAllowGroup()
        {
            Console.Write("请输入群组Id: ");
            var id = Convert.ToInt64(Console.ReadLine());
            if (DB.GetAllowGroupExists(id) != true) { Console.WriteLine("群组不存在!"); return; }

            var group = DB.GetAllowGroup(id);

            Console.Write("是否允许该群组使用口塞(输入y/n): ");
            var ab = Console.ReadLine();
            while (ab != "y" && ab != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                ab = Console.ReadLine();
            }
            if (ab == "y") group.AllowGag = true; else group.AllowGag = false;

            Console.Write("是否允许该群组使用验证(输入y/n): ");
            ab = Console.ReadLine();
            while (ab != "y" && ab != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                ab = Console.ReadLine();
            }
            if (ab == "y") group.AllowVerify = true; else group.AllowVerify = false;

            Console.Write("是否允许该群组使用消息计数(输入y/n): ");
            ab = Console.ReadLine();
            while (ab != "y" && ab != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                ab = Console.ReadLine();
            }
            if (ab == "y") group.AllowMsgCount = true; else group.AllowMsgCount = false;

            Console.WriteLine("请输入每日消息计数欢迎语: ");
            group.SamplyMsgCountStr = Console.ReadLine();

            DB.SetAllowGroup(group);

            Console.WriteLine($"主菜单 > 允许群组控制 > 设置成功!");
        }
        #endregion

        #region 口塞列表控制
        private static void GagItemOperate()
        {
            Console.Write("请输入 1 查看功能列表\n主菜单 > 口塞列表控制 > ");
            var command = Console.ReadLine();
            while (command != "9")
            {
                switch (command)
                {
                    case "c": Console.Clear(); break;
                    case "1": Console.WriteLine(GagItemCommandList); break;
                    case "2": PrintAllGagItem(); break;
                    case "3": AddGagItem(); break;
                    case "4": DelGagItem(); break;
                    case "5": SetGagItem(); break;
                    case "9": break;
                    default: Console.WriteLine("未知命令, 请输入 1 查看命令列表."); break;
                }
                Console.WriteLine();
                Console.Write("主菜单 > 口塞列表控制 > ");
                command = Console.ReadLine();
            }
        }

        private static void PrintAllGagItem()
        {
            var gags = DB.GetAllGagItems();
            foreach (var i in gags)
            {
                Console.WriteLine($"口塞名字「{i.Name}」 要求绒度「{i.LimitPoint}」 挣扎次数「{i.UnLockCount}」 显示要求绒度「{i.ShowLimit}」 显示解锁次数「{i.ShowUnlock}」");
            }
        }

        private static void AddGagItem()
        {
            Console.Write("请输入口塞名: ");
            var name = Console.ReadLine();
            if (DB.GetGagItemExist(name) == true) { Console.WriteLine("口塞已存在!"); return; }

            Console.Write("请输入口塞要求绒度(Int32数字): ");
            var limitPoint = Convert.ToInt32(Console.ReadLine());
            Console.Write("请输入挣扎次数(Int32数字): ");
            var unlockCount = Convert.ToInt32(Console.ReadLine());
            Console.Write("请输入是否显示要求绒度(y/n): ");
            var showPoint = Console.ReadLine();
            while (showPoint != "y" && showPoint != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                showPoint = Console.ReadLine();
            }
            Console.Write("请输入是否显示挣脱次数(y/n): ");
            var showUnlock = Console.ReadLine();
            while (showUnlock != "y" && showUnlock != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                showUnlock = Console.ReadLine();
            }

            DB.AddGagItem(name, limitPoint, unlockCount, showPoint == "y" ? true : false, showUnlock == "y" ? true : false, null, null, null, null);

            var gag = DB.GetGagItemInfo(name);

            Console.WriteLine("接下来的输入会有些长, 使用回车继续输入, 什么都不输入直接回车将会结束输入, 您可以一开始直接回车以使用默认值");

            Console.WriteLine("请输入自我佩戴口塞时的消息(咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!):");
            var smsg = Console.ReadLine();
            var smsgx = new string[0];
            var smsgl = new StringBuilder();
            while (smsg != "")
            {
                smsgl.AppendLine(smsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                smsg = Console.ReadLine();
            }
            if (smsgl.Length > 0)
            {
                smsg = smsgl.ToString();
                smsgx = smsg.Split("\r\n");
            }

            Console.WriteLine("请输入被他人佩戴口塞时的消息(顺便在 Ta 身上画了一个正字~):");
            var lmsg = Console.ReadLine();
            var lmsgx = new string[0];
            var lmsgl = new StringBuilder();
            while (lmsg != "")
            {
                lmsgl.AppendLine(lmsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                lmsg = Console.ReadLine();
            }
            if (lmsgl.Length > 0)
            {
                lmsg = lmsgl.ToString();
                lmsgx = lmsg.Split("\r\n");
            }

            Console.WriteLine("请输入被他人加固口塞时的消息(修好了口塞!\n顺便展示了钥匙并丢到了一边!):");
            var emsg = Console.ReadLine();
            var emsgx = new string[0];
            var emsgl = new StringBuilder();
            while (emsg != "")
            {
                emsgl.AppendLine(emsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                emsg = Console.ReadLine();
            }
            if (emsgl.Length > 0)
            {
                emsg = emsgl.ToString();
                emsgx = emsg.Split("\r\n");
            }

            Console.WriteLine("请输入挣脱口塞时的消息(Ta感觉自己可以容纳更大的尺寸了呢!):");
            var umsg = Console.ReadLine();
            var umsgx = new string[0];
            var umsgl = new StringBuilder();
            while (umsg != "")
            {
                umsgl.AppendLine(umsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                umsg = Console.ReadLine();
            }
            if (umsgl.Length > 0)
            {
                umsg = umsgl.ToString();
                umsgx = umsg.Split("\r\n");
            }

            if (smsgx.Length > 0) gag.SelfLockMsg = smsgx;
            if (lmsgx.Length > 0) gag.LockMsg = lmsgx;
            if (emsgx.Length > 0) gag.EnhancedLockMsg = emsgx;
            if (umsgx.Length > 0) gag.UnLockMsg = umsgx;

            DB.SetGagItem(gag);

            Console.WriteLine("主菜单 > 口塞列表控制 > 添加成功!");
        }

        private static void DelGagItem()
        {
            Console.Write("请输入口塞名: ");
            var name = Console.ReadLine();
            if (DB.GetGagItemExist(name) != true) { Console.WriteLine("口塞不存在!"); return; }

            DB.DelGagItem(name);

            Console.WriteLine("主菜单 > 口塞列表控制 > 删除成功!");
        }

        private static void SetGagItem()
        {
            Console.Write("请输入口塞名: ");
            var name = Console.ReadLine();
            if (DB.GetGagItemExist(name) != true) { Console.WriteLine("口塞不存在!"); return; }

            var gag = DB.GetGagItemInfo(name);

            Console.Write("请输入口塞要求绒度(Int32数字): ");
            gag.LimitPoint = Convert.ToInt32(Console.ReadLine());
            Console.Write("请输入挣扎次数(Int32数字): ");
            gag.UnLockCount = Convert.ToInt32(Console.ReadLine());
            Console.Write("请输入是否显示要求绒度(y/n): ");
            var showPoint = Console.ReadLine();
            while (showPoint != "y" && showPoint != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                showPoint = Console.ReadLine();
            }
            gag.ShowLimit = showPoint == "y" ? true : false;
            Console.Write("请输入是否显示挣脱次数(y/n): ");
            var showUnlock = Console.ReadLine();
            while (showUnlock != "y" && showUnlock != "n")
            {
                Console.Write("输入错误!请重新输入(输入y/n): ");
                showUnlock = Console.ReadLine();
            }
            gag.ShowUnlock = showUnlock == "y" ? true : false;

            Console.WriteLine("接下来的输入会有些长, 使用回车继续输入, 什么都不输入直接回车将会结束输入, 您可以一开始直接回车以使用默认值");

            Console.WriteLine("请输入自我佩戴口塞时的消息(咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!):");
            var smsg = Console.ReadLine();
            var smsgx = new string[0];
            var smsgl = new StringBuilder();
            while (smsg != "")
            {
                smsgl.AppendLine(smsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                smsg = Console.ReadLine();
            }
            if (smsgl.Length > 0)
            {
                smsg = smsgl.ToString();
                smsgx = smsg.Split("\r\n");
            }

            Console.WriteLine("请输入被他人佩戴口塞时的消息(顺便在 Ta 身上画了一个正字~):");
            var lmsg = Console.ReadLine();
            var lmsgx = new string[0];
            var lmsgl = new StringBuilder();
            while (lmsg != "")
            {
                lmsgl.AppendLine(lmsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                lmsg = Console.ReadLine();
            }
            if (lmsgl.Length > 0)
            {
                lmsg = lmsgl.ToString();
                lmsgx = lmsg.Split("\r\n");
            }

            Console.WriteLine("请输入被他人加固口塞时的消息(修好了口塞!\\n顺便展示了钥匙并丢到了一边!):");
            var emsg = Console.ReadLine();
            var emsgx = new string[0];
            var emsgl = new StringBuilder();
            while (emsg != "")
            {
                emsgl.AppendLine(emsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                emsg = Console.ReadLine();
            }
            if (emsgl.Length > 0)
            {
                emsg = emsgl.ToString();
                emsgx = emsg.Split("\r\n");
            }

            Console.WriteLine("请输入挣脱口塞时的消息(Ta感觉自己可以容纳更大的尺寸了呢!):");
            var umsg = Console.ReadLine();
            var umsgx = new string[0];
            var umsgl = new StringBuilder();
            while (umsg != "")
            {
                umsgl.AppendLine(umsg);
                Console.WriteLine("请继续输入或输入回车结束:");
                umsg = Console.ReadLine();
            }
            if (umsgl.Length > 0)
            {
                umsg = umsgl.ToString();
                umsgx = umsg.Split("\r\n");
            }

            if (smsgx.Length > 0) gag.SelfLockMsg = smsgx;
            if (lmsgx.Length > 0) gag.LockMsg = lmsgx;
            if (emsgx.Length > 0) gag.EnhancedLockMsg = emsgx;
            if (umsgx.Length > 0) gag.UnLockMsg = umsgx;

            DB.SetGagItem(gag);

            Console.WriteLine("主菜单 > 口塞列表控制 > 设置成功!");
        }
        #endregion

        #endregion

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
