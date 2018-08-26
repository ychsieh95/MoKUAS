# MoKUAS

A user-friendly website of KUAS.

Based on ASP.NET Core 2.1 and using Razor Page.

# What is MoKUAS?

[MoKUAS](https://kuas.holey.cc) 是由原先最初設計的 KUASxSecretary 應用程式為基礎，發展為網頁版本的 KUASxSECS 後，再以此所延伸出的新網頁版本。除了延續 KUASxSECS 所有的功能外，也更新、優化了架構、組件與介面。

關於 KUASxSECS 的發想起源，可以參考 [KUASxSECS 從無到有](https://blog.holey.cc/2018/01/01/kuasxsecs-start-from-scratch/)。

關於 MoKUAS 的小小心得，可以參考 [MoKUAS - KUASxSECS Rebuild On ASP.NET Core](https://blog.holey.cc/2018/08/31/mokuas-kuasxsecs-rebuild-on-asp-net-core/)。

# What can MoKUAS do?

目前 MoKUAS 提供了以下功能：

* 學期資訊
    * 學籍資料
    * 學期課表
    * 學期成績
    * 學期缺曠
    * 學分試算
* 預警報表
    * 期中預警資料
    * 歷年成績報表
    * 畢業預審查調
    * 畢業預審報表
* 繳費單據
    * 繳費單
    * 繳費單收據
* 教學評量
    * 教學評量自動填寫
* 學生評量
    * 評價排行
    * 搜尋課程
    * 課程綱要
    * 所有評論
    * 我的評論
    * 新增評論

# Database Schema

MoKUAS 資料庫結構描述如下圖，也另外提供 [MoKUAS 結構描述指令碼](https://blog.holey.cc/2018/08/31/mokuas-kuasxsecs-rebuild-on-asp-net-core/MoKUAS-Schema-Only.zip)以建構資料庫。

![MoKUAS Database Schema](https://blog.holey.cc/2018/08/31/mokuas-kuasxsecs-rebuild-on-asp-net-core/mokuas-database-schema.png)