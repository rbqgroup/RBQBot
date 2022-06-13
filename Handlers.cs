#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
using RBQBot.Model;
using System;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
//using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace RBQBot
{
    public class Handlers
    {
        /// <summary>[绒布球] 给自己带上了默认口塞! 咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!</summary>
        public static readonly string[] defaultSelfLockMsg = { "咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!" };

        /// <summary>[Ta] 帮 [绒布球] 带上了默认口塞! 顺便在 Ta 身上画了一个正字~</summary>
        public static readonly string[] defaultLockMsg = { "顺便在 Ta 身上画了一个正字~" };

        /// <summary>[Ta] 帮 [绒布球] 修好了口塞!\n顺便展示了钥匙并丢到了一边!</summary>
        public static readonly string[] defaultEnhancedLockMsg = { "修好了口塞!\n顺便展示了钥匙并丢到了一边!" };

        /// <summary>[绒布球] 挣脱了被人们安装的 超大号默认口塞! Ta感觉自己可以容纳更大的尺寸了呢!</summary>
        public static readonly string[] defaultUnlockMsg = { "Ta感觉自己可以容纳更大的尺寸了呢!" };

        #region 管理员命令封装
        private static void AdminProcess(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 1)
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: Program.AdminTxt);
            }
            else
            {
                if (message.Chat.Type == ChatType.Private)
                {
                    switch (comm[1])
                    {
                        case "GetAllowGroup": GetAllowGroup(botClient, message); break;
                        case "AddAllowGroup": AddAllowGroup(botClient, message); break;
                        case "DelAllowGroup": DelAllowGroup(botClient, message); break;
                        case "SetAllowGroup": SetAllowGroup(botClient, message); break;
                        case "AddGagItem": AddGagItem(botClient, message); break;
                        case "DelGagItem": DelGagItem(botClient, message); break;
                        case "SetGagItem": SetGagItem(botClient, message); break;
                        case "AddRBQPoint": AddRBQPoint(botClient, message); break;
                        case "DelRBQPoint": DelRBQPoint(botClient, message); break;
                        case "SetRBQPoint": SetRBQPoint(botClient, message); break;
                        case "AddRBQLockCount":
                        case "DelRBQLockCount":
                        case "ClearLockCount":
                        case "SetGag":
                        case "ClearGagFrom":
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "该命令仅限群组内使用!");
                            break;
                        default:
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "未知命令, 请输入 /admin 查看命令!");
                            break;
                    }
                }
                else
                {
                    switch (comm[1])
                    {
                        case "GetAllowGroup":
                        case "AddAllowGroup":
                        case "DelAllowGroup":
                        case "SetAllowGroup":
                        case "AddGagItem":
                        case "DelGagItem":
                        case "SetGagItem":
                        case "AddRBQPoint":
                        case "DelRBQPoint":
                        case "SetRBQPoint":
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "该命令仅限私聊使用!");
                            break;
                        case "AddRBQLockCount":
                            if (message.ReplyToMessage != null && message.ReplyToMessage?.From.IsBot != true) AddRBQLockCount(botClient, message);
                            else botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "请回复用户的消息使用本命令!");
                            break;
                        case "DelRBQLockCount":
                            if (message.ReplyToMessage != null && message.ReplyToMessage?.From.IsBot != true) DelRBQLockCount(botClient, message);
                            else botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "请回复用户的消息使用本命令!");
                            break;
                        case "ClearLockCount":
                            if (message.ReplyToMessage != null && message.ReplyToMessage?.From.IsBot != true) ClearLockCount(botClient, message);
                            else botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "请回复用户的消息使用本命令!");
                            break;
                        case "SetGag":
                            if (message.ReplyToMessage != null && message.ReplyToMessage?.From.IsBot != true) SetGag(botClient, message);
                            else botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "请回复用户的消息使用本命令!");
                            break;
                        case "ClearGagFrom":
                            if (message.ReplyToMessage != null && message.ReplyToMessage?.From.IsBot != true) ClearGagFrom(botClient, message);
                            else botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "请回复用户的消息使用本命令!");
                            break;
                        default:
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "未知命令, 请输入 /admin 查看命令!");
                            break;
                    }
                }
            }
        }

        #region 超管命令封装
        private static void GetAllowGroup(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 3)
            {
                if (int.TryParse(comm[2], out int page)==true)
                {
                    var index = 0;
                    var sb = new StringBuilder();

                    var groups = Program.DB.GetAllowGroups();
                    foreach (var i in groups)
                    {
                        try
                        {
                            var result = botClient.GetChatAsync(i.GroupId).Result;
                            index++;
                            sb.AppendLine($"群组Id: <code>{i.GroupId}</code> 群组名称: {result?.Title} @{result?.Username} 允许口塞: {i.AllowGag} 允许验证: {i.AllowVerify} 允许消息计数: {i.AllowMsgCount} 每日计数消息: {i.SamplyMsgCountStr}");
                            if (index >= page)
                            {
                                index = 0;
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    replyToMessageId: message.MessageId,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: sb.ToString());
                                sb.Clear();
                            }
                        }
                        catch (Exception)
                        {
                            Program.DB.DelAllowGroup(i.GroupId);
                            Console.WriteLine($"错误! 找不到群组Id为 {i.GroupId} 群组!已移除该群组!");
                        }
                    }
                    botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    replyToMessageId: message.MessageId,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: sb.ToString());
                }
                else
                {
                    botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数错误! 请重新输入!");
                }
            }
            else
            {
                var index = 0;
                var sb = new StringBuilder();

                var groups = Program.DB.GetAllowGroups();
                foreach (var i in groups)
                {
                    try
                    {
                        var result = botClient.GetChatAsync(i.GroupId).Result;
                        index++;
                        sb.AppendLine($"群组Id: <code>{i.GroupId}</code> 群组名称: {result?.Title} @{result?.Username} 允许口塞: {i.AllowGag} 允许验证: {i.AllowVerify} 允许消息计数: {i.AllowMsgCount} 每日计数消息: {i.SamplyMsgCountStr}");
                        if (index >= 10)
                        {
                            index = 0;
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: sb.ToString());
                            sb.Clear();
                        }
                    }
                    catch (Exception)
                    {
                        Program.DB.DelAllowGroup(i.GroupId);
                        Console.WriteLine($"错误! 找不到群组Id为 {i.GroupId} 群组!已移除该群组!");
                    }
                }
                botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: sb.ToString());
            }
        }

        private static void AddAllowGroup(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 3)
            {
                if (long.TryParse(comm[2], out long id) == true)
                {
                    if (Program.DB.GetAllowGroupExists(id) != true)
                    {
                        Program.DB.AddAllowGroup(id);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "添加成功!");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "群组已存在!");
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "参数错误! 请重新输入!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数错误! 请重新输入!");
            }
        }

        private static void DelAllowGroup(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 3)
            {
                if (long.TryParse(comm[2], out long id) == true)
                {
                    if (Program.DB.GetAllowGroupExists(id) != true)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "群组不存在!");
                    }
                    else
                    {
                        Program.DB.DelAllowGroup(id);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "删除成功!");
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "参数错误! 请重新输入!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数错误! 请重新输入!");
            }
        }

        private static void SetAllowGroup(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 7)
            {
                if (long.TryParse(comm[2], out long id)==true&&bool.TryParse(comm[3], out bool gag)==true&&bool.TryParse(comm[4], out bool verify)==true&&bool.TryParse(comm[5], out bool msg)==true)
                {
                    if (Program.DB.GetAllowGroupExists(id) == true)
                    {
                        var group = Program.DB.GetAllowGroup(id);
                        group.AllowGag = gag;
                        group.AllowVerify = verify;
                        group.AllowMsgCount = msg;
                        group.SamplyMsgCountStr = comm[6];
                        Program.DB.SetAllowGroup(group);

                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "修改成功!");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "群组不存在!");
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数错误! 请重新输入!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数不够! 请重新输入!");
            }
        }

        private static void AddGagItem(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 3)
            {
                if (Program.DB.GetGagItemExist(comm[2]) != true)
                {
                    Program.DB.AddGagItem(comm[2], 1, 1, false, false, null, null, null, null);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "添加成功!");
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "口塞已存在!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数错误! 请重新输入!");
            } 
        }

        private static void DelGagItem(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 3)
            {
                if (Program.DB.GetGagItemExist(comm[2]) == true)
                {
                    Program.DB.DelGagItem(comm[2]);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "删除成功!");
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "口塞不存在!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数错误! 请重新输入!");
            }
        }

        private static void SetGagItem(ITelegramBotClient botClient, Message message)
        {
            var head = message.Text.IndexOf("SetGagItem") + 11;
            var json = message.Text.Substring(head);

            try
            {
                var obj = JsonSerializer.Deserialize<GagItem>(json, new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString | System.Text.Json.Serialization.JsonNumberHandling.WriteAsString });
                if (Program.DB.GetGagItemExist(obj.Name) != true)
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "口塞不存在!");
                }
                else
                {
                    var gag = Program.DB.GetGagItemInfo(obj.Name);
                    gag.LimitPoint = obj.LimitPoint;
                    gag.UnLockCount = obj.UnLockCount;
                    gag.SelfLockMsg = obj.SelfLockMsg == null ? defaultSelfLockMsg : obj.SelfLockMsg;
                    gag.LockMsg = obj.LockMsg == null ? defaultLockMsg : obj.LockMsg;
                    gag.EnhancedLockMsg = obj.EnhancedLockMsg == null ? defaultEnhancedLockMsg : obj.EnhancedLockMsg;
                    gag.UnLockMsg = obj.UnLockMsg == null ? defaultUnlockMsg : obj.UnLockMsg;
                    gag.ShowLimit = obj.ShowLimit;
                    gag.ShowUnlock = obj.ShowUnlock;
                    Program.DB.SetGagItem(gag);
                    
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "修改成功!");
                }
            }
            catch (Exception ex)
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "Json参数错误! 请重新输入!");
            }
        }

        private static void AddRBQPoint(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 4)
            {
                if (long.TryParse(comm[2], out long uid) == true && int.TryParse(comm[3], out int point) == true)
                {
                    if (Program.DB.GetRBQExist(uid) != true)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "用户不存在!");
                    }
                    else
                    {
                        var user = Program.DB.GetRBQInfo(uid);
                        user.RBQPoint += point;
                        Program.DB.SetRBQInfo(user);

                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "已增加绒度!");
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "请输入正确的参数!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数不足! 请重新输入!");
            }
        }

        private static void DelRBQPoint(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 4)
            {
                if (long.TryParse(comm[2], out long uid) == true && int.TryParse(comm[3], out int point) == true)
                {
                    if (Program.DB.GetRBQExist(uid) != true)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "用户不存在!");
                    }
                    else
                    {
                        var user = Program.DB.GetRBQInfo(uid);
                        user.RBQPoint -= point;
                        Program.DB.SetRBQInfo(user);

                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "已减少绒度!");
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "请输入正确的参数!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数不足! 请重新输入!");
            }
        }

        private static void SetRBQPoint(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length == 4)
            {
                if (long.TryParse(comm[2], out long uid) == true && int.TryParse(comm[3], out int point) == true)
                {
                    if (Program.DB.GetRBQExist(uid) != true)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "用户不存在!");
                    }
                    else
                    {
                        var user = Program.DB.GetRBQInfo(uid);
                        user.RBQPoint = point;
                        Program.DB.SetRBQInfo(user);

                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "已设置绒度!");
                    }
                }
                else
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "请输入正确的参数!");
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    text: "参数不足! 请重新输入!");
            }
        }
        #endregion

        #region 群管命令功能封装
        private static void AddRBQLockCount(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length < 3)
            {
                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图增加 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但管理员忘了输入数了!");
            }
            else
            {
                if (int.TryParse(comm[2], out int count) != false)
                {
                    var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);

                    if (rbq != null)
                    {
                        rbq.LockCount += count;
                        Program.DB.SetRBQStatus(rbq);

                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已增加 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数!");
                    }
                    else
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图增加 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但找不到该用户的状态!");
                    }
                }
                else
                {
                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图增加 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但管理员输错数了!");
                }
            }
        }

        private static void DelRBQLockCount(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length < 3)
            {
                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图减少 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但管理员忘了输入数了!");
            }
            else
            {
                if (int.TryParse(comm[2], out int count) != false)
                {
                    var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);

                    if (rbq != null)
                    {
                        if (rbq.LockCount-count <= 0)
                        {
                            if (Program.List.TryRemove(rbq.Id, out RBQList rbql) == true) rbql.Stop();
                            rbq.LockCount = 0;
                            Program.DB.SetRBQStatus(rbq);
                        }
                        else
                        {
                            rbq.LockCount -= count;
                            Program.DB.SetRBQStatus(rbq);
                        }

                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已减少 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数!");
                    }
                    else
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图减少 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但找不到该用户的状态!");
                    }
                }
                else
                {
                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图减少 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但管理员输错数了!");
                }
            }
        }

        private static void ClearLockCount(ITelegramBotClient botClient, Message message)
        {
            var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
            if (rbq != null)
            {
                if (Program.List.TryRemove(rbq.Id, out RBQList rbql) != false) rbql.Stop();

                rbq.LockCount = 0;
                Program.DB.SetRBQStatus(rbq);

                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已清除 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数!");
            }
            else
            {
                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图清除 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的挣脱次数, 但找不到该用户的状态!");
            }
        }

        private static void SetGag(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.Split(' ');
            if (comm.Length < 3)
            {
                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图设置 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的口塞, 但口塞不存在!");
            }
            else
            {
                if (Program.DB.GetGagItemExist(comm[2]) != true)
                {
                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图设置 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的口塞, 但口塞不存在!");
                }
                else
                {
                    var gag = Program.DB.GetGagItemInfo(comm[2]);
                    var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                    if (rbq != null)
                    {
                        var tm = DateTime.UtcNow.AddHours(8);

                        rbq.FromId = new long[] { rbq.RBQId };
                        rbq.StartLockTime = tm.Ticks;
                        rbq.LockCount = gag.UnLockCount;
                        rbq.GagId = gag.Id;
                        Program.DB.SetRBQStatus(rbq);

                        #region 内存队列存在检查
                        if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                        {
                            var timeout = (tm.AddMinutes(Program.LockTime) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                            var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                            Program.List.TryAdd(rbq.Id, rbqx);
                        }
                        else
                        {
                            rbqItem.Stop();
                            rbqItem.Id = rbq.Id;
                            rbqItem.GagId = rbq.GagId;
                            rbqItem.StartLockTime = rbq.StartLockTime;
                            rbqItem.LockCount = rbq.LockCount;
                            rbqItem.ResetTimer();
                            Program.List.TryAdd(rbq.Id, rbqItem);
                        }
                        #endregion

                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已将 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的口塞设置为 {gag.Name}!");
                    }
                    else
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode : ParseMode.Html,
                            text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图设置 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的口塞, 但找不到该用户的状态!");
                    }
                }
            }
        }

        private static void ClearGagFrom(ITelegramBotClient botClient, Message message)
        {
            var rbq =Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
            if (rbq != null)
            {
                rbq.FromId = new long[0];
                Program.DB.SetRBQStatus(rbq);

                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已清除 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的所有口塞添加/加固者!");
            }
            else
            {
                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图清除 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的所有口塞添加/加固者, 但找不到该用户的状态!");
            }
        }
        #endregion

        #endregion

        #region 通用功能
        public static void MessageCountProcess(Message message)
        {
            if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowMsgCount) == true)
            {
                if (Program.DB.GetMessageCountUserExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddMessageCountUser(message.Chat.Id, message.From.Id);
                Program.DB.AddMessageCountUserCount(message.Chat.Id, message.From.Id);
            }
        }


        public static void DelayDeleteMessage(ITelegramBotClient botClient, long chatId, int msgId, int delayms)
        {
            ThreadPool.QueueUserWorkItem( o => {
                Thread.Sleep(delayms);
                botClient.DeleteMessageAsync(chatId, msgId);
            });
        }

        /// <summary>消息合规检查</summary>
        /// <param name="msg">输入消息</param>
        /// <returns>合规性,大于0为不合规,等于0合规</returns>
        public static int TypeProcess(string msg)
        {
            if (msg.IndexOfAny(new char[] { '呜', '哈', '啊', '唔', '嗯', '呃', '哦', '嗷', '呕', '噢', '喔' }) < 0) return 1;
            msg = msg.Replace('呜', ' ');
            msg = msg.Replace('哈', ' ');
            msg = msg.Replace('啊', ' ');
            msg = msg.Replace('唔', ' ');
            msg = msg.Replace('嗯', ' ');
            msg = msg.Replace('呃', ' ');
            msg = msg.Replace('哦', ' ');
            msg = msg.Replace('嗷', ' ');
            msg = msg.Replace('呕', ' ');
            msg = msg.Replace('噢', ' ');
            msg = msg.Replace('喔', ' ');
            msg = msg.Replace('…', ' ');
            msg = msg.Replace('!', ' ');
            msg = msg.Replace('?', ' ');
            msg = msg.Replace(',', ' ');
            msg = msg.Replace('！', ' ');
            msg = msg.Replace('？', ' ');
            msg = msg.Replace('，', ' ');
            msg = msg.Replace('.', ' ');
            msg = System.Text.RegularExpressions.Regex.Replace(msg, @"\s", "");
            msg = msg.Trim();
            return msg.Length;
        }

        /// <summary>检查是否为管理员</summary>
        /// <returns>是管理 = true 不是管理 = false</returns>
        public static bool CheckIsAdmin(ITelegramBotClient botClient, long groupId, long userId)
        {
            var result = botClient.GetChatAdministratorsAsync(groupId).Result;
            if (result.Length > 0)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i].User.Id == userId) return true;
                }
            }
            return false;
        }
        #endregion

        #region 内联命令功能封装
        public static string GetRBQSay()
        {
            string[] charta = {
                "呜",
                "哈",
                "啊", "啊", "啊", "啊", "啊",
                "啊啊", "啊啊", "啊啊", "啊啊",
                "啊啊啊", "啊啊啊", "啊啊啊",
                "唔",
                "嗯", "嗯", "嗯", "嗯",
                "嗯嗯", "嗯嗯",
                "呃",
                "呃呃",
                "哦",
                "哦哦",
                "嗷",
                "嗷嗷",
                "呕",
                "噢",
                "喔",
                "喔喔",

                "唔嗯", "唔嗯",
                "唔啊",
            };
            string[] chartb = {
                ".", ".", ".",
                "..", "..",
                "…", //"…", "…", "…", "…", "…",
                //"……", "……", "……", "……",
                //"………", "………",
                "！", "!",
                "！！", "!!",
                "？", "?",
                "？？", "??",
                "！？", "!?",
                "？！", "?!",
                "，", "，", "，",
                "，，", "，，",
            };

            var R = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < R.Next(3, 7); i++)
            {
                sb.Append(charta[R.Next(0, charta.Length)]);
                if (i % 2 == 1)
                {
                    sb.Append(chartb[R.Next(0, chartb.Length)]);
                }
            }
            return sb.ToString();
        }

        public static string GetRBQPoint(long telegramId)
        {
            if (Program.DB.GetRBQExist(telegramId) != true) Program.DB.AddRBQ(telegramId, 0);

            var rbq = Program.DB.GetRBQInfo(telegramId);
            return $"我的绒度有 {rbq.RBQPoint} 点, 感觉自己绒绒哒.";
        }
        #endregion

        #region 通用命令功能封装
        public static string GetAllGag()
        {
            var count = Program.DB.GetGagItemCount();
            var gag = Program.DB.GetAllGagItems();
            var sb = new StringBuilder();
            sb.AppendLine($"一共有 {count} 个口塞. (点击<b><u>名字</u></b>即可复制)");
            foreach (var i in gag)
            {
                var limit = i.ShowLimit ? i.LimitPoint.ToString() : "未知";
                var unlock = i.ShowUnlock ? i.UnLockCount.ToString() : "未知";

                sb.AppendLine($"<code>{i.Name}</code> 要求绒度 「{limit}」 挣脱次数 「{unlock}」");
            }
            return sb.ToString();
        }

        private static async void PingProcess(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine();
            Console.WriteLine($"Pong!\nFrom : {message.Chat.Id} ChatTitle: {message.Chat?.Title} ChatName: {message.Chat?.Username} ChatNick: {message.Chat?.FirstName} {message.Chat?.LastName}");
            #region 通用获取 CPU/MEM 使用率
            Task<CpuMemStruct> proc = new Task<CpuMemStruct>(() =>
            {
                var p = System.Diagnostics.Process.GetProcesses();

                TimeSpan startCpuUsage;
                TimeSpan stopCpuUsage;
                DateTime startTm;
                DateTime stopTm;

                double cpuUsedMs;
                double totalMsPassed;
                double cpuUsageTotal;

                double allCpuTm = 0;
                double allUsedMem = 0;

                for (int i = 0; i < p.Length; i++)
                {
                    try
                    {
                        allUsedMem += p[i].WorkingSet64 / 1024 / 1024;

                        startTm = DateTime.UtcNow;
                        startCpuUsage = p[i].TotalProcessorTime;

                        Thread.Sleep(15);

                        stopTm = DateTime.UtcNow;
                        stopCpuUsage = p[i].TotalProcessorTime;

                        cpuUsedMs = (stopCpuUsage - startCpuUsage).TotalMilliseconds;
                        totalMsPassed = (stopTm - startTm).TotalMilliseconds;
                        cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                        allCpuTm += cpuUsageTotal * 100;
                    }
                    catch (Exception ex) { Console.WriteLine($"获取 CPU/MEM 使用率时错误!{Environment.NewLine}错误信息: {Environment.NewLine}{ex.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}"); }
                }

                return new CpuMemStruct() { UsedCpu = allCpuTm, UsedMem = allUsedMem };
            });
            proc.Start();
            #endregion

            double allMem = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
            double diskSize = 0;

            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (var i in allDrives)
                {
                    diskSize += (i.AvailableFreeSpace / 1024 / 1024);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取剩余可用空间失败!{Environment.NewLine}错误信息: {Environment.NewLine}{ex.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");
            }

            proc.Wait();

            var allow = "";
            if (message.Chat.Type == ChatType.Private) allow = "您拥有管理权限";
            else
            {
                if (Program.DB.GetAllowGroupExists(message.Chat.Id) == true) allow = "具有使用许可权";
                else allow = "不具有使用许可权";
            }

            botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId,
                disableNotification: true,
                parseMode: ParseMode.Html,
                text: $"Pong!\n电子绒布球 v{Program.Version}\n服务器当前时间: {DateTime.UtcNow.AddHours(8)}\n距离上次重启: {new TimeSpan(DateTime.UtcNow.AddHours(8).Ticks-Program.StartTime.Ticks)}\n可用磁盘: {diskSize} MB\n可用内存: {(allMem - proc.Result.UsedMem).ToString("0.00")}MB\nCPU使用率: {proc.Result.UsedCpu.ToString("0.00")}%\n当前会话 「{message.Chat.Title}」 {allow}\n有关更多信息请参阅 「<code>/about</code>」\n{"本Bot具有超级绒力",20}");
            Console.WriteLine();
        }

        private static void Help(ITelegramBotClient botClient, Message message)
        {
            botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId,
                disableNotification: true,
                parseMode: ParseMode.Html,
                text: Program.HelpTxt);
        }

        private static void About(ITelegramBotClient botClient, Message message)
        {
            botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId,
                disableNotification: true,
                parseMode: ParseMode.Html,
                text: Program.AboutTxt);
        }
        #endregion

        #region 私聊Bot命令功能封装

        private static void GetRBQPoint(ITelegramBotClient botClient, Message message)
        {
            if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0);
            if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

            var point = Program.DB.GetRBQPoint(message.From.Id);

            botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                replyToMessageId: message.MessageId,
                disableNotification: true,
                parseMode: ParseMode.Html,
                text: $"您的绒度为 {point}");
        }
        #endregion

        #region 群内命令功能封装
        private static void CountProcess(ITelegramBotClient botClient, Message message)
        {
            var lockCount = Program.DB.GetRBQLockCount(message.Chat.Id, message.From.Id);
            var msg = $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 还需要挣扎 {lockCount} 次.";
            if (lockCount == 0) msg = $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 好像想念口塞了.";

            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
            botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                disableNotification: true,
                parseMode: ParseMode.Html,
                text: msg);
        }

        private static void GagProcess(ITelegramBotClient botClient, Message message, string[] comm)
        {
            if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0);
            if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

            #region 对别人回复命令
            if (message.ReplyToMessage != null && message.From.Id != message.ReplyToMessage.From.Id)
            {
                if (message.ReplyToMessage.From.IsBot == true)
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: $"机器人永不为绒!");
                }
                else
                {
                    if (Program.DB.GetRBQExist(message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQ(message.ReplyToMessage.From.Id, 0);
                    if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id, 0, false);

                    #region 如果命令长度 > 1 是有开关/给别人加口塞
                    if (comm.Length > 1)
                    {
                        #region 如果是任意使用开关
                        if (comm[1] == "on" || comm[1] == "off")
                        {
                            if (CheckIsAdmin(botClient, message.Chat.Id, message.From.Id) != true)
                            {
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                /*var result = */
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图修改 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的开关! 但Ta没有权限!")/*.Result*/;

                                //ThreadPool.QueueUserWorkItem(o => { DelayDeleteMessage(botClient, message.Chat.Id, result.MessageId, 5000); });
                            }
                            else
                            {
                                if (comm[1] == "on")
                                {
                                    Program.DB.SetRBQUsed(message.Chat.Id, message.ReplyToMessage.From.Id, true);
                                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                    botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        disableNotification: true,
                                        parseMode: ParseMode.Html,
                                        text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已强制让 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 接受被人公共调教!");
                                }
                                else
                                {
                                    Program.DB.SetRBQUsed(message.Chat.Id, message.ReplyToMessage.From.Id, false);
                                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                    botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        disableNotification: true,
                                        parseMode: ParseMode.Html,
                                        text: $"管理员 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已强制让 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 拒绝被人公共调教!");
                                }
                            }
                        }
                        #endregion
                        #region 否则就是加口塞
                        else
                        {
                            if (Program.DB.GetGagItemExist(comm[1]) != true)
                            {
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: $"口塞 {comm[1]} 不存在!");
                            }
                            else
                            {
                                #region 检查是否允许被他人加口塞 和 管理员身份检查
                                if (Program.DB.GetRBQAnyUsed(message.Chat.Id, message.ReplyToMessage.From.Id) != true && CheckIsAdmin(botClient, message.Chat.Id, message.From.Id) != true)
                                {
                                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                    botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        disableNotification: true,
                                        parseMode: ParseMode.Html,
                                        text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图给 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 使用 <code>{comm[1]}</code> ,但对方不是公用型绒布球!");
                                }
                                #endregion
                                else
                                {
                                    #region 检查是否重复加口塞
                                    if (Program.DB.GetRBQFromExits(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id) == true)
                                    {
                                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                        botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            disableNotification: true,
                                            parseMode: ParseMode.Html,
                                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图给 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 使用 <code>{comm[1]}</code>,\n但 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已经添加/加固过 <a href=\"tg://user?id={message.ReplyToMessage?.From.Id}\">{message.ReplyToMessage?.From.FirstName} {message.ReplyToMessage?.From.LastName}</a> 的口塞了!");
                                    }
                                    #endregion
                                    else
                                    {
                                        #region 检查是否已经上了一个口塞了
                                        if (Program.DB.GetRBQLockCount(message.Chat.Id, message.ReplyToMessage.From.Id) > 0)
                                        {
                                            var gagId = Program.DB.GetRBQGagId(message.Chat.Id, message.ReplyToMessage.From.Id);
                                            if (gagId > 0)
                                            {
                                                var gag = Program.DB.GetGagItemInfo(gagId);

                                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                botClient.SendTextMessageAsync(
                                                    chatId: message.Chat.Id,
                                                    disableNotification: true,
                                                    parseMode: ParseMode.Html,
                                                    text: $"<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 已经有一个 <code>{gag.Name}</code> 了!\n<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 可以使用/gag 回复这只绒布球来加固.");
                                            }
                                            else
                                            {
                                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                botClient.SendTextMessageAsync(
                                                    chatId: message.Chat.Id,
                                                    disableNotification: true,
                                                    parseMode: ParseMode.Html,
                                                    text: $"获取口塞失败,这可能是因为最近修改了数据库导致无法找到这个口塞.");
                                            }
                                        }
                                        #endregion
                                        else
                                        {
                                            var gag = Program.DB.GetGagItemInfo(comm[1]);

                                            #region 不是管理员上口塞
                                            if (CheckIsAdmin(botClient, message.Chat.Id, message.From.Id) != true)
                                            {
                                                if (Program.DB.GetRBQPoint(message.ReplyToMessage.From.Id) < gag.LimitPoint)
                                                {
                                                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                    botClient.SendTextMessageAsync(
                                                        chatId: message.Chat.Id,
                                                        disableNotification: true,
                                                        parseMode: ParseMode.Html,
                                                        text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图给 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 使用 <code>{gag.Name}</code>, 但绒布球的绒度不够!");
                                                }
                                                else
                                                {
                                                    if (Program.DB.GetRBQCanLock(message.Chat.Id, message.ReplyToMessage.From.Id) != true)
                                                    {
                                                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                        botClient.SendTextMessageAsync(
                                                            chatId: message.Chat.Id,
                                                            disableNotification: true,
                                                            parseMode: ParseMode.Html,
                                                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图给 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 使用 <code>{gag.Name}</code>, 但绒布球刚挣脱没多久!");
                                                    }
                                                    else
                                                    {
                                                        Program.DB.AddRBQFroms(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id);

                                                        var R = new Random();
                                                        var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                                                        var tm = DateTime.UtcNow.AddHours(8);

                                                        rbq.StartLockTime = tm.Ticks;
                                                        rbq.LockCount = gag.UnLockCount;
                                                        rbq.GagId = gag.Id;

                                                        Program.DB.SetRBQStatus(rbq);

                                                        var msg = $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 帮 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 戴上了 <code>{comm[1]}</code>!\n{gag.LockMsg[R.Next(0, gag.LockMsg.Length)]}";

                                                        #region 内存队列存在检查
                                                        if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                                                        {
                                                            var timeout = (tm.AddMinutes(Program.LockTime) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                                            var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                                                            Program.List.TryAdd(rbq.Id, rbqx);
                                                        }
                                                        else
                                                        {
                                                            rbqItem.Stop();
                                                            rbqItem.Id = rbq.Id;
                                                            rbqItem.GagId = gag.Id;
                                                            rbqItem.StartLockTime = tm.Ticks;
                                                            rbqItem.LockCount = gag.UnLockCount;
                                                            rbqItem.ResetTimer();
                                                            Program.List.TryAdd(rbq.Id, rbqItem);
                                                        }
                                                        #endregion

                                                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                        botClient.SendTextMessageAsync(
                                                            chatId: message.Chat.Id,
                                                            disableNotification: true,
                                                            parseMode: ParseMode.Html,
                                                            text: msg);
                                                    }
                                                }
                                            }
                                            #endregion
                                            #region 是管理员上口塞
                                            else
                                            {
                                                Program.DB.AddRBQFroms(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id);

                                                var R = new Random();
                                                var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                                                var tm = DateTime.UtcNow.AddHours(8);

                                                rbq.StartLockTime = tm.Ticks;
                                                rbq.LockCount = gag.UnLockCount;
                                                rbq.GagId = gag.Id;
                                                Program.DB.SetRBQStatus(rbq);

                                                var msg = $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 帮 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 戴上了 {comm[1]}!\n{gag.LockMsg[R.Next(0, gag.LockMsg.Length)]}";

                                                #region 内存队列存在检查
                                                if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                                                {
                                                    var timeout = (tm.AddMinutes(Program.LockTime) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                                    var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                                                    Program.List.TryAdd(rbq.Id, rbqx);
                                                }
                                                else
                                                {
                                                    rbqItem.Stop();
                                                    rbqItem.Id = rbq.Id;
                                                    rbqItem.GagId = gag.Id;
                                                    rbqItem.StartLockTime = tm.Ticks;
                                                    rbqItem.LockCount = gag.UnLockCount;
                                                    rbqItem.ResetTimer();
                                                    Program.List.TryAdd(rbq.Id, rbqItem);
                                                }
                                                #endregion

                                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                botClient.SendTextMessageAsync(
                                                    chatId: message.Chat.Id,
                                                    disableNotification: true,
                                                    parseMode: ParseMode.Html,
                                                    text: msg);
                                            }
                                            #endregion
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                    #region 命令长度 = 1 是给别人加固
                    else
                    {
                        #region 重复加固检查
                        if (Program.DB.GetRBQFromExits(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id) == true)
                        {
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已经给 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 添加/加固过口塞了,就放过Ta吧.");
                        }
                        #endregion
                        else
                        {
                            Program.DB.AddRBQFroms(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id);

                            var R = new Random();
                            var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                            var gag = Program.DB.GetGagItemInfo(rbq.GagId);

                            if (gag != null)
                            {
                                var tm = DateTime.UtcNow.AddHours(8);

                                rbq.StartLockTime = tm.Ticks;
                                rbq.LockCount = gag.UnLockCount;
                                rbq.GagId = gag.Id;
                                Program.DB.SetRBQStatus(rbq);

                                #region 内存队列存在检查
                                if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                                {
                                    var timeout = (tm.AddMinutes(Program.LockTime) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                    var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                                    Program.List.TryAdd(rbq.Id, rbqx);
                                }
                                else
                                {
                                    rbqItem.Stop();
                                    rbqItem.Id = rbq.Id;
                                    rbqItem.GagId = gag.Id;
                                    rbqItem.StartLockTime = tm.Ticks;
                                    rbqItem.LockCount = gag.UnLockCount;
                                    rbqItem.ResetTimer();
                                    Program.List.TryAdd(rbq.Id, rbqItem);
                                }
                                #endregion

                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 帮 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> {gag.EnhancedLockMsg[R.Next(0, gag.EnhancedLockMsg.Length)]}");
                            }
                            else
                            {
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: "要加固的口塞不存在!");
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region 自我开关与口塞
            else
            {
                if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0);
                if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

                if (comm.Length > 1)
                {
                    if (comm[1] == "on")
                    {
                        Program.DB.SetRBQUsed(message.Chat.Id, message.From.Id, true);

                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 想要被公开调教了!");
                        return;
                    }
                    if (comm[1] == "off")
                    {
                        Program.DB.SetRBQUsed(message.Chat.Id, message.From.Id, false);

                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 不愿意被公开调教了!");
                        return;
                    }

                    if (Program.DB.GetGagItemExist(comm[1]) != true)
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"{comm[1]} 不存在!");
                    }
                    else
                    {
                        if (Program.DB.GetRBQLockCount(message.Chat.Id, message.From.Id) > 0)
                        {
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                disableNotification: true,
                                parseMode : ParseMode.Html,
                                text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 还未完全挣脱口塞就已经渴望新的口塞了!");
                        }
                        else
                        {
                            var gag = Program.DB.GetGagItemInfo(comm[1]);

                            #region 是管理 绕过绒度限制
                            if (CheckIsAdmin(botClient, message.Chat.Id, message.From.Id) == true) // is true
                            {
                                Program.DB.AddRBQFroms(message.Chat.Id, message.From.Id, message.From.Id);

                                var R = new Random();
                                var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id);
                                var tm = DateTime.UtcNow.AddHours(8);

                                rbq.StartLockTime = tm.Ticks;
                                rbq.LockCount = gag.UnLockCount;
                                rbq.GagId = gag.Id;
                                Program.DB.SetRBQStatus(rbq);

                                var msg = $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 给自己带上了 <code>{comm[1]}</code>!\n{gag.SelfLockMsg[R.Next(0, gag.SelfLockMsg.Length)]}";

                                #region 内存队列存在检查
                                if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                                {
                                    var timeout = (tm.AddMinutes(Program.LockTime) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                    var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                                    Program.List.TryAdd(rbq.Id, rbqx);
                                }
                                else
                                {
                                    rbqItem.Stop();
                                    rbqItem.Id = rbq.Id;
                                    rbqItem.GagId = gag.Id;
                                    rbqItem.StartLockTime = tm.Ticks;
                                    rbqItem.LockCount = gag.UnLockCount;
                                    rbqItem.ResetTimer();
                                    Program.List.TryAdd(rbq.Id, rbqItem);
                                }
                                #endregion

                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: msg);
                            }
                            #endregion
                            else
                            {
                                if (Program.DB.GetRBQPoint(message.From.Id) < gag.LimitPoint)
                                {
                                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                    botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        disableNotification: true,
                                        parseMode: ParseMode.Html,
                                        text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 的绒度不够使用 <code>{gag.Name}</code>!");
                                }
                                else
                                {
                                    Program.DB.AddRBQFroms(message.Chat.Id, message.From.Id, message.From.Id);

                                    var R = new Random();
                                    var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id);
                                    var tm = DateTime.UtcNow.AddHours(8);

                                    rbq.StartLockTime = tm.Ticks;
                                    rbq.LockCount = gag.UnLockCount;
                                    rbq.GagId = gag.Id;
                                    Program.DB.SetRBQStatus(rbq);

                                    var msg = $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 给自己带上了 <code>{comm[1]}</code>!\n{gag.SelfLockMsg[R.Next(0, gag.SelfLockMsg.Length)]}";

                                    #region 内存队列存在检查
                                    if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                                    {
                                        var timeout = (tm.AddMinutes(Program.LockTime) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                        var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                                        Program.List.TryAdd(rbq.Id, rbqx);
                                    }
                                    else
                                    {
                                        rbqItem.Stop();
                                        rbqItem.Id = rbq.Id;
                                        rbqItem.GagId = gag.Id;
                                        rbqItem.StartLockTime = tm.Ticks;
                                        rbqItem.LockCount = gag.UnLockCount;
                                        rbqItem.ResetTimer();
                                        Program.List.TryAdd(rbq.Id, rbqItem);
                                    }
                                    #endregion

                                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                    botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        disableNotification: true,
                                        parseMode: ParseMode.Html,
                                        text: msg);
                                }
                            }
                        }
                    }
                }
                else
                {
                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"参数不够!请输入 /help 查看帮助!");
                }
            }
            #endregion
        }

        private static void RBQPointProcess(ITelegramBotClient botClient, Message message)
        {
            if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0);
            if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

            if (message.ReplyToMessage != null)
            {
                if (message.ReplyToMessage.From.IsBot == true)
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: $"机器人永不为绒!");
                }
                else
                {
                    if (Program.DB.GetRBQExist(message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQ(message.ReplyToMessage.From.Id, 0);
                    if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id, 0, false);

                    var point = Program.DB.GetRBQPoint(message.ReplyToMessage.From.Id);

                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> 的绒度为 {point}");
                }
            }
            else
            {
                var point = Program.DB.GetRBQPoint(message.From.Id);

                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    replyToMessageId: message.MessageId,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 的绒度为 {point}");
            }
        }
        #endregion

        #region 错误拦截
        /// <summary>拦截API错误并输出</summary>
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:{Environment.NewLine}[{apiRequestException.ErrorCode}]{Environment.NewLine}{apiRequestException.Message}{Environment.NewLine}错误信息: {Environment.NewLine}{exception.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        /// <summary>未知api处理(可能是tg服务器的api更新了或者单纯没有去拦截而已)</summary>
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            //Console.WriteLine($"未知更新类型: {update.Type}, 可能是API变更");
            return Task.CompletedTask;
        }
        #endregion

        #region 消息处理
        public static async Task HandleUpdateAsyncIgnore(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
                Console.WriteLine($"消息处理时出错: {Environment.NewLine}错误信息: {Environment.NewLine}{exception.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}");
            }
        }

        /// <summary>所有接收消息拦截处理</summary>
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                #region 未被使用的消息拦截器
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                //UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                #endregion

                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
                Console.WriteLine($"消息拦截时出错: {Environment.NewLine}错误信息: {Environment.NewLine}{exception.Message}{Environment.NewLine}错误堆栈:{Environment.NewLine}{exception.StackTrace}{Environment.NewLine}");
            }
        }

        /// <summary>内联查询处理(就是直接输入@Bot名 空格后返回的按钮/功能)</summary>
        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
        {
            if (inlineQuery.From.IsBot == false)
            {
                // 显示的结果集合 // 这里不能返回空消息,不然必定不显示
                InlineQueryResult[] results = {
                    new InlineQueryResultArticle(
                        id: "0", // 这个结果的唯一标识符 id
                        title: "说猫话",
                        inputMessageContent: new InputTextMessageContent(GetRBQSay())),
                    new InlineQueryResultArticle(
                        id: "1", // 这个结果的唯一标识符 id
                        title: "查挣脱次数",
                        inputMessageContent: new InputTextMessageContent("/count")),
                    new InlineQueryResultArticle(
                        id: "2",
                        title: "查绒度",
                        inputMessageContent: new InputTextMessageContent(GetRBQPoint(inlineQuery.From.Id))),
                    new InlineQueryResultArticle(
                        id: "3",
                        title: "查询口塞列表",
                        inputMessageContent: new InputTextMessageContent(GetAllGag()){ ParseMode = ParseMode.Html })
                };

                botClient.AnswerInlineQueryAsync(
                    inlineQueryId: inlineQuery.Id,
                    results: results,
                    isPersonal: true, // 必须为 true,不然三个个人同时查询结果会变成同一个
                    cacheTime: 0); // 返回的inline查询结果在服务器最长保存时间（单位：秒），默认300秒
            }
        }

        /// <summary>处理Bot收到的点击按钮的数据(例如验证按钮返回的数据)</summary>
        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var data = callbackQuery.Data.Split(' ');
            switch (data[0])
            {
                case "kickme":
                    if (Program.BanList.TryRemove(callbackQuery.From.Id, out WaitBan wait) != true) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您无法完成别人的验证!", true, null, 30);
                    else
                    {
                        wait.Stop();

                        var result = botClient.GetChatMemberAsync(wait.ChatId, wait.UserId).Result;

                        botClient.DeleteMessageAsync(wait.ChatId, wait.CallbackMsgId);
                        botClient.BanChatMemberAsync(wait.ChatId, wait.UserId);
                        botClient.UnbanChatMemberAsync(wait.ChatId, wait.UserId);
                        botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "祝您身体健康,再见!", true, null, 30);

                        botClient.SendTextMessageAsync(
                            chatId: wait.ChatId,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"由于 <a href=\"tg://user?id={wait.UserId}\">{result.User?.FirstName} {result.User?.LastName}</a> 的主动要求,Ta已被移出本群.");
                    }
                    break;
                case "verifyme":
                    if (Program.BanList.TryRemove(callbackQuery.From.Id, out WaitBan wait2) != true) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您无法完成别人的验证!", true, null, 30);
                    else
                    {
                        wait2.Stop();
                        botClient.DeleteMessageAsync(wait2.ChatId, wait2.CallbackMsgId);
                        botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "恭喜您通过验证!", false, null, 30);

                        botClient.RestrictChatMemberAsync(wait2.ChatId, wait2.UserId, new ChatPermissions
                        {
                            CanSendMessages = true,
                            CanSendMediaMessages = true,
                            CanSendPolls = true,
                            CanSendOtherMessages = true,
                            CanAddWebPagePreviews = true,
                            CanChangeInfo = false,
                            CanInviteUsers = true,
                            CanPinMessages = false
                        }, DateTime.UtcNow.AddYears(2));

                        var result = botClient.GetChatMemberAsync(wait2.ChatId, wait2.UserId).Result;

                        botClient.SendTextMessageAsync(
                            chatId: wait2.ChatId,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"欢迎 <a href=\"tg://user?id={wait2.UserId}\"><b><u>{result.User?.FirstName} {result.User?.LastName}</u></b></a> 加入!");
                    }
                    break;
                case "adminverify":
                    if (CheckIsAdmin(botClient, Convert.ToInt64(data[1]), callbackQuery.From.Id) != true) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您不是管理员!", false, null, 30);
                    else if (data.Length > 1)
                    {
                        if (Program.BanList.TryRemove(Convert.ToInt64(data[2]), out WaitBan wait3) == true)
                        {
                            wait3.Stop();
                            botClient.DeleteMessageAsync(wait3.ChatId, wait3.CallbackMsgId);
                            botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您已放行该用户!", false, null, 30);

                            botClient.RestrictChatMemberAsync(wait3.ChatId, wait3.UserId, new ChatPermissions
                            {
                                CanSendMessages = true,
                                CanSendMediaMessages = true,
                                CanSendPolls = true,
                                CanSendOtherMessages = true,
                                CanAddWebPagePreviews = true,
                                CanChangeInfo = false,
                                CanInviteUsers = true,
                                CanPinMessages = false
                            }, DateTime.UtcNow.AddYears(2));

                            var result = botClient.GetChatMemberAsync(wait3.ChatId, wait3.UserId).Result;

                            botClient.SendTextMessageAsync(
                                chatId: wait3.ChatId,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: $"欢迎管理员批准的 <a href=\"tg://user?id={wait3.UserId}\"><b><u>{result.User?.FirstName} {result.User?.LastName}</u></b></a> 加入!");
                        }
                        else botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "参数错误!可能用户已验证!", false, null, 30);
                    }
                    else botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "参数错误! inline请求的目标 id 不存在!", false, null, 30);
                    break;
                case "adminkick":
                    if (CheckIsAdmin(botClient, Convert.ToInt64(data[1]), callbackQuery.From.Id) != true) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您不是管理员!", false, null, 30);
                    else if (data.Length > 1)
                    {
                        if (Program.BanList.TryRemove(Convert.ToInt64(data[2]), out WaitBan wait4) == true)
                        {
                            wait4.Stop();

                            var result = botClient.GetChatMemberAsync(wait4.ChatId, wait4.UserId).Result;

                            botClient.DeleteMessageAsync(wait4.ChatId, wait4.CallbackMsgId);
                            botClient.BanChatMemberAsync(wait4.ChatId, wait4.UserId);
                            //botClient.UnbanChatMemberAsync(wait4.ChatId, wait4.UserId);
                            botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您已移除该用户!", false, null, 30);

                            botClient.SendTextMessageAsync(
                                chatId: wait4.ChatId,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: $"<a href=\"tg://user?id={wait4.UserId}\">{result.User?.FirstName} {result.User?.LastName}</a> 已被管理员永久移除!");

                            if (Program.DB.GetRBQStatusExist(wait4.ChatId, wait4.UserId) == true) Program.DB.DelRBQStatus(wait4.ChatId, wait4.UserId);
                        }
                        else botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "参数错误!可能用户已移除!", false, null, 30);
                    }
                    else botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "参数错误! inline请求的目标 id 不存在!", false, null, 30);
                    break;
                default:
                    botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "", false, null, 30);
                    Console.WriteLine($"未知 inline 请求,来自 UserId:{callbackQuery.From.Id} UserName:{callbackQuery.From?.Username} 昵称:{callbackQuery.From.FirstName} {callbackQuery.From?.LastName} 请求内容:\n{callbackQuery?.Data}");
                    break;
            }
        }

        /// <summary>接收消息分类</summary>
        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
