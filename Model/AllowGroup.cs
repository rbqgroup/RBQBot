namespace RBQBot.Model
{

    public enum AllowFunction
    {
        AllowGag = 0,
        AllowVerify = 1,
        AllowMsgCount = 2
    }

    /// <summary>允许使用该Bot的群组</summary>
    public class AllowGroup
    {
        public int Id { get; set; }

        /// <summary>允许的群组Id</summary>
        public long GroupId { get; set; }

        public bool AllowGag { get; set; }

        public bool AllowVerify { get; set; }

        public bool AllowMsgCount { get; set; }

        public string SamplyMsgCountStr { get; set; }

        public AllowGroup(long groupId)
        {
            GroupId = groupId;
        }
    }
}
