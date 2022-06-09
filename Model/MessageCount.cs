namespace RBQBot.Model
{
    public struct MessageCount
    {
        /// <summary>消息计数器主键</summary>
        public int Id { get; set; }
        /// <summary>群组Id</summary>
        public long GroupId { get; set; }
        /// <summary>用户Id</summary>
        public long UserId { get; set; }
        /// <summary>消息数量</summary>
        public int Count { get; set; }

        public MessageCount(int id, long groupId, long userId)
        {
            Id = id;
            GroupId = groupId;
            UserId = userId;
            Count = 1;
        }

        public MessageCount(int id, long groupId, long userId, int count)
        {
            Id = id;
            GroupId = groupId;
            UserId = userId;
            Count = count;
        }
    }

    public struct MsgCount
    {
        public long UserId;
        public long GroupId;
        public int Count;

        public MsgCount(long groupId, long userId, int count)
        {
            UserId = userId;
            GroupId = groupId;
            Count = count;
        }
    }

    public struct MsgCountX
    {
        public long UserId;
        public int Count;

        public MsgCountX(long userId, int count)
        {
            UserId=userId;
            Count=count;
        }
    }
}
