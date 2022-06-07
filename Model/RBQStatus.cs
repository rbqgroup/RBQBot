namespace RBQBot.Model
{
    /// <summary>绒布球在群组的状态</summary>
    public class RBQStatus
    {
        /// <summary>绒布球状态主键Id</summary>
        public int Id { get; set; }

        /// <summary>群组Id</summary>
        public long GroupId { get; set; }

        /// <summary>绒布球的Id</summary>
        public long RBQId { get; set; }

        /// <summary>需要挣扎的次数</summary>
        public long LockCount { get; set; }

        /// <summary>任何人使用口塞</summary>
        public bool AnyUsed { get; set; }

        /// <summary>口塞的主键ID</summary>
        public int GagId { get; set; }

        /// <summary>开始口塞的时间</summary>
        public long StartLockTime { get; set; }

        /// <summary>被谁塞口塞的</summary>
        public long[] FromId { get; set; }
    }
}
