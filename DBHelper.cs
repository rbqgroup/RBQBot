using LiteDB;
using RBQBot.Model;
using System;
using System.Collections.Generic;

namespace RBQBot
{
    public sealed class DBHelper
    {
        public DBHelper() {
            InitDB();
        }
        public static readonly DBHelper Instance = new DBHelper();

        private ILiteDatabase db = null;

        private ILiteCollection<AllowGroup> allowGroupCol = null;
        private ILiteCollection<GagItem> gagItemCol = null;
        private ILiteCollection<RBQ> rbqCol = null;
        private ILiteCollection<RBQStatus> rbqStatusCol = null;

        /// <summary>
        /// [绒布球] 给自己带上了默认口塞! 咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!
        /// </summary>
        private readonly string[] defaultSelfLockMsg = { "咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!" };

        /// <summary>
        /// [Ta] 帮 [绒布球] 带上了默认口塞! 顺便在 Ta 身上画了一个正字~
        /// </summary>
        private readonly string[] defaultLockMsg = { "顺便在 Ta 身上画了一个正字~" };

        /// <summary>
        /// [Ta] 帮 [绒布球] 修好了口塞!\n顺便展示了钥匙并丢到了一边!
        /// </summary>
        private readonly string[] defaultEnhancedLockMsg = { "修好了口塞!\n顺便展示了钥匙并丢到了一边!" };

        /// <summary>
        /// [绒布球] 挣脱了被人们安装的 超大号默认口塞! Ta感觉自己可以容纳更大的尺寸了呢!
        /// </summary>
        private readonly string[] defaultUnlockMsg = { "Ta感觉自己可以容纳更大的尺寸了呢!" };

        private void InitDB()
        {
            db = new LiteDatabase(new ConnectionString() {
                Filename = $"{AppDomain.CurrentDomain.BaseDirectory}database.db",
                Connection = ConnectionType.Direct,
                Password = null,
                InitialSize = 0,
                ReadOnly = false,
                Upgrade = false,
                Collation = new Collation("zh-CN/None") // 使用区域化 和 比较字符串时 不忽略任何字符去比较 http://www.litedb.org/docs/collation/
            });

            allowGroupCol = db.GetCollection<AllowGroup>("GroupList");
            gagItemCol = db.GetCollection<GagItem>("GagList");
            rbqCol = db.GetCollection<RBQ>("RBQList");
            rbqStatusCol = db.GetCollection<RBQStatus>("RBQStatusList");
        }

        #region Group Operate
        public int GetGroupCount() => allowGroupCol.Count();

        public void AddAllowGroup(long groupId)
        {
            var obj = new AllowGroup(groupId);
            allowGroupCol.Insert(obj);
        }

        public void DelAllowGroup(long groupId)
        {
            var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
            if (result != null) allowGroupCol.Delete(result.Id);
        }

        public bool GetAllowGroupExist(long groupId)
        {
            var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
            if (result != null) return true;
            return false;
        }

        //public long[] GetAllGroupId()
        //{
        //    long[] l = new long[allowGroupCol.Count()];
        //    var result = allowGroupCol.FindAll();
        //    if (result != null)
        //    {
        //        var c = 0;
        //        foreach (var i in result)
        //        {
        //            l[c] = i.GroupId;
        //            c++;
        //        }
        //        return l;
        //    }
        //    return null;
        //}

        //public IEnumerable<AllowGroup> GetGroup(int startId, int stopId)
        //{
        //    var results = allowGroupCol.Find(x => x.Id >= startId && x.Id <= stopId);
        //    if (results != null) return results;
        //    return null;
        //}
        #endregion

        #region GagItem Operate
        public int GetGagItemCount() => gagItemCol.Count();

