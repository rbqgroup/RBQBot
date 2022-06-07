namespace RBQBot.Model
{
    /// <summary>口塞物品</summary>
    public class GagItem
    {
        public int Id { get; set; }

        /// <summary>口塞名字</summary>
        public string Name { get; set; }

        /// <summary>需要多少绒度才能使用</summary>
        public long LimitPoint { get; set; }

        /// <summary>解锁需要的挣扎次数</summary>
        public long UnLockCount { get; set; }

        /// <summary>自我佩戴口塞时的消息</summary>
        public string[] SelfLockMsg { get; set; }

        /// <summary>被佩戴口塞时的消息</summary>
        public string[] LockMsg { get; set; }

        /// <summary>加固口塞时的消息</summary>
        public string[] EnhancedLockMsg { get; set; }

        /// <summary>挣脱时的消息</summary>
        public string[] UnLockMsg { get; set; }

        /// <summary>是否显示需要绒度</summary>
        public bool ShowLimit { get; set; }

        /// <summary>是否显示解锁需要的次数</summary>
        public bool ShowUnlock { get; set; }
    }
}
