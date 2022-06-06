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

public class RBQList : RBQBot.Model.RBQStatus
{
    System.Timers.Timer tm;
    object obj;

    public RBQList(int id, long lockCount, int gagId, double timeout, object reflection)
    {
        obj = reflection;

        Id = id;
        LockCount = lockCount;
        GagId = gagId;

        tm = new System.Timers.Timer();
        tm.AutoReset = false;
        tm.Interval = timeout;
        tm.Elapsed += Tm_Elapsed;
        tm.Start();
    }

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

        var list = (System.Collections.Concurrent.ConcurrentDictionary<int, RBQList>)obj;
        list.TryRemove(Id, out RBQList x);

        Program.DB.SetRBQPointDel(rbq.RBQId, 10);
    }
}