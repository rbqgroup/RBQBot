﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace RBQBot
{
    public class Handlers
    {

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
                "呃", "呃呃",
                "哦", "哦哦", "哦哦哦",
                "嗷", "嗷嗷",
                "呕",
                "噢",
                "喔", "喔喔", "喔喔喔",

                "唔嗯", "唔嗯",
                "唔啊",
            };
            string[] chartb = {
                ".",
                "..",
                "…", //"…", "…", "…", "…", "…",
                //"……", "……", "……", "……",
                //"………", "………",
                "！", "！！",
                "？", "？？",
                "！？", "？！",
                "，", "，，"
            };

            var R = new Random();
            var sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
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
            var rbq = Program.DB.GetRBQInfo(telegramId);
            return $"我的绒度有{rbq.RBQPoint}点，感觉自己绒绒哒.";
        }

        public static string GetAllGag()
        {
            var count = Program.DB.GetGagItemCount();
            var gag = Program.DB.GetAllGagItems();
            var sb = new StringBuilder();
            sb.AppendLine($"一共有 {count} 个口塞.");
            sb.AppendLine($"{"口塞名字", 0}{"要求绒度", 15}{"挣扎次数", 20}");
            sb.AppendLine("================================");
            foreach (var i in gag)
            {
                var limit = i.ShowLimit ? i.LimitPoint.ToString() : "??";
                var unlock = i.ShowUnlock ? i.UnLockCount.ToString() : "??";
                sb.AppendLine($"{i.Name, 0}{limit, 15}{unlock, 25}");
            }
            sb.AppendLine("================================");
            return sb.ToString();
        }

        public const string GetUsageHelp = "";

        //public static string SwitchRBQUse(long telegramId)
        //{

        //    Console.WriteLine();
        //    return "";
        //}

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                #region Unused Message Type Process
                // UpdateType.Unknown:
                // UpdateType.ChannelPost:
                // UpdateType.EditedChannelPost:
                // UpdateType.ShippingQuery:
                // UpdateType.PreCheckoutQuery:
                // UpdateType.Poll:
                //UpdateType.EditedMessage => BotOnMessageReceived(botClient, update.EditedMessage!),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(botClient, update.ChosenInlineResult!),
                #endregion

                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.InlineQuery => BotOnInlineQueryReceived(botClient, update.InlineQuery!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"RecvMSG! MsgType: {message.Type} | MsgId: {message.MessageId}\r\n" +
                    $"ChatType: {message.Chat.Type} | ChatId: {message.Chat.Id} | ChatTitle: {message.Chat.Title}");
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
                                MsgProcess(botClient, message);
                                break;
                            default:
                                break;
                        }
                        break;
                    case MessageType.ChatMembersAdded: // Bot & User 加入了群组
                        if (Program.DB.GetAllowGroupExist(message.Chat.Id) == true) ChatMembersAdded(botClient, message);
                        break;
                    case MessageType.ChatMemberLeft: // Bot & User 离开了群组
                        if (Program.DB.GetAllowGroupExist(message.Chat.Id) == true) ChatMemberLeft(botClient, message);
                        break;
                    #region /////
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
                    //case MessageType.Unknown:
                    #endregion
                    case MessageType.MigratedToSupergroup: // 权限提升
                        break;
                    case MessageType.Sticker:
                    case MessageType.Document:
                        StickerProcess(botClient, message);
                        break;
                    default:
                        break;
                }
                #region Old Command Switch
                //Console.WriteLine($"Receive message type: {message.Type}");
                //if (message.Type != MessageType.Text)
                //    return;

                //var action = message.Text!.Split(' ')[0] switch
                //{
                //    "/gag help" => SendInlineKeyboard(botClient, message),
                //    "/keyboard" => SendReplyKeyboard(botClient, message),
                //    "/remove" => RemoveKeyboard(botClient, message),
                //    "/photo" => SendFile(botClient, message),
                //    "/request" => RequestContactAndLocation(botClient, message),
                //    _ => Usage(botClient, message)
                //};
                //Message sentMessage = await action;
                //Console.WriteLine($"The message was sent with id: {sentMessage.MessageId}");
                #endregion
            }

            #region Demo Code
            // Send inline keyboard
            // You can process responses in BotOnCallbackQueryReceived handler
            //static async Task<Message> SendInlineKeyboard(ITelegramBotClient botClient, Message message)
            //{
            //    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            //    // Simulate longer running task
            //    await Task.Delay(500);

            //    InlineKeyboardMarkup inlineKeyboard = new(
            //        new[]
            //        {
            //        // first row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("1.1", "11"),
            //            InlineKeyboardButton.WithCallbackData("1.2", "12"),
            //        },
            //        // second row
            //        new []
            //        {
            //            InlineKeyboardButton.WithCallbackData("2.1", "21"),
            //            InlineKeyboardButton.WithCallbackData("2.2", "22"),
            //        },
            //        });

            //    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                                text: "Choose",
            //                                                replyMarkup: inlineKeyboard);
            //}

            //static async Task<Message> SendReplyKeyboard(ITelegramBotClient botClient, Message message)
            //{
            //    ReplyKeyboardMarkup replyKeyboardMarkup = new(
            //        new[]
            //        {
            //            new KeyboardButton[] { "1.1", "1.2" },
            //            new KeyboardButton[] { "2.1", "2.2" },
            //        })
            //    {
            //        ResizeKeyboard = true
            //    };

            //    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                                text: "Choose",
            //                                                replyMarkup: replyKeyboardMarkup);
            //}

            //static async Task<Message> RemoveKeyboard(ITelegramBotClient botClient, Message message)
            //{
            //    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                                text: "Removing keyboard",
            //                                                replyMarkup: new ReplyKeyboardRemove());
            //}

            //static async Task<Message> SendFile(ITelegramBotClient botClient, Message message)
            //{
            //    await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            //    const string filePath = @"Files/tux.png";
            //    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            //    return await botClient.SendPhotoAsync(chatId: message.Chat.Id,
            //                                          photo: new InputOnlineFile(fileStream, fileName),
            //                                          caption: "Nice Picture");
            //}

            //static async Task<Message> RequestContactAndLocation(ITelegramBotClient botClient, Message message)
            //{
            //    ReplyKeyboardMarkup RequestReplyKeyboard = new(
            //        new[]
            //        {
            //        KeyboardButton.WithRequestLocation("Location"),
            //        KeyboardButton.WithRequestContact("Contact"),
            //        });

            //    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                                text: "Who or Where are you?",
            //                                                replyMarkup: RequestReplyKeyboard);
            //}

            //static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
            //{
            //    const string usage = "使用帮助:\n" +
            //                         "/gag help - 显示此帮助.\n" +
            //                         "/gag 口塞名称 - 给回复的用户「佩戴」口塞, 如果对方已经佩戴口塞, 则「加固」对方的口塞.\n" +
            //                         "/gag @username 口塞名称 - 给@username用户「佩戴」口塞, 如果对方已经佩戴口塞, 则「加固」对方的口塞.\n" +
            //                         "/gag on - 允许任何人给自己「佩戴」口塞, 为了方式骚扰默认是不允许的.\n" +
            //                         "/gag off - 不允许其他人给自己「佩戴」口塞, 如果已经处于「佩戴」状态, 不会影响当前已经「佩戴」中的口塞." +
            //                         "/rbqpoint - 查询自己的「绒度」.\n" +
            //                         "/rbqpoint @username - 查询@username的「绒度」.";

            //    return await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
            //                                                text: usage,
            //                                                replyMarkup: new ReplyKeyboardRemove());
            //}
            #endregion
        }

        // Process Inline Keyboard callback data
        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "kickme":
                    if (Program.BanList.TryRemove(callbackQuery.From.Id, out WaitBan wait) != true) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您无法完成别人的验证!", true, null, 30);
                    else
                    {
                        wait.Stop();
                        botClient.DeleteMessageAsync(wait.ChatId, wait.CallbackMsgId);
                        //botClient.BanChatMemberAsync(wait.ChatId, wait.UserId);
                        botClient.UnbanChatMemberAsync(wait.ChatId, wait.UserId);
                        botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "祝您身体健康,再见!", true, null, 30);

                        botClient.SendTextMessageAsync(
                                chatId: wait.ChatId,
                                text: $"由于主动要求, <a href=\"tg://user?id={wait.UserId}\">Ta</a> 已被移出本群.",
                                parseMode: ParseMode.Html,
                                disableNotification: true);
                    }
                    break;
                case "verifyme":
                    if (Program.BanList.TryRemove(callbackQuery.From.Id, out WaitBan wait2) != true) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您无法完成别人的验证!", true, null, 30);
                    else
                    {
                        wait2.Stop();
                        botClient.DeleteMessageAsync(wait2.ChatId, wait2.CallbackMsgId);
                        botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "恭喜您通过验证!", false, null, 30);

                        botClient.SendTextMessageAsync(
                            chatId: wait2.ChatId,
                            text: $"欢迎 <a href=\"tg://user?id={wait2.UserId}\">新绒布球</a> 加入!",
                            parseMode: ParseMode.Html,
                            disableNotification: true);
                    }
                    break;
                case "adminverify":
                    if (callbackQuery.From.Id != 1324338125) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您不是管理员!", true, null, 30);
                    else
                    {
                        Program.BanList.TryRemove(callbackQuery.From.Id, out WaitBan wait3);
                        wait3.Stop();
                        botClient.DeleteMessageAsync(wait3.ChatId, wait3.CallbackMsgId);
                        botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您已放行该用户!", false, null, 30);

                        botClient.SendTextMessageAsync(
                            chatId: wait3.ChatId,
                            text: $"欢迎管理员批准的 <a href=\"tg://user?id={wait3.UserId}\">新绒布球</a> 加入!",
                            parseMode: ParseMode.Html,
                            disableNotification: true);
                    }
                    break;
                case "adminkick":
                    if (callbackQuery.From.Id != 1324338125) botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您不是管理员!", true, null, 30);
                    else
                    {
                        Program.BanList.TryRemove(callbackQuery.From.Id, out WaitBan wait4);
                        wait4.Stop();
                        botClient.DeleteMessageAsync(wait4.ChatId, wait4.CallbackMsgId);
                        //botClient.BanChatMemberAsync(wait4.ChatId, wait4.UserId);
                        botClient.UnbanChatMemberAsync(wait4.ChatId, wait4.UserId);
                        botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您已移除该用户!", false, null, 30);

                        botClient.SendTextMessageAsync(
                            chatId: wait4.ChatId,
                            text: $"该 <a href=\"tg://user?id={wait4.UserId}\">新绒布球</a> 已被管理员移除!",
                            parseMode: ParseMode.Html,
                            disableNotification: true);
                    }
                    break;
                default:
                    break;
            }
            //if (Program.BanList.TryGetValue(callbackQuery.From.Id, out WaitBan wait) != true)
            //{
            //    botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "您无法完成别人的验证!", true, null, 30);
            //}
            //else
            //{
                
            //}
            //Console.WriteLine("Processing CallbackQuery");
            //await botClient.AnswerCallbackQueryAsync(
            //    callbackQueryId: callbackQuery.Id,
            //    text: $"Received A{callbackQuery.Data}");

            //await botClient.SendTextMessageAsync(
            //    chatId: callbackQuery.Message.Chat.Id,
            //    text: $"Received B{callbackQuery.Data}");
        }

        /// <summary>未知api处理</summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"未知更新类型: {update.Type}, 可能是API变更");
            return Task.CompletedTask;
        }

        #region RecvMsg Process
        /// <summary>内联查询处理</summary>
        /// <param name="botClient"></param>
        /// <param name="inlineQuery"></param>
        /// <returns></returns>
        private static async Task BotOnInlineQueryReceived(ITelegramBotClient botClient, InlineQuery inlineQuery)
        {
            #region OldCode
            //Program.DB.SetRBQInfo(inlineQuery.From.Id, 0, true);

            //var rbq = Program.DB.GetRBQInfo(inlineQuery.From.Id);
            //if (rbq != null)
            //{
            //    if (inlineQuery.From.IsBot == false && (inlineQuery.ChatType == ChatType.Supergroup || inlineQuery.ChatType == ChatType.Group) == true  && rbq.FastInline == true)
            //    {
            //        //显示的结果集合
            //        InlineQueryResult[] results = {
            //        new InlineQueryResultArticle(
            //            id: "0", // 这个结果的唯一标识符 id
            //            title: "说绒话",
            //            inputMessageContent: new InputTextMessageContent("这是一条测试消息"))};

            //        botClient.AnswerInlineQueryAsync(
            //            inlineQueryId: inlineQuery.Id,
            //            results: results,
            //            isPersonal: true, // 必须为 true,不然三个个人同时查询结果会变成同一个
            //            cacheTime: 0); // 返回的inline查询结果在服务器最长保存时间（单位：秒），默认300秒
            //    }
            //    else
            //    {
            //        InlineQueryResult[] results = { };

            //        botClient.AnswerInlineQueryAsync(
            //            inlineQueryId: inlineQuery.Id,
            //            results: results,
            //            isPersonal: true,
            //            cacheTime: 0);
            //    }
            //}
            //else
            //{
            //    InlineQueryResult[] results = { };

            //    botClient.AnswerInlineQueryAsync(
            //        inlineQueryId: inlineQuery.Id,
            //        results: results,
            //        isPersonal: true,
            //        cacheTime: 0);
            //}
            #endregion

            if (inlineQuery.From.IsBot == false && (inlineQuery.ChatType == ChatType.Supergroup || inlineQuery.ChatType == ChatType.Group) == true)
            {
                //显示的结果集合
                InlineQueryResult[] results = {
                    new InlineQueryResultArticle(
                        id: "0", // 这个结果的唯一标识符 id
                        title: "说绒话",
                        inputMessageContent: new InputTextMessageContent(GetRBQSay())),
                    new InlineQueryResultArticle(
                        id: "1",
                        title: "查绒度",
                        inputMessageContent: new InputTextMessageContent(GetRBQPoint(inlineQuery.From.Id))),
                    new InlineQueryResultArticle(
                        id: "2",
                        title: "查询口塞列表",
                        inputMessageContent: new InputTextMessageContent(GetAllGag())),
                    new InlineQueryResultArticle(
                        id: "3",
                        title: "使用说明",
                        inputMessageContent: new InputTextMessageContent(GetUsageHelp))
                };

                botClient.AnswerInlineQueryAsync(
                    inlineQueryId: inlineQuery.Id,
                    results: results,
                    isPersonal: true, // 必须为 true,不然三个个人同时查询结果会变成同一个
                    cacheTime: 0); // 返回的inline查询结果在服务器最长保存时间（单位：秒），默认300秒
            }
        }

        /// <summary>选择内联结果接收</summary>
        /// <param name="botClient"></param>
        /// <param name="chosenInlineResult"></param>
        /// <returns></returns>
        //private static Task BotOnChosenInlineResultReceived(ITelegramBotClient botClient, ChosenInlineResult chosenInlineResult)
        //{
        //    Console.WriteLine($"Received inline result: {chosenInlineResult.ResultId} {chosenInlineResult.InlineMessageId}");
        //    return Task.CompletedTask;
        //}

        private static void ChatMembersAdded(ITelegramBotClient botClient, Message message)
        {
            for (int i = 0; i < message.NewChatMembers.Length; i++)
            {
                if (message.NewChatMembers[i].IsBot != true)
                {
                    var exist = Program.DB.GetRBQExist(message.NewChatMembers[0].Id);
                    if (exist != true) Program.DB.AddRBQ(message.NewChatMembers[0].Id, 0, false);

                    exist = Program.DB.GetRBQStatusExist(message.Chat.Id, message.NewChatMembers[i].Id);
                    if (exist != true) Program.DB.AddRBQStatus(message.Chat.Id, message.NewChatMembers[i].Id, 0, false);

                    InlineKeyboardMarkup inlineKeyboard = new(new[] {
                        new [] {
                            InlineKeyboardButton.WithCallbackData(text: "我不可爱,别验证我", callbackData: "kickme"),
                            InlineKeyboardButton.WithCallbackData(text: "我很可爱,请验证我", callbackData: "verifyme"),
                        },
                        new [] {
                            InlineKeyboardButton.WithCallbackData(text: "管理通过", callbackData: "adminverify"),
                            InlineKeyboardButton.WithCallbackData(text: "管理踢出", callbackData: "adminkick"),
                        },
                    });

                    var result = botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"欢迎 <a href=\"tg://user?id={message.NewChatMembers[i].Id}\">新绒布球</a> 加入!\n请发送 「<code>/verify 我很可爱</code>」 来完成加群验证,否则您将会在120秒后被移出群组.",
                        parseMode: ParseMode.Html,
                        replyMarkup: inlineKeyboard,
                        disableNotification: true).Result;

                    var b = new WaitBan(message.Chat.Id, message.From.Id, result.MessageId, 120000, Program.BanList, botClient);
                    Program.BanList.TryAdd(message.From.Id, b);
                }
                else
                {
                    if (message.NewChatMembers[i].Id != botClient.BotId)
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"欢迎 <a href=\"tg://user?id={message.NewChatMembers[i].Id}\">新同类</a> 加入!",
                            parseMode: ParseMode.Html,
                            disableNotification: true);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "绒布球管理器已入驻本群!",
                            parseMode: ParseMode.Html,
                            disableNotification: true);
                    }
                }
            }
        }

        private static void ChatMemberLeft(ITelegramBotClient botClient, Message message)
        {
            if (message.LeftChatMember.IsBot != true)
            {
                Program.DB.DelRBQStatus(message.Chat.Id, message.LeftChatMember.Id);
                botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"一只 <a href=\"tg://user?id={message.LeftChatMember.Id}\">绒布球</a> 离开了群组.",
                    parseMode: ParseMode.Html,
                    disableNotification: true);
            }
            else
            {
                if (message.LeftChatMember.Id != botClient.BotId)
                {
                    botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"一只 <a href=\"tg://user?id={message.LeftChatMember.Id}\">同类</a> 离开了群组.",
                    parseMode: ParseMode.Html,
                    disableNotification: true);
                }
                else
                {
                    Console.WriteLine($"本Bot被 UserId:{message.From.Id} {message.From.Username} {message.From.FirstName}{message.From.LastName} 丢出 GroupId:{message.Chat.Id} {message.Chat.Username} {message.Chat.Title}");
                }
            }
        }

        private static void PrivateMsgProcess(ITelegramBotClient botClient, Message message)
        {
            //switch (message.Text.ToLower())
            //{
            //    case "/gag on":
            //        break;
            //    case "/gag off":
            //        break;
            //    case "/help":
            //        Help(botClient, message);
            //        break;
            //    case "/rbqpoint":
            //        break;
            //    default:
            //        botClient.SendTextMessageAsync(
            //            chatId: message.Chat.Id,
            //            text: "命令错误! 请输入 /help 查看命令!",
            //            disableNotification: true,
            //            replyToMessageId: message.MessageId);
            //        break;
            //}

            //static void Help(ITelegramBotClient botClient, Message message)
            //{
            //    botClient.SendTextMessageAsync(
            //            chatId: message.Chat.Id,
            //            text:
            //            "======Bot功能======\n" +
            //            "这是一项娱乐功能, 可以让群里指定的用户暂时只能发送包含指定字符的消息.\n" +
            //            "======命令列表======\n" +
            //            "/gag 口塞名称 - 给自己「佩戴」口塞. 对用户发送的消息回复会给对方「佩戴」口塞.\n" +
            //            "/gag on - 允许其他人给自己「佩戴」口塞，为了方式骚扰默认是不允许的.\n" +
            //            "/gag off - 不允许其他人给自己「佩戴」口塞，如果已经处于「佩戴」状态，不会影响当前已经「佩戴」中的口塞.\n" +
            //            "/gag - 显示此帮助.\n" +
            //            "/rbqpoint - 查询自己的「绒度」. 对用户发送的消息回复会查询对方的绒度.\n" +
            //            "/about - 查看有关本 Bot 本身的相关信息 (如介绍、隐私权、许可、反馈等)\n" +
            //            "======作用范围======\n" +
            //            "「绒度」- 在本 Bot 所在的任何群组通用\n" +
            //            "「开关」- 在本 Bot 所在的任何群组通用,仅对口塞功能有效.\n" +
            //            "「口塞」- 仅在当前群组适用. 如果 10分钟 没有「挣扎」或者「加固」操作, 将会自动解除. 若有「挣扎」或者「加固」操作则会重新计时.\n" +
            //            "备注: 本群首席绒布球可能会被强制保持「开关」启用.",
            //            disableNotification: true,
            //            replyToMessageId: message.MessageId);
            //}
        }

        private static void MsgProcess(ITelegramBotClient botClient, Message message)
        {
            if (Program.DB.GetAllowGroupExist(message.Chat.Id) == false) Program.DB.AddAllowGroup(message.Chat.Id);

            if (Program.DB.GetAllowGroupExist(message.Chat.Id)) // 消息进入
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
                                text: $"验证失败! <a href=\"tg://user?id={message.From.Id}\">您</a> 还有 {3-ban.FailCount} 次验证机会.",
                                parseMode: ParseMode.Html,
                                disableNotification: false).Result;

                            Program.BanList.TryAdd(message.From.Id, ban);
                        }
                        else
                        {
                            ban.Stop();

                            //botClient.BanChatMemberAsync(message.Chat.Id, message.From.Id);
                            botClient.UnbanChatMemberAsync(message.Chat.Id, message.From.Id);

                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.DeleteMessageAsync(message.Chat.Id, ban.CallbackMsgId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"由于多次验证失败! <a href=\"tg://user?id={message.From.Id}\">Ta</a> 已被移出本群.",
                                parseMode: ParseMode.Html,
                                disableNotification: true);
                        }
                    }
                    else
                    {
                        ban.Stop();

                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.DeleteMessageAsync(message.Chat.Id, ban.CallbackMsgId);
                        botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: $"恭喜这只 <a href=\"tg://user?id={message.From.Id}\">绒布球</a> 验证通过!",
                            parseMode: ParseMode.Html,
                            disableNotification: true);
                    }
                }

                if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0, false); // 检查是否注册RBQ的全局信息
                if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false); // 检查是否注册RBQ的群组状态信息

                
                //Program.DB.SetRBQStatus(message.Chat.Id, message.From.Id, 1, false, 1, DateTime.UtcNow.AddHours(8).Ticks, new long[] { message.From.Id }); // 迫害所有人 （调试用）
                var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id); // 获取RBQ状态
                if (rbq != null)
                {
                    var time = new DateTime(rbq.StartLockTime).AddMinutes(10);
                    if (rbq.LockCount > 0 && DateTime.UtcNow.AddHours(8) < time) // 有锁定次数并在时间内
                    {
                        if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false) // 检查内存队列中没有RBQ 并添加进内存队列
                        {
                            var timeout = (time - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                            var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout, Program.List);
                            Program.List.TryAdd(rbq.Id, rbqx);
                        }
                        else
                        {
                            rbqItem.Id = rbq.Id;
                            rbqItem.GagId = rbq.GagId;
                            rbqItem.StartLockTime = rbq.StartLockTime;
                            rbqItem.LockCount = rbq.LockCount;
                            rbqItem.ResetTimer();
                            Program.List.TryAdd(rbq.Id, rbqItem);
                        }

                        // 对RBQ进行输入合规检查
                        if (TypeProcess(message.Text) > 0) // 不合规
                        { // 如果不符合要求删除消息并嘲讽
                            var R = new Random();
                            if (R.Next(0, 100) >= 70)
                            {
                                rbq.LockCount++;
                                rbq.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                                Program.DB.SetRBQStatus(rbq);
                                if (Program.List.TryRemove(rbq.Id, out RBQList rbqI)) // TryUpdate不好用,就直接删除添加了
                                {
                                    rbqI.LockCount++;
                                    rbqI.StartLockTime = rbq.StartLockTime;
                                    rbqI.ResetTimer();

                                    Program.List.TryAdd(rbq.Id, rbqI);
                                }

                                Program.DB.SetRBQPointAdd1(rbq.RBQId);

                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: $"这只 <a href=\"tg://user?id={message.From.Id}\">绒布球</a> 试图逃脱口塞的限制!\n作为惩罚我们增加了1点它需要挣扎的次数",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                            }
                            else
                            {
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    //text: $"这只 <a href=\"tg://user?id={message.From.Id}\">绒布球</a> 试图逃脱口塞的限制!",
                                    text: $"这只 <a href=\"tg://user?id={message.From.Id}\">绒布球</a> 试图逃脱口塞的限制!",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                            }
                        }
                        else // 合规 并进行挣扎计数-1 并重置时间
                        {
                            if (rbq.LockCount-1 > -1 && rbq.LockCount-1 < 1) // 这么写防止管理员重置次数后次数越界(可能会存在的bug的处理)
                            {
                                rbq.LockCount = 0;
                                rbq.StartLockTime = DateTime.MinValue.Ticks;
                                rbq.FromId = null;
                                rbq.GagId = 0;
                                Program.DB.SetRBQStatus(rbq);
                                Program.DB.SetRBQPointAdd1(rbq.RBQId);

                                Program.List.TryRemove(rbq.Id, out _); // 语法糖，直接不用那个单位就写个下划线 _ 就丢弃了
                            }
                            else if (rbq.LockCount-1 > 0)
                            {
                                rbq.LockCount--;
                                rbq.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                                Program.DB.SetRBQStatus(rbq);

                                if (Program.List.TryRemove(rbq.Id, out RBQList rbqI)) // TryUpdate不好用,就直接删除添加了
                                {
                                    rbqI.LockCount--;
                                    rbqI.StartLockTime = rbq.StartLockTime;
                                    rbqI.ResetTimer();
                                    Program.DB.SetRBQPointAdd1(rbq.RBQId);

                                    Program.List.TryAdd(rbq.Id, rbqI);
                                }
                            }
                        }
                    }
                    else // 无口塞无计时器
                    {
                        if (message.Text[0] == '/') CommandProcess(botClient, message);
                    }
                }
            }
            else
            {
                botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"请联系 <a href=\"tg://user?id=1324338125\">管理员</a> 允许启用本机器人!",
                        parseMode: ParseMode.Html,
                        disableNotification: true);
            }
        }

        private static void StickerProcess(ITelegramBotClient botClient, Message message)
        {
            if (Program.DB.GetAllowGroupExist(message.Chat.Id)) // 消息进入
            {
                if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0, false); // 检查是否注册RBQ的全局信息
                if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false); // 检查是否注册RBQ的群组状态信息

                var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id);
                if (rbq != null)
                {
                    var time = new DateTime(rbq.StartLockTime).AddMinutes(10);
                    if (rbq.LockCount > 0 && DateTime.UtcNow.AddHours(8) < time)
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    //text: $"这只 <a href=\"tg://user?id={message.From.Id}\">绒布球</a> 试图逃脱口塞的限制!",
                                    text: $"这只 <a href=\"tg://user?id={message.From.Id}\">绒布球</a> 试图逃脱口塞的限制!",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                    }
                }
            }
        }
        #endregion

        private static int TypeProcess(string msg)
        {
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

        private static void RBQPointProcess(ITelegramBotClient botClient, Message message)
        {
            if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0, false);
            if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

            if (message.ReplyToMessage != null)
            {
                if (Program.DB.GetRBQExist(message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQ(message.ReplyToMessage.From.Id, 0, false);
                if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id, 0, false);

                var point = Program.DB.GetRBQPoint(message.ReplyToMessage.From.Id);

                botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: $"<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">Ta</a> 的绒度为 {point}",
                        parseMode: ParseMode.Html,
                        disableNotification: true);
            }
            else
            {
                var point = Program.DB.GetRBQPoint(message.From.Id);

                botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        replyToMessageId: message.MessageId,
                        text: $"<a href=\"tg://user?id={message.From.Id}\">您</a> 的绒度为 {point}",
                        parseMode: ParseMode.Html,
                        disableNotification: true);
            }
        }

        private static async void CommandProcess(ITelegramBotClient botClient, Message message)
        {
            var comm = message.Text.ToLower().Split(' ');
            switch (comm[0])
            {
                case "/gag":
                    GagProcess(botClient, message, comm);
                    break;
                case "/rbqpoint":
                    RBQPointProcess(botClient, message);
                    break;
                case "/verify":
                    break;
                case "/ping":
                    break;
                case "/about":
                    break;
                default:
                    InlineKeyboardMarkup inlineKeyboard = new(new[] {
                        new [] {
                            InlineKeyboardButton.WithCallbackData(text: "我不可爱,别验证我", callbackData: "kickme"),
                            InlineKeyboardButton.WithCallbackData(text: "我很可爱,请验证我", callbackData: "verifyme"),
                        },
                        new [] {
                            InlineKeyboardButton.WithCallbackData(text: "管理通过", callbackData: "adminverify"),
                            InlineKeyboardButton.WithCallbackData(text: "管理踢出", callbackData: "adminkick"),
                        },
                    });
                    var result = botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "test",
                        disableNotification: true,
                        replyToMessageId: message.MessageId,
                        replyMarkup: inlineKeyboard).Result;

                    var b = new WaitBan(message.Chat.Id, message.From.Id, result.MessageId, 120000, Program.BanList, botClient);
                    Program.BanList.TryAdd(message.From.Id, b);
                    break;
            }
        }

        private static void DelayDeleteMessage(ITelegramBotClient botClient,long chatId, int msgId, int delay)
        {
            Thread.Sleep(delay);
            botClient.DeleteMessageAsync(chatId, msgId);
        }

        private static void GagProcess(ITelegramBotClient botClient, Message message, string[] comm)
        {
            if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0, false);
            if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

            #region 对别人回复命令
            if (message.ReplyToMessage != null)
            {
                if (Program.DB.GetRBQExist(message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQ(message.ReplyToMessage.From.Id, 0, false);
                if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.ReplyToMessage.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id, 0, false);
                #region 如果命令长度 > 1 是有开关/给别人加口塞
                if (comm.Length > 1)
                {
                    #region 如果是任意使用开关
                    if (comm[1] == "on" || comm[1] == "off")
                    {
                        if (message.From.Id != 1324338125)
                        {
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            var result = botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"<a href=\"tg://user?id={message.From.Id}\">你</a> 不是管理员!不能那么做!",
                                parseMode: ParseMode.Html,
                                disableNotification: true).Result;

                            ThreadPool.QueueUserWorkItem(o => { DelayDeleteMessage(botClient, message.Chat.Id, result.MessageId, 5000); });
                        }
                        else
                        {
                            if (comm[1] == "on")
                            {
                                Program.DB.SetRBQUsed(message.Chat.Id, message.ReplyToMessage.From.Id, true);
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: $"<a href=\"tg://user?id=1324338125\">管理员</a> 已将 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 变为公用球!",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                            }
                            else
                            {
                                Program.DB.SetRBQUsed(message.Chat.Id, message.ReplyToMessage.From.Id, false);
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: $"<a href=\"tg://user?id=1324338125\">管理员</a> 已禁止 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 被公用!",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                            }
                        }
                    }
                    #endregion
                    #region 否则就是加口塞
                    else
                    {
                        #region 检查口塞不存在
                        if (Program.DB.GetGagItemExist(comm[1]) != true)
                        {
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: $"{comm[1]} 不存在!",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                        }
                        #endregion
                        else
                        {
                            #region 检查是否允许被他人加口塞
                            if (Program.DB.GetRBQAnyUsed(message.Chat.Id, message.ReplyToMessage.From.Id) != true)
                            {
                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        text: $"<a href=\"tg://user?id={message.From.Id}\">你</a> 试图给<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"> 绒布球 </a>使用 {comm[1]} ,但对方拒绝了你的口塞!",
                                        parseMode: ParseMode.Html,
                                        disableNotification: true);
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
                                            text: $"<a href=\"tg://user?id={message.From.Id}\">Ta</a> 试图给<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"> 绒布球 </a>使用 {comm[1]} ,但你已经使用过一次了!",
                                            parseMode: ParseMode.Html,
                                            disableNotification: true);
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
                                                    text: $"<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 已经有一个 {gag.Name} 了!\n<a href=\"tg://user?id={message.From.Id}\">您</a> 可以使用/gag 回复这只绒布球来加固.",
                                                    parseMode: ParseMode.Html,
                                                    disableNotification: true);
                                        }
                                        else
                                        {
                                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                            botClient.SendTextMessageAsync(
                                                    chatId: message.Chat.Id,
                                                    text: $"获取口塞失败,这可能是因为最近修改了数据库导致无法找到这个口塞.",
                                                    parseMode: ParseMode.Html,
                                                    disableNotification: true);
                                        }
                                    }
                                    #endregion
                                    else
                                    {
                                        var gag = Program.DB.GetGagItemInfo(comm[1]);

                                        #region 不是管理员上口塞
                                        if (message.From.Id != 1324338125)
                                        {
                                            if (Program.DB.GetRBQPoint(message.ReplyToMessage.From.Id) < gag.LimitPoint)
                                            {
                                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                botClient.SendTextMessageAsync(
                                                    chatId: message.Chat.Id,
                                                    text: $"<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 的绒度不够使用 {gag.Name}!",
                                                    parseMode: ParseMode.Html,
                                                    disableNotification: true);
                                            }
                                            else
                                            {
                                                var R = new Random();
                                                var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                                                var tm = DateTime.UtcNow.AddHours(8);

                                                Program.DB.AddRBQFroms(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id);

                                                rbq.StartLockTime = tm.Ticks;
                                                rbq.LockCount = gag.UnLockCount;
                                                rbq.GagId = gag.Id;
                                                Program.DB.SetRBQStatus(rbq);

                                                var msg = $"<a href=\"tg://user?id={message.From.Id}\">Ta</a> 帮 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 带上了 {comm[1]}!\n{gag.LockMsg[R.Next(0, gag.LockMsg.Length)]}";

                                                if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false) // 检查内存队列中没有RBQ 并添加进内存队列
                                                {
                                                    var timeout = (tm.AddMinutes(10) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                                    var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout, Program.List);
                                                    Program.List.TryAdd(rbq.Id, rbqx);
                                                }
                                                else
                                                {
                                                    rbqItem.Id = rbq.Id;
                                                    rbqItem.GagId = gag.Id;
                                                    rbqItem.StartLockTime = tm.Ticks;
                                                    rbqItem.LockCount = gag.UnLockCount;
                                                    rbqItem.ResetTimer();
                                                    Program.List.TryAdd(rbq.Id, rbqItem);
                                                }

                                                botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                                botClient.SendTextMessageAsync(
                                                        chatId: message.Chat.Id,
                                                        text: msg,
                                                        parseMode: ParseMode.Html,
                                                        disableNotification: true);
                                            }
                                        }
                                        #endregion
                                        #region 是管理员上口塞
                                        else
                                        {
                                            var R = new Random();
                                            var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                                            var tm = DateTime.UtcNow.AddHours(8);

                                            //Program.DB.AddRBQFroms(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id);

                                            rbq.StartLockTime = tm.Ticks;
                                            rbq.LockCount = gag.UnLockCount;
                                            rbq.GagId = gag.Id;
                                            Program.DB.SetRBQStatus(rbq);

                                            var msg = $"<a href=\"tg://user?id={message.From.Id}\">管理员</a> 帮 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 带上了 {comm[1]}!\n{gag.LockMsg[R.Next(0, gag.LockMsg.Length)]}";

                                            if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false) // 检查内存队列中没有RBQ 并添加进内存队列
                                            {
                                                var timeout = (tm.AddMinutes(10) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                                var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout, Program.List);
                                                Program.List.TryAdd(rbq.Id, rbqx);
                                            }
                                            else
                                            {
                                                rbqItem.Id = rbq.Id;
                                                rbqItem.GagId = gag.Id;
                                                rbqItem.StartLockTime = tm.Ticks;
                                                rbqItem.LockCount = gag.UnLockCount;
                                                rbqItem.ResetTimer();
                                                Program.List.TryAdd(rbq.Id, rbqItem);
                                            }

                                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                            botClient.SendTextMessageAsync(
                                                    chatId: message.Chat.Id,
                                                    text: msg,
                                                    parseMode: ParseMode.Html,
                                                    disableNotification: true);
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
                #region 命令长度 < 1 是给别人加固
                else
                {
                    #region 被加固检查
                    if (Program.DB.GetRBQFromExits(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id) == true)
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"<a href=\"tg://user?id={message.From.Id}\">您</a> 已经给<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\"> 绒布球 </a>加固过了,就放过Ta吧.",
                                parseMode: ParseMode.Html,
                                disableNotification: true);
                    }
                    #endregion
                    else
                    {
                        Program.DB.AddRBQFroms(message.Chat.Id, message.ReplyToMessage.From.Id, message.From.Id);

                        var R = new Random();
                        var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.ReplyToMessage.From.Id);
                        var gag = Program.DB.GetGagItemInfo(rbq.GagId);
                        var tm = DateTime.UtcNow.AddHours(8);

                        rbq.StartLockTime = tm.Ticks;
                        rbq.LockCount = gag.UnLockCount;
                        rbq.GagId = gag.Id;
                        Program.DB.SetRBQStatus(rbq);

                        if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false) // 检查内存队列中没有RBQ 并添加进内存队列
                        {
                            var timeout = (tm.AddMinutes(10) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                            var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout, Program.List);
                            Program.List.TryAdd(rbq.Id, rbqx);
                        }
                        else
                        {
                            rbqItem.Id = rbq.Id;
                            rbqItem.GagId = gag.Id;
                            rbqItem.StartLockTime = tm.Ticks;
                            rbqItem.LockCount = gag.UnLockCount;
                            rbqItem.ResetTimer();
                            Program.List.TryAdd(rbq.Id, rbqItem);
                        }

                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"<a href=\"tg://user?id={message.From.Id}\">Ta</a> 帮 <a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> {gag.EnhancedLockMsg[R.Next(0, gag.EnhancedLockMsg.Length)]}",
                                parseMode: ParseMode.Html,
                                disableNotification: true);
                    }
                }
                #endregion
            }
            #endregion

            #region 自我开关与口塞
            else
            {
                if (Program.DB.GetRBQExist(message.From.Id) != true) Program.DB.AddRBQ(message.From.Id, 0, false);
                if (Program.DB.GetRBQStatusExist(message.Chat.Id, message.From.Id) != true) Program.DB.AddRBQStatus(message.Chat.Id, message.From.Id, 0, false);

                if (comm.Length > 1)
                {
                    if (comm[1] == "on")
                    {
                        Program.DB.SetRBQUsed(message.Chat.Id, message.From.Id, true);

                        botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: $"<a href=\"tg://user?id={message.From.Id}\">您</a>可以被公开调教了!",
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                    }
                    if (comm[1] == "off")
                    {
                        Program.DB.SetRBQUsed(message.Chat.Id, message.From.Id, false);

                        botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        text: $"<a href=\"tg://user?id={message.From.Id}\">您</a>不能再被公开调教了!",
                                        parseMode: ParseMode.Html,
                                        disableNotification: true);
                    }

                    if (Program.DB.GetGagItemExist(comm[1]) != true)
                    {
                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"{comm[1]} 不存在!",
                                parseMode: ParseMode.Html,
                                disableNotification: true);
                    }
                    else
                    {
                        var gag = Program.DB.GetGagItemInfo(comm[1]);

                        if (Program.DB.GetRBQPoint(message.From.Id) < gag.LimitPoint)
                        {
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                chatId: message.Chat.Id,
                                text: $"<a href=\"tg://user?id={message.ReplyToMessage.From.Id}\">绒布球</a> 的绒度不够使用 {gag.Name}!",
                                parseMode: ParseMode.Html,
                                disableNotification: true);
                        }
                        else
                        {
                            var R = new Random();
                            var rbq = Program.DB.GetRBQStatus(message.Chat.Id, message.From.Id);
                            var tm = DateTime.UtcNow.AddHours(8);

                            Program.DB.AddRBQFroms(message.Chat.Id, message.From.Id, message.From.Id);

                            rbq.StartLockTime = tm.Ticks;
                            rbq.LockCount = gag.UnLockCount;
                            rbq.GagId = gag.Id;
                            Program.DB.SetRBQStatus(rbq);

                            var msg = $"<a href=\"tg://user?id={message.From.Id}\">绒布球</a> 给自己带上了 {comm[1]}!\n{gag.LockMsg[R.Next(0, gag.LockMsg.Length)]}";

                            if (Program.List.TryRemove(rbq.Id, out RBQList rbqItem) == false) // 检查内存队列中没有RBQ 并添加进内存队列
                            {
                                var timeout = (tm.AddMinutes(10) - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
                                var rbqx = new RBQList(rbq.Id, rbq.LockCount, rbq.GagId, timeout, Program.List);
                                Program.List.TryAdd(rbq.Id, rbqx);
                            }
                            else
                            {
                                rbqItem.Id = rbq.Id;
                                rbqItem.GagId = gag.Id;
                                rbqItem.StartLockTime = tm.Ticks;
                                rbqItem.LockCount = gag.UnLockCount;
                                rbqItem.ResetTimer();
                                Program.List.TryAdd(rbq.Id, rbqItem);
                            }

                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                            botClient.SendTextMessageAsync(
                                    chatId: message.Chat.Id,
                                    text: msg,
                                    parseMode: ParseMode.Html,
                                    disableNotification: true);
                        }
                    }
                }
                else
                {
                    botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                    botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: $"参数不够!请重新输入!",
                        parseMode: ParseMode.Html,
                        disableNotification: true);
                }
            }
            #endregion
        }
    }
}