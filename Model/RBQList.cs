using System;

namespace RBQBot
{
    /// <summary>封装的自动超时解除口塞</summary>
    public class RBQList
    {
        System.Timers.Timer tm;

        /// <summary>绒布球状态主键Id</summary>
        public int Id { get; set; }
        /// <summary>需要挣脱的次数</summary>
        public long LockCount { get; set; }
        /// <summary>口塞的主键ID</summary>
        public long GagId { get; set; }
        /// <summary>开始口塞的时间</summary>
        public long StartLockTime { get; set; }


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

        public void Stop() => tm.Stop();

        /// <summary>重新计时</summary>
        public void ResetTimer()
        {
            var time = new DateTime(StartLockTime).AddMinutes(Program.LockTime);
            tm.Stop();
            tm.Interval = (time - DateTime.UtcNow.AddHours(8)).TotalMilliseconds;
            tm.Start();
        }

        private void Tm_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var rbq = Program.DB.GetRBQStatus(Id);
            if (rbq != null)
            {
                rbq.LockCount = 0;
                rbq.FromId = new long[0];
                rbq.GagId = 0;
                rbq.StartLockTime = DateTime.UtcNow.AddHours(8).Ticks;
                Program.DB.SetRBQStatus(rbq);
            }

            Program.List.TryRemove(Id, out _);
            Program.DB.SetRBQPointDel(rbq.RBQId, 10);
        }
    }
}
