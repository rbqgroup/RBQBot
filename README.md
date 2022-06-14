# RBQBot
tg-rbq-bot 的 C# 重制版  
目前基于.NET Core 6  
BuildToLinux.bat => Linux-X64 不管发行版 单文件带运行时 （[Alpine最小依赖](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux-alpine#dependencies "Alpine基础库参考点我")）（[其他OS参考](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux "其他OS参考点我")）  
BuildToWindows.bat => Windows-X64 [可用版本](https://docs.microsoft.com/zh-cn/dotnet/core/install/windows?tabs=net60#supported-releases "可用系统参考点我") [系统补丁点我](https://docs.microsoft.com/zh-cn/dotnet/core/install/windows?tabs=net60#additional-deps "各版本系统补丁点我")

以下是修改口塞的Json 数据，除了SelfLockMsg/LockMsg/EnhancedLockMsg/UnLockMsg可以为null，其他请正确传入，使用时需要将其压缩为一行

{
    "Name": "口塞名称",
    "LimitPoint": "123",
    "UnLockCount": "123",
	"ShowLimit": true,
    "ShowUnlock": false,
    "SelfLockMsg": null,
    "LockMsg": null,
    "EnhancedLockMsg": ["a", "b"],
    "UnLockMsg": ["a", "b"]
}