#if DEBUG
            Console.WriteLine($"RecvMSG! MsgType: {message.Type} | MsgId: {message.MessageId}\r\nChatType: {message.Chat.Type} | ChatId: {message.Chat.Id} | ChatTitle: {message.Chat.Title}");
#endif
            if (message.From.IsBot == false)
            {
                switch (message.Type)
                {
                    case MessageType.Text:
                        switch (message.Chat.Type)
                        {
                            case ChatType.Private:
                                PrivateMsgProcess(botClient, message);
                                break;
                            case ChatType.Group:
                            case ChatType.Supergroup:
                                MessageProcess(botClient, message);
                                MessageCountProcess(message);
                                break;
                            default:
                                break;
                        }
                        break;
                    case MessageType.ChatMembersAdded: // Bot & User 加入了群组
                        if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowVerify) == true) ChatMembersAdded(botClient, message);
                        break;
                    case MessageType.ChatMemberLeft: // Bot & User 离开了群组
                        if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowVerify) == true) ChatMemberLeft(botClient, message);
                        break;

                    #region 不使用/拦截的消息类型
                    //case MessageType.Photo:
                    //case MessageType.Audio:
                    //case MessageType.Video:
                    //case MessageType.Voice:
                    //case MessageType.Document:
                    //case MessageType.Sticker:
                    //case MessageType.Location:
                    //case MessageType.Contact:
                    //case MessageType.Venue:
                    //case MessageType.Game:
                    //case MessageType.VideoNote:
                    //case MessageType.Invoice:
                    //case MessageType.SuccessfulPayment:
                    //case MessageType.WebsiteConnected:
                    //case MessageType.ChatTitleChanged:
                    //case MessageType.ChatPhotoChanged:
                    //case MessageType.MessagePinned:
                    //case MessageType.ChatPhotoDeleted:
                    //case MessageType.GroupCreated:
                    //case MessageType.SupergroupCreated:
                    //case MessageType.ChannelCreated:
                    //case MessageType.MigratedToSupergroup:
                    //case MessageType.MigratedFromGroup:
                    //case MessageType.Poll:
                    //case MessageType.Dice:
                    //case MessageType.MessageAutoDeleteTimerChanged:
                    //case MessageType.ProximityAlertTriggered:
                    //case MessageType.VoiceChatScheduled:
                    //case MessageType.VoiceChatStarted:
                    //case MessageType.VoiceChatEnded:
                    //case MessageType.VoiceChatParticipantsInvited:
                    //case MessageType.Document:
                    //case MessageType.MigratedToSupergroup: // 用户权限被提升?
                    //case MessageType.Unknown:
                    #endregion

                    case MessageType.Sticker:
                        StickerProcess(botClient, message);
                        MessageCountProcess(message);
                        break;
                    default:
                        Console.WriteLine($"收到未处理的消息 消息类型: {message.Chat.Type} 消息内容: {message?.Text} 消息来源: {message.From?.Id} @{message.From?.Username} {message.From?.FirstName} {message.From?.LastName}");
                        break;
                }
            }
        }

        /// <summary>群组加入了新人的处理</summary>
        private static void ChatMembersAdded(ITelegramBotClient botClient, Message message)
        {
            for (int i = 0; i < message.NewChatMembers.Length; i++)
            {
                if (message.NewChatMembers[i].IsBot != true)
                {
                    var exist = Program.DB.GetRBQExist(message.NewChatMembers[i].Id);
                    if (exist != true) Program.DB.AddRBQ(message.NewChatMembers[i].Id, 0);

                    exist = Program.DB.GetRBQStatusExist(message.Chat.Id, message.NewChatMembers[i].Id);
                    if (exist != true) Program.DB.AddRBQStatus(message.Chat.Id, message.NewChatMembers[i].Id, 0, false);

                    InlineKeyboardMarkup inlineKeyboard = new(new[] {
                        new [] {
                            InlineKeyboardButton.WithCallbackData(text: "我不可爱,别验证我", callbackData: "kickme"),
                            InlineKeyboardButton.WithCallbackData(text: "我很可爱,请验证我", callbackData: "verifyme"),
                        },
                        new [] {
                            InlineKeyboardButton.WithCallbackData(text: "管理通过", callbackData: $"adminverify {message.Chat.Id} {message.NewChatMembers[i].Id}"),
                            InlineKeyboardButton.WithCallbackData(text: "管理踢出", callbackData: $"adminkick {message.Chat.Id} {message.NewChatMembers[i].Id}"),
                        },
                    });

                    botClient.RestrictChatMemberAsync(message.Chat.Id, message.NewChatMembers[i].Id, new ChatPermissions
                    {
                        CanSendMessages = true,
                        CanSendMediaMessages = false,
                        CanSendPolls = false,
                        CanSendOtherMessages = false,
                        CanAddWebPagePreviews = false,
                        CanChangeInfo = false,
                        CanInviteUsers = false,
                        CanPinMessages = false
                    }, DateTime.UtcNow.AddYears(2));

                    var result = botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        replyMarkup: inlineKeyboard,
                        text: $"欢迎 <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.NewChatMembers[i].FirstName} {message.NewChatMembers[i]?.LastName}</u></b></a> 加入!\n请发送 「<code>/verify 我很可爱</code>」 或点击下面的按钮来完成加群验证(点击可复制),否则您将会在120秒后被移出群组.").Result;

                    var b = new WaitBan(message.Chat.Id, message.NewChatMembers[i].Id, result.MessageId, Program.UserVerifyTime, botClient);
                    Program.BanList.TryAdd(message.NewChatMembers[i].Id, b);
                }
                else
                {
                    if (message.NewChatMembers[i].Id != botClient.BotId)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"欢迎 <a href=\"tg://user?id={message.NewChatMembers[i].Id}\">新Bot</a> 加入!");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: "绒布球管理器已入驻本群!");
                    }
                }
            }
        }

        /// <summary>群组有人离开的处理</summary>
        private static void ChatMemberLeft(ITelegramBotClient botClient, Message message)
        {
            if (message.LeftChatMember.IsBot != true)
            {
                //Program.DB.DelRBQStatus(message.Chat.Id, message.LeftChatMember.Id);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    disableNotification: true,
                    parseMode: ParseMode.Html,
                    text: $"<a href=\"tg://user?id={message.LeftChatMember.Id}\"><b><u>{message.LeftChatMember.FirstName} {message.LeftChatMember?.LastName}</u></b></a> 离开了群组.");
            }
            else
            {
                if (message.LeftChatMember.Id != botClient.BotId)
                {
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"一只 <a href=\"tg://user?id={message.LeftChatMember.Id}\">Bot</a> 离开了群组.",
                        parseMode: ParseMode.Html,
                        disableNotification: true);
                }
                else
                {
                    Console.WriteLine($"本 Bot被 UserId:{message.From.Id} {message.From?.Username} <b><u>{message.From.FirstName} {message.From?.LastName}</u></b> 丢出 GroupId:{message.Chat.Id} {message.Chat?.Username} {message.Chat?.Title}");
                }
            }
        }

        /// <summary>私聊Bot的处理</summary>
        private static void PrivateMsgProcess(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.ToLower().Split(' ');
            switch (comm[0])
            {
                case "/start":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "欢迎! 请务必阅读 /help /about /privacy 点击蓝色字即可");
                    break;
                case "/count":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "本命令只能在群内使用!");
                    break;
                case "/gag":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "本命令只能在群内使用!");
                    break;
                case "/rbqpoint":
                    GetRBQPoint(botClient, message);
                    break;
                case "/list":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: GetAllGag());
                    break;
                case "/ping":
                    if (message.From.Id == Program.DebugUserId || message.From.Id == Program.AdminId) PingProcess(botClient, message);
                    else botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "您不是管理员,不能使用此命令");
                    break;
                case "/verify":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "本命令只能在群内使用!");
                    break;
                case "/getid":
                    botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: $"您的用户Id: <code>{message.From.Id}</code>");
                    break;
                case "/getgid":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "本命令只能在群内使用!");
                    break;
                case "/help":
                    Help(botClient, message);
                    break;
                case "/about":
                    About(botClient, message);
                    break;
                case "/privacy":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: Program.PrivacyTxt);
                    break;
                case "/admin":
                    if (message.From.Id == Program.DebugUserId || message.From.Id == Program.AdminId) AdminProcess(botClient, message);
                    else botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "您不是管理员,不能使用此命令");
                    break;
                default:
                    if (message.ForwardDate != null)
                    {
                        if (message.ForwardFrom != null)
                        {
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                parseMode : ParseMode.Html,
                                text: $"用户Id: <code>{message.ForwardFrom?.Id}</code>");
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                replyToMessageId: message.MessageId,
                                disableNotification: true,
                                text: "无法获取这条消息的用户Id!");
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: "命令错误! 请输入 /help 查看命令!");
                    }
                    break;
            }
        }

        /// <summary>群内消息处理</summary>
        private static void MessageProcess(ITelegramBotClient botClient, Message message)
        {
#if Debug
            if (Program.DB.GetAllowGroupExist(message.Chat.Id) != true) Program.DB.AddAllowGroup(message.Chat.Id);
#endif

            if (Program.DB.GetAllowGroupExists(message.Chat.Id)) // 消息进入
            {
                #region 进群验证
                if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowVerify) == true)
                {
                    if (Program.BanList.TryRemove(message.From.Id, out WaitBan ban) == true)
                    {
                        if (message.Text != "/verify 我很可爱")
                        {
                            ban.FailCount++;
                            if (ban.FailCount <3)
                            {
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                var result = botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: false,
                                    parseMode: ParseMode.Html,
                                    text: $"验证失败! <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 还有 {3-ban.FailCount} 次验证机会.").Result;

                                Program.BanList.TryAdd(ban.UserId, ban);
                            }
                            else
                            {
                                ban.Stop();

                                botClient.BanChatMemberAsync(ban.ChatId, ban.UserId);
                                botClient.UnbanChatMemberAsync(ban.ChatId, ban.UserId);

                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.DeleteMessageAsync(ban.ChatId, ban.CallbackMsgId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    disableNotification: true,
                                    parseMode: ParseMode.Html,
                                    text: $"由于多次验证失败! <a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 已被移出本群.");
                            }
                        }
                        else
                        {
                            ban.Stop();

                            var result = botClient.GetChatMemberAsync(ban.ChatId, ban.UserId).Result;

                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.DeleteMessageAsync(ban.ChatId, ban.CallbackMsgId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: $"恭喜 <a href=\"tg://user?id={ban.ChatId}\">{result.User?.FirstName} {result.User?.LastName}</a> 验证通过!");
                        }
                    }
                }
                #endregion

                if (message.Text[0] == '/') CommandProcess(botClient, message);
                else if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowGag) == true)
                {
                    if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0); // 检查是否注册RBQ的全局信息
                    if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false); // 检查是否注册RBQ的群组状态信息


                    //Program.DB.SetRBQStatus(message.Chat.Id, message.From.Id, 1, false, 1, DateTime.UtcNow.AddHours(8).Ticks, new long[] { message.From.Id }); // 迫害所有人 （调试用）
                    var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id); // 获取RBQ状态
                    if (rbq != null)
                    {
                        var time = new DateTime(rbq.StartLockTime).AddMinutes(Program.LockTime);
                        #region 绒布球被塞口塞后处理
                        if (rbq.LockCount > 0 && DateTime.UtcNow.AddHours(8) < time) // 有锁定次数并在时间内
                        {
                            #region 内存队列存在检查
                            if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false)
                            {
                                var timeout = (time - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout);
                                Program.List.TryAdd(rbq.Id, rbqx);
                            }
                            else
                            {
                                rbqItem.Stop();
                                rbqItem.Id = rbq.Id;
                                rbqItem.GagId = rbq.GagId;
                                rbqItem.StartLockTime = rbq.StartLockTime;
                                rbqItem.LockCount = rbq.LockCount;
                                rbqItem.ResetTimer();
                                Program.List.TryAdd(rbq.Id, rbqItem);
                            }
                            #endregion

                            if (message.ViaBot != null && message.ViaBot?.Id == botClient.BotId && message.Text == "/count") CountProcess(botClient, message);
                            else
                            {
                                #region 输入不规范的绒布球处理
                                if (TypeProcess(message.Text) > 0) // 不合规
                                { // 如果不符合要求删除消息并提示
                                    var R = new Random();
                                    if (R.Next(0, 100) >= 70)
                                    {
                                        rbq.LockCount++;
                                        rbq.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                                        Program.DB.SetRBQStatus(rbq);
                                        if (Program.List.TryRemove(rbq.Id, out RBQList rbqI))
                                        {
                                            rbqItem.Stop();
                                            rbqI.LockCount++;
                                            rbqI.StartLockTime = rbq.StartLockTime;
                                            rbqI.ResetTimer();

                                            Program.List.TryAdd(rbq.Id, rbqI);
                                        }

                                        Program.DB.SetRBQPointAdd1(rbq.RBQId);

                                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                        botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            disableNotification: true,
                                            parseMode: ParseMode.Html,
                                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图逃脱口塞的限制!\n作为惩罚我们增加了1点它需要挣脱的次数");
                                    }
                                    else
                                    {
                                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                        botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            disableNotification: true,
                                            parseMode: ParseMode.Html,
                                            text: $"<a href=\"tg://user?id={message.From?.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图逃脱口塞的限制!");
                                    }
                                }
                                #endregion

                                #region 输入规范的绒布球处理
                                else // 合规 并进行挣脱计数-1 并重置时间
                                {
                                    if (rbq.LockCount-1 == 0)
                                    {
                                        var gag = Program.DB.GetGagItemInfo(rbq.GagId);
                                        var R = new Random();

                                        rbq.LockCount = 0;
                                        rbq.FromId = new long[0];
                                        rbq.GagId = 0;
                                        rbq.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                                        Program.DB.SetRBQStatus(rbq);
                                        Program.DB.SetRBQPointAdd1(rbq.RBQId);

                                        botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            disableNotification: true,
                                            parseMode: ParseMode.Html,
                                            text: $"<a href=\"tg://user?id={message.From?.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 成功挣脱了被人们安装的 <code>{gag.Name}</code>!\n{gag.UnLockMsg[R.Next(0, gag.UnLockMsg.Length)]}");

                                        Program.List.TryRemove(rbq.Id, out RBQList rbqx);
                                        rbqx.Stop();
                                    }
                                    else if (rbq.LockCount-1 > 0)
                                    {
                                        rbq.LockCount--;
                                        rbq.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                                        Program.DB.SetRBQStatus(rbq);

                                        if (Program.List.TryRemove(rbq.Id, out RBQList rbqI))
                                        {
                                            rbqItem.Stop();
                                            rbqI.LockCount--;
                                            rbqI.StartLockTime = rbq.StartLockTime;
                                            rbqI.ResetTimer();
                                            Program.DB.SetRBQPointAdd1(rbq.RBQId);

                                            Program.List.TryAdd(rbq.Id, rbqI);
                                        }
                                    }
                                    DelayDeleteMessage(botClient, message.Chat.Id, message.MessageId, 3000);
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                }
            }
            else
            {
                Console.WriteLine($"未验证的 ChatId:{message.Chat.Id} ChatName:{message.Chat?.Username} ChatTitle: {message.Chat?.Title} ChatLink: {message.Chat?.InviteLink} ChatNick:{message.Chat?.FirstName} {message.Chat?.LastName} 试图使用机器人!");
            }
        }

        /// <summary>群内命令的处理</summary>
        private static async void CommandProcess(ITelegramBotClient botClient, Message message)
        {
            string[] comm = new string[] { };
#if DEBUG
            if (message.Text.ToLower().IndexOf("@rbqexbot") > 0)
            {
                var len = message.Text.ToLower().IndexOf("@rbqexbot");
                comm = message.Text.Substring(0, len).ToLower().Split(' ');
            }
            else comm = message.Text.ToLower().Split(' ');
#else
            if (message.Text.ToLower().IndexOf("@rbq_ch_bot") > 0)
            {
                var len = message.Text.ToLower().IndexOf("@rbq_ch_bot");
                comm = message.Text.Substring(0, len).ToLower().Split(' ');
            }
            else comm = message.Text.ToLower().Split(' ');
#endif

            switch (comm[0])
            {
                case "/count":
                    if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowGag) == true) CountProcess(botClient, message);
                    break;
                case "/gag":
                    if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowGag) == true) GagProcess(botClient, message, comm);
                    break;
                case "/rbqpoint":
                    if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowGag) == true) RBQPointProcess(botClient, message);
                    break;
                case "/list":
                    if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowGag) == true) 
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: GetAllGag());
                    break;
                case "/ping":
                    if (CheckIsAdmin(botClient, message.Chat.Id, message.From.Id)) PingProcess(botClient, message);
                    else botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 不是管理员, 不能使用此命令!");
                    break;
                case "/verify":
                    if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowVerify) == true)
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图使用验证命令, 但此命令只允许加入时的用户使用!");
                    break;
                case "/getid":
                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    if (message.ReplyToMessage != null)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 请求获取 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"><b><u>{message.ReplyToMessage.From.FirstName} {message.ReplyToMessage?.From.LastName}</u></b></a> Id: <code>{message.ReplyToMessage.From.Id}</code>");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> Id: <code>{message.From.Id}</code>");
                    }
                    break;
                case "/getgid":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        disableNotification: true,
                        parseMode: ParseMode.Html,
                        text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 请求获取当前群组 Id: <code>{message.Chat.Id}</code>");
                    break;
                case "/admin":
                    if (CheckIsAdmin(botClient, message.Chat.Id, message.From.Id)) AdminProcess(botClient, message);
                    else botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            parseMode: ParseMode.Html,
                            text: $"<a href=\"tg://user?id={message.From.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 不是管理员, 不能使用此命令!");
                    break;
                case "/help":
                    Help(botClient, message);
                    break;
                case "/about":
                    About(botClient, message);
                    break;
                case "/privacy":
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        disableNotification: true,
                        text: Program.PrivacyTxt);
                    break;
                default:
                    if (Program.DB.GetAllowGroupExists(message.Chat.Id) == true)
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            replyToMessageId: message.MessageId,
                            disableNotification: true,
                            text: "命令错误! 请输入 /help 查看命令!");
                    break;
            }
        }

        /// <summary>群内贴纸消息处理</summary>
        private static void StickerProcess(ITelegramBotClient botClient, Message message)
        {
            if (Program.DB.GetAllowGroupExists(message.Chat.Id)) // 消息进入
            {
                if (Program.DB.GetAllowFunctionStatus(message.Chat.Id, AllowFunction.AllowGag) == true)
                {
                    if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0); // 检查是否注册RBQ的全局信息
                    if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false); // 检查是否注册RBQ的群组状态信息

                    var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id);
                    if (rbq != null)
                    {
                        var time = new DateTime(rbq.StartLockTime).AddMinutes(Program.LockTime);
                        if (rbq.LockCount > 0 && DateTime.UtcNow.AddHours(8) < time)
                        {
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                disableNotification: true,
                                parseMode: ParseMode.Html,
                                text: $"<a href=\"tg://user?id={message.From?.Id}\"><b><u>{message.From.FirstName} {message.From?.LastName}</u></b></a> 试图逃脱口塞的限制!");
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>(目前不使用)选择内联结果接收</summary>
        //private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
        //{
        //    Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId} {chosenInlineResult.InlineMessageId}");
        //    return Task.CompletedTask;
        //}
    }
}

#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法