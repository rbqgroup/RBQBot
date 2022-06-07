using Telegram.Bot;

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
}
