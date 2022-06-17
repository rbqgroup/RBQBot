using LiteDB;
using RBQBot.Model;
using System;
using System.Collections.Generic;

namespace RBQBot
{
    public sealed class DBHelper
    {
        private string DBPath = AppDomain.CurrentDomain.BaseDirectory + "db/database.db";
        private string tempDBPath = AppDomain.CurrentDomain.BaseDirectory + "db/temp.db";

        public DBHelper() {
            InitDB();
        }
        public static readonly DBHelper Instance = new DBHelper();

        private ILiteDatabase db = null;
        private volatile bool CanAccess = false;

        private ILiteCollection<AllowGroup> allowGroupCol = null;
        private ILiteCollection<GagItem> gagItemCol = null;
        private ILiteCollection<RBQ> rbqCol = null;
        private ILiteCollection<RBQStatus> rbqStatusCol = null;
        private ILiteCollection<MessageCount> messageCountCol = null;

        /// <summary>[绒布球] 给自己带上了默认口塞! 咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!</summary>
        public readonly string[] defaultSelfLockMsg = { "咦?! 居然自己给自己戴? 真是个可爱的绒布球呢!" };

        /// <summary>[Ta] 帮 [绒布球] 带上了默认口塞! 顺便在 Ta 身上画了一个正字~</summary>
        public readonly string[] defaultLockMsg = { "顺便在 Ta 身上画了一个正字~" };

        /// <summary>[Ta] 帮 [绒布球] 修好了口塞!\n顺便展示了钥匙并丢到了一边!</summary>
        public readonly string[] defaultEnhancedLockMsg = { "修好了口塞!\n顺便展示了钥匙并丢到了一边!" };

        /// <summary>[绒布球] 挣脱了被人们安装的 超大号默认口塞! Ta感觉自己可以容纳更大的尺寸了呢!</summary>
        public readonly string[] defaultUnlockMsg = { "Ta感觉自己可以容纳更大的尺寸了呢!" };

        private void InitDB()
        {
            if (System.IO.Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}db/") != true) System.IO.Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}db/");
            db = new LiteDatabase(new ConnectionString() {
                Filename = DBPath,
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
            messageCountCol = db.GetCollection<MessageCount>("MessageCount");

            CanAccess = true;
        }

        public void StopDatabase()
        {
            CanAccess = false;
            db.Checkpoint();
            db.Commit();

            allowGroupCol = null;
            gagItemCol = null;
            rbqCol = null;
            rbqStatusCol = null;
            messageCountCol = null;

            db.Dispose();
            db = null;
        }

        public void GetDatabase()
        {
            CanAccess = false;
            db.Checkpoint();
            db.Commit();

            allowGroupCol = null;
            gagItemCol = null;
            rbqCol = null;
            rbqStatusCol = null;
            messageCountCol = null;

            db.Dispose();
            db = null;

            if (System.IO.File.Exists(tempDBPath) == true) System.IO.File.Delete(tempDBPath);
            System.IO.File.Copy(DBPath, tempDBPath);

            InitDB();
        }

        #region Group Operate
        public int GetGroupCount()
        {
            if (CanAccess) return allowGroupCol.Count();
            return -1;
        }

        public void AddAllowGroup(long groupId)
        {
            if (CanAccess)
            {
                var obj = new AllowGroup(groupId)
                {
                    AllowGag = true,
                    AllowMsgCount = true,
                    AllowVerify = true,
                    SamplyMsgCountStr = "快乐的一天开始了!"
                };
                allowGroupCol.Insert(obj);
            }
        }

        public void DelAllowGroup(long groupId)
        {
            if (CanAccess)
            {
                var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
                if (result != null) allowGroupCol.Delete(result.Id);
            }
        }

        public IEnumerable<AllowGroup> GetAllowGroups()
        {
            if (CanAccess)
            {
                return allowGroupCol.FindAll();
            }
            return new AllowGroup[0];
        }

        public AllowGroup GetAllowGroup(long groupId)
        {
            if (CanAccess)
            {
                var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
                if (result != null) return result;
            }
            return null;
        }

        public string GetAllowMsgCountStr(long groupId)
        {
            if (CanAccess)
            {
                var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
                if (result != null)
                {
                    return result.SamplyMsgCountStr;
                }
            }
            return "快乐的一天开始了!";
        }

        public bool GetAllowFunctionStatus(long groupId, AllowFunction query)
        {
            if (CanAccess)
            {
                var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
                if (result != null)
                {
                    switch (query)
                    {
                        case AllowFunction.AllowGag: return result.AllowGag;
                        case AllowFunction.AllowVerify: return result.AllowVerify;
                        case AllowFunction.AllowMsgCount: return result.AllowMsgCount;
                        default: return false;
                    }
                }
            }
            return false;
        }

        public bool GetAllowGroupExists(long groupId)
        {
            if (CanAccess)
            {
                var result = allowGroupCol.FindOne(x => x.GroupId == groupId);
                if (result != null) return true;
            }
            return false;
        }

        public void SetAllowGroup(AllowGroup allowGroup)
        {
            if (CanAccess)
            {
                allowGroupCol.Update(allowGroup);
            }
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
        public int GetGagItemCount()
        {
            if (CanAccess)
            {
                return gagItemCol.Count();
            }
            return 0;
        }

        public void AddGagItem(string gagName, int limitPoint, int unLockCount, bool showLimit, bool showUnlock, string[] selfLockMsg = null, string[] lockMsg = null, string[] enhancedLockMsg = null, string[] unlockMsg = null)
        {
            if (CanAccess)
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
        }

        public void DelGagItem(string gagName)
        {
            if (CanAccess)
            {
                var result = gagItemCol.FindOne(x => x.Name == gagName);
                if (result != null) gagItemCol.Delete(result.Id);
            }
        }

        public IEnumerable<GagItem> GetAllGagItems()
        {
            if (CanAccess)
            {
                return gagItemCol.FindAll();
            }
            return new GagItem[0];
        }

        public bool GetGagItemExist(string gagName)
        {
            if (CanAccess)
            {
                var result = gagItemCol.FindOne(x => x.Name == gagName);
                if (result != null) return true;
            }
            return false;
        }

        public GagItem GetGagItemInfo(string gagName)
        {
            if (CanAccess)
            {
                var result = gagItemCol.FindOne(x => x.Name == gagName);
                return result;
            }
            return new GagItem()
            {
                Id = 1,
                Name = "巧克力口塞",
                LimitPoint = -100,
                UnLockCount = 1,
                ShowLimit = true,
                ShowUnlock = true,
                SelfLockMsg = defaultSelfLockMsg,
                LockMsg = defaultLockMsg,
                EnhancedLockMsg = defaultEnhancedLockMsg,
                UnLockMsg = defaultUnlockMsg
            };
        }

        public GagItem GetGagItemInfo(int id)
        {
            if (CanAccess)
            {
                var result = gagItemCol.FindOne(x => x.Id == id);
                return result;
            }
            return new GagItem()
            {
                Id = 1,
                Name = "巧克力口塞",
                LimitPoint = -100,
                UnLockCount = 1,
                ShowLimit = true,
                ShowUnlock = true,
                SelfLockMsg = defaultSelfLockMsg,
                LockMsg = defaultLockMsg,
                EnhancedLockMsg = defaultEnhancedLockMsg,
                UnLockMsg = defaultUnlockMsg
            };
        }

        public void SetGagItem(string gagName, int limitPoint, int unLockCount, string[] selfLockMsg = null, string[] lockMsg = null, string[] enhancedLockMsg = null, string[] unlockMsg = null)
        {
            if (CanAccess)
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
        }

        public void SetGagItem(GagItem item)
        {
            if (CanAccess)
            {
                gagItemCol.Update(item);
            }
        }
        #endregion

        #region RBQ Operate
        public int GetRBQCount()
        {
            if (CanAccess)
            {
                return rbqCol.Count();
            }
            return 0;
        }

        public void AddRBQ(long telegramId, long rbqPoint)
        {
            if (CanAccess)
            {
                var obj = new RBQ()
                {
                    TelegramId = telegramId,
                    RBQPoint = rbqPoint
                };
                rbqCol.Insert(obj);
            }
        }

        public void DelRBQ(long telegramId)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null) rbqCol.Delete(result.Id);
            }
        }