        public void AddGagItem(string gagName, long limitPoint, long unLockCount, bool showLimit, bool showUnlock, string[] selfLockMsg = null, string[] lockMsg = null, string[] enhancedLockMsg = null, string[] unlockMsg = null)
        {
            if (selfLockMsg == null) selfLockMsg = defaultSelfLockMsg;
            if (lockMsg == null) lockMsg = defaultLockMsg;
            if (enhancedLockMsg == null) enhancedLockMsg = defaultEnhancedLockMsg;
            if (unlockMsg == null) unlockMsg = defaultUnlockMsg;

            var obj = new GagItem()
            {
                Name = gagName,
                LimitPoint = limitPoint,
                UnLockCount = unLockCount,
                SelfLockMsg = selfLockMsg,
                LockMsg = lockMsg,
                EnhancedLockMsg = enhancedLockMsg,
                UnLockMsg = unlockMsg,
                ShowLimit = showLimit,
                ShowUnlock = showUnlock
            };
            gagItemCol.Insert(obj);
        }

        public void DelGagItem(string gagName)
        {
            var result = gagItemCol.FindOne(x => x.Name == gagName);
            if (result != null) gagItemCol.Delete(result.Id);
        }

        public System.Collections.Generic.IEnumerable<GagItem> GetAllGagItems() => gagItemCol.FindAll();

        public bool GetGagItemExist(string gagName)
        {
            var result = gagItemCol.FindOne(x => x.Name == gagName);
            if (result != null) return true;
            return false;
        }

        public GagItem GetGagItemInfo(string gagName)
        {
            var result = gagItemCol.FindOne(x => x.Name == gagName);
            if (result != null) return result;
            return null;
        }

        public GagItem GetGagItemInfo(int id)
        {
            var result = gagItemCol.FindOne(x => x.Id == id);
            if (result != null) return result;
            return null;
        }

        public void SetGagItem(string gagName, long limitPoint, long unLockCount, string[] selfLockMsg = null, string[] lockMsg = null, string[] enhancedLockMsg = null, string[] unlockMsg = null)
        {
            var result = gagItemCol.FindOne(x => x.Name == gagName);
            if (result != null)
            {
                result.LimitPoint = limitPoint;
                result.UnLockCount = unLockCount;
                result.SelfLockMsg = selfLockMsg;
                result.LockMsg = lockMsg;
                result.EnhancedLockMsg = enhancedLockMsg;
                result.UnLockMsg = unlockMsg;
                gagItemCol.Update(result);
            }
        }

        public void SetGagItem(GagItem item) => gagItemCol.Update(item);
        #endregion

        #region RBQ Operate
        public int GetRBQCount() => rbqCol.Count();

        public void AddRBQ(long telegramId, long rbqPoint, bool fastInline)
        {
            var obj = new RBQ()
            {
                TelegramId = telegramId,
                RBQPoint = rbqPoint,
                FastInline = fastInline
            };
            rbqCol.Insert(obj);
        }

        public void DelRBQ(long telegramId)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null) rbqCol.Delete(result.Id);
        }

        public bool GetRBQExist(long telegramId)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null) return true;
            return false;
        }

        public RBQ GetRBQInfo(long telegramId)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null) return result;
            return null;
        }

        public long GetRBQPoint(long telegramId)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null) return result.RBQPoint;
            return -1;
        }

        public void SetRBQInfo(long telegramId, long rbqPoint, bool fastInline)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null)
            {
                result.RBQPoint = rbqPoint;
                result.FastInline = fastInline;
                rbqCol.Update(result);
            }
        }

        public void SetRBQPointAdd(long telegramId, long addRbqPoint)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null)
            {
                result.RBQPoint = result.RBQPoint + addRbqPoint;
                rbqCol.Update(result);
            }
        }

        public void SetRBQPointAdd1(long telegramId)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null)
            {
                result.RBQPoint++;
                rbqCol.Update(result);
            }
        }

        public void SetRBQPointDel(long telegramId, long delRbqPoint)
        {
            var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
            if (result != null)
            {
                result.RBQPoint = result.RBQPoint - delRbqPoint;
                rbqCol.Update(result);
            }
        }

        public void SetRBQInfo(RBQ rbq) => rbqCol.Update(rbq);

        #endregion

        #region RBQStatus Operate
        public int GetRBQStatusCount() => rbqStatusCol.Count();

        public bool AddRBQFroms(long groupId, long rbqId, long userId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null)
            {
                if (result.FromId == null)
                {
                    result.FromId = new long[] { userId };
                    rbqStatusCol.Update(result);
                    return true;
                }
                else
                {
                    if (result.FromId.Length == 0)
                    {
                        result.FromId = new long[] { userId };
                        rbqStatusCol.Update(result);
                        return true;
                    }
                    else
                    {
                        long[] a = new long[result.FromId.Length + 1];
                        Array.Copy(result.FromId, 0, a, 0, result.FromId.Length);
                        a[result.FromId.Length] = userId;
                        result.FromId = a;
                        rbqStatusCol.Update(result);
                        return true;
                    }
                }
            } return false;
        }

        public void AddRBQStatus(long groupId, long rbqId, long lockCount = 0, bool anyUsed = false)
        {
            var obj = new RBQStatus()
            {
                GroupId = groupId,
                RBQId = rbqId,
                LockCount = lockCount,
                AnyUsed = anyUsed,
                StartLockTime = DateTime.UnixEpoch.Ticks,
            };
            rbqStatusCol.Insert(obj);
        }

        public void DelRBQStatus(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) rbqStatusCol.Delete(result.Id);
        }

        public IEnumerable<RBQStatus> GetAllRBQStatus() => rbqStatusCol.FindAll();

        //public IEnumerable<RBQStatus> GetRBQStatusById(long rbqId)
        //{
        //    var result = rbqStatusCol.Find(x => x.RBQId == rbqId);
        //    if (result != null)
        //    {
        //        return result;
        //    } return null;
        //}

        public bool GetRBQFromExits(long groupId, long rbqId, long userId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null)
            {
                if (result.FromId == null) return false;

                for (int i = 0; i < result.FromId.Length; i++)
                {
                    if (result.FromId[i] == userId) return true;
                }
            } return false;
        }

        public int GetRBQGagId(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return result.GagId;
            return 0;
        }

        public long GetRBQLockCount(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return result.LockCount;
            return 0;
        }

        public bool GetRBQStatusExist(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return true;
            return false;
        }

        public bool GetRBQCanLock(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null)
            {
                var rbqP = GetRBQPoint(rbqId);
                var cd = 0;
                switch (rbqP)
                {
                    case <= 100: cd = 60; break;
                    case <= 200: cd = 40; break;
                    case <= 500: cd = 25; break;
                    case <= 1000: cd = 15; break;
                    case > 1000: cd = 10; break;
                    default: cd = 60; break;
                }
                // 锁定时间+超时时间+CD < 当前时间
                return new DateTime(result.StartLockTime).AddSeconds(cd) < DateTime.UtcNow.AddHours(8);
            }
            return false;
        }

        public bool GetRBQAnyUsed(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return result.AnyUsed;
            return false;
        }

        public long[] GetRBQFromId(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return result.FromId;
            return null;
        }

        public RBQStatus GetRBQStatus(int id)
        {
            var result = rbqStatusCol.FindOne(x => x.Id == id);
            if (result != null) return result;
            return null;
        }

        public RBQStatus GetRBQStatus(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return result;
            return null;
        }

        public int GetRBQStatusId(long groupId, long rbqId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null) return result.Id;
            return -1;
        }

        public void SetRBQStatus(long groupId, long rbqId, long lockCount, bool anyUsed, int gagId, long startLockTime, long[] fromId)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null)
            {
                result.LockCount = lockCount;
                result.AnyUsed = anyUsed;
                result.GagId = gagId;
                result.StartLockTime = startLockTime;
                result.FromId = fromId;
                rbqStatusCol.Update(result);
            }
        }

        public void SetRBQUsed(long groupId, long rbqId, bool anyUsed)
        {
            var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
            if (result != null)
            {
                result.AnyUsed = anyUsed;
                rbqStatusCol.Update(result);
            }
        }

        public void SetRBQStatus(RBQStatus rbq) => rbqStatusCol.Update(rbq);

        #endregion
    }
}
