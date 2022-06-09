namespace RBQBot.Model
{
    /// <summary>绒布球信息</summary>
    public class RBQ
    {
        public int Id { get; set; }

        /// <summary>绒布球Id</summary>
        public long TelegramId { get; set; }

        /// <summary>全局绒度</summary>
        public long RBQPoint { get; set; }
    }
}