        public bool GetRBQExist(long telegramId)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null) return true;
            }
            return false;
        }

        public RBQ GetRBQInfo(long telegramId)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                return result;
            }
            return new RBQ()
            {
                TelegramId = 777000,
                RBQPoint = 0,
            };
        }

        public long GetRBQPoint(long telegramId)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null) return result.RBQPoint;
            }
            return -1;
        }

        public void SetRBQInfo(long telegramId, long rbqPoint)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null)
                {
                    result.RBQPoint = rbqPoint;
                    rbqCol.Update(result);
                }
            }
        }

        public void SetRBQPointAdd(long telegramId, long addRbqPoint)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null)
                {
                    result.RBQPoint = result.RBQPoint + addRbqPoint;
                    rbqCol.Update(result);
                }
            }
        }

        public void SetRBQPointAdd1(long telegramId)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null)
                {
                    result.RBQPoint++;
                    rbqCol.Update(result);
                }
            }
        }

        public void SetRBQPointDel(long telegramId, long delRbqPoint)
        {
            if (CanAccess)
            {
                var result = rbqCol.FindOne(x => x.TelegramId == telegramId);
                if (result != null)
                {
                    result.RBQPoint = result.RBQPoint - delRbqPoint;
                    rbqCol.Update(result);
                }
            }
        }

        public void SetRBQInfo(RBQ rbq)
        {
            if (CanAccess)
            {
                rbqCol.Update(rbq);
            }
        }

        public IEnumerable<RBQ> GetAllRBQ()
        {
            if (CanAccess)
            {
                return rbqCol.FindAll();
            }
            return new RBQ[0];
        }

        #endregion

        #region RBQStatus Operate
        public int GetRBQStatusCount()
        {
            if (CanAccess)
            {
                return rbqStatusCol.Count();
            }
            return 0;
        }

        public bool AddRBQFroms(long groupId, long rbqId, long userId)
        {
            if (CanAccess)
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
                }
            }
            return false;
        }

        public void AddRBQStatus(long groupId, long rbqId, long lockCount = 0, bool anyUsed = false)
        {
            if (CanAccess)
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
        }

        public void DelRBQStatus(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) rbqStatusCol.Delete(result.Id);
            }
        }

        public IEnumerable<RBQStatus> GetAllRBQStatus()
        {
            if (CanAccess)
            {
                return rbqStatusCol.FindAll();
            }
            return new RBQStatus[0];
        }

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
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null)
                {
                    if (result.FromId == null) return false;

                    for (int i = 0; i < result.FromId.Length; i++)
                    {
                        if (result.FromId[i] == userId) return true;
                    }
                }
            }
            return false;
        }

        public int GetRBQGagId(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return result.GagId;
            }
            return 0;
        }

        public long GetRBQLockCount(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return result.LockCount;
            }
            return 0;
        }

        public bool GetRBQStatusExist(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return true;
            }
            return false;
        }

        public bool GetRBQCanLock(long groupId, long rbqId)
        {
            if (CanAccess)
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
                    // (result.StartLockTime)开始锁定时间+(Program.LockTime)超时时间+CD < 当前时间
                    if (new DateTime(result.StartLockTime).AddMinutes(Program.LockTime).AddSeconds(cd) < DateTime.UtcNow.AddHours(8)) return true;
                    else return false;
                }
            }
            return false;
        }

        public bool GetRBQAnyUsed(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return result.AnyUsed;

            }
            return false;
        }

        public long[] GetRBQFromId(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return result.FromId;
            }
            return new long[0];
        }

        public RBQStatus GetRBQStatus(int id)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.Id == id);
                if (result != null) return result;
            }
            return null;
        }

        public RBQStatus GetRBQStatus(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return result;
            }
            return null;
        }

        public int GetRBQStatusId(long groupId, long rbqId)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null) return result.Id;
            }
            return -1;
        }

        public void SetRBQStatus(long groupId, long rbqId, long lockCount, bool anyUsed, int gagId, long startLockTime, long[] fromId)
        {
            if (CanAccess)
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
        }

        public void SetRBQUsed(long groupId, long rbqId, bool anyUsed)
        {
            if (CanAccess)
            {
                var result = rbqStatusCol.FindOne(x => x.GroupId == groupId && x.RBQId == rbqId);
                if (result != null)
                {
                    result.AnyUsed = anyUsed;
                    rbqStatusCol.Update(result);
                }
            }
        }

        public void SetRBQStatus(RBQStatus rbq)
        {
            if (CanAccess)
            {
                rbqStatusCol.Update(rbq);
            }
        }

        #endregion

        #region MessageCount Operate
        public void AddMessageCountUser(long groupId, long userId)
        {
            if (CanAccess)
            {
                var obj = new MessageCount(messageCountCol.Count()+1, groupId, userId);
                messageCountCol.Insert(obj);
            }
        }

        public void AddMessageCountUser(long groupId, long userId, int count)
        {
            if (CanAccess)
            {
                var obj = new MessageCount(messageCountCol.Count()+1, groupId, userId, count);
                messageCountCol.Insert(obj);
            }
        }

        public bool GetMessageCountUserExist(long groupId, long userId)
        {
            if (CanAccess)
            {
                var result = messageCountCol.FindOne(x => x.GroupId == groupId && x.UserId == userId);
                if (result.Id > 0) return true;
            }
            return false;
        }

        public void AddMessageCountUserCount(long groupId, long userId)
        {
            if (CanAccess)
            {
                var result = messageCountCol.FindOne(x => x.GroupId == groupId && x.UserId == userId);
                if (result.Id > 0)
                {
                    result.Count++;
                    messageCountCol.Update(result);
                }
            }
        }

        public IEnumerable<MessageCount> GetAllMessageCounts()
        {
            if (CanAccess)
            {
                return messageCountCol.FindAll();
            }
            return new MessageCount[0];
        }

        public int GetMessageCountTableCount()
        {
            if (CanAccess)
            {
                return messageCountCol.Count();
            }
            return 0;
        }

        public void DropMessageCountTable()
        {
            if (CanAccess)
            {
                db.DropCollection("MessageCount");
            }
        }

        #endregion
    }
}
