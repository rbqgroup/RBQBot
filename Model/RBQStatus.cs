using RBQBot;
using System;

namespace RBQBot.Model
{
    /// <summary>绒布球在群组的状态</summary>
    public class RBQStatus
    {
        public int Id { get; set; }

        /// <summary>群组Id</summary>
        public long GroupId { get; set; }

        /// <summary>绒布球的Id</summary>
        public long RBQId { get; set; }

        /// <summary>需要挣扎的次数</summary>
        public long LockCount { get; set; }

        /// <summary>任何人使用口塞</summary>
        public bool AnyUsed { get; set; }

        /// <summary>口塞的ID</summary>
        public int GagId { get; set; }

        /// <summary>开始口塞的时间</summary>
        public long StartLockTime { get; set; } // 主线程里开处理队列

        /// <summary>被谁塞口塞的</summary>
        public long[] FromId { get; set; }
    }
}

/// <summary>封装的自动超时解除口塞</summary>
public class RBQList : RBQBot.Model.RBQStatus
{
    System.Timers.Timer tm;

    /// <summary>定时移除口塞</summary>
    /// <param name="id">RBQStatus 主键ID</param>
    /// <param name="lockCount">口塞需要挣扎的次数</param>
    /// <param name="gagId">口塞主键Id</param>
    /// <param name="timeout">超时计时器的时间(毫秒)</param>
    public RBQList(int id, long lockCount, int gagId, double timeout)
    {
        Id = id;
        LockCount = lockCount;
        GagId = gagId;

        tm = new System.Timers.Timer();
        tm.AutoReset = false;
        tm.Interval = timeout;
        tm.Elapsed += Tm_Elapsed;
        tm.Start();
    }

    /// <summary>重新计时</summary>
    public void ResetTimer()
    {
        var time = new DateTime(StartLockTime).AddMinutes(10);
        tm.Stop();
        tm.Interval = (time - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
        tm.Start();
    }

    private void Tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        var rbq = Program.DB.GetRBQStatus(Id);
        rbq.LockCount = 0;
        rbq.StartLockTime = DateTime.MinValue.Ticks;
        rbq.FromId = null;
        rbq.GagId = 0;
        Program.DB.SetRBQStatus(rbq);

        Program.List.TryRemove(Id, out _);

        Program.DB.SetRBQPointDel(rbq.RBQId, 10);
    }
}