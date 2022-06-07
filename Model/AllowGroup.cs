namespace RBQBot.Model
{
    /// <summary>允许使用该Bot的群组</summary>
    public class AllowGroup
    {
        public int Id { get; set; }

        /// <summary>允许的群组Id</summary>
        public long GroupId { get; set; }

        public AllowGroup(long groupId)
        {
            GroupId = groupId;
        }
    }
}
