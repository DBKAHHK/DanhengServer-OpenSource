# Danheng Server-Beta

**__此專案目前尚在開發中！__**

<p align="center">
<a href="https://visualstudio.com"><img src="https://img.shields.io/badge/Visual%20Studio-000000.svg?style=for-the-badge&logo=visual-studio&logoColor=white" /></a>
<a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-000000.svg?style=for-the-badge&logo=.NET&logoColor=white" /></a>
<a href="https://www.gnu.org/"><img src="https://img.shields.io/badge/GNU-000000.svg?style=for-the-badge&logo=GNU&logoColor=white" /></a>
</p>
<p align="center">
  <a href="https://discord.gg/xRtZsmHBVj"><img src="https://img.shields.io/badge/Discord%20Server-000000.svg?style=for-the-badge&logo=Discord&logoColor=white" /></a>
</p>

<div align="center">
<table>
<td valign="center"><a href="*/README.md"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1fa-1f1f8.png" width="16"/> English</td>
 
<td valign="center"><a href="README_zh-CN.md"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 简中</td>
 
<td valign="center"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 繁中</td>
 
<td valign="center"><a href="README-JP.md"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1ef-1f1f5.png" width="16"/> 日本語</td>
</td>
</table>
</div>

## 💡功能

- [√] **商店**
- [√] **編隊**
- [√] **抽卡** - 可自訂機率: )
- [√] **戰鬥** - 場景技能中有可能存在一些錯誤
- [√] **場景** - 走路模擬器、互動、正確載入實體
- [√] **基本的角色培養** - 一些小bug，影響體驗不大
- [√] **任務** - 某些任務中可能存在一些錯誤，貝洛伯格主線任務已全數完成，剩餘內容正在製作中或是尚未測試，適用於星與穹
- [√] **好友**
- [√] **忘卻之庭 & 虛構敘事 & 末日幻影**
- [√] **類比宇宙&黃金機械&差分宇宙**
- [ ] **更多**  - Coming soon...

當新版本之「某動漫遊戲」發佈時，某些功能將不會在第一時間支援，請持續關注我們的提交內容。自從2.3版本起，我們建立了適用於Beta版本的私人分支，將在準備完成後第一時間合併至主倉庫。

## 🍗使用&安裝

### 快速啟動

1. 在 [Action](https://github.com/StopWuyu/DanhengServer-Beta/actions) 下載可執行文件
2. 打開下載完成的 `DanhengServer-Beta.zip` 解解壓至任意資料夹 __*最好是英文路徑*__

> (可選) 在原始碼的WebServer資料夾中下載 `certificate.p12` 使其以HTTPS模式啟動 讓你的傳輸更加安全: )

3. 運行GameServer.exe
4. 運行代理 啟動遊戲 連結，享受！

### 構建

DanhengServer 使用 .NET Framework 構建

**前置：**

- [.NET](https://dotnet.microsoft.com/)
- [Git](https://git-scm.com/downloads)

##### Windows

```shell
git clone --recurse-submodules https://github.com/StopWuyu/DanhengServer-Beta.git
cd DanhengServer-Beta
.\dotnet build # 編譯
```
##### Linux （Ubuntu 20.04）
```shell
# 添加 Microsoft 包儲存庫
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安裝 .NET SDK
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

- 編譯並運行環境
```shell
git clone --recurse-submodules https://github.com/StopWuyu/DanhengServer-Beta.git
cd DanhengServer-Beta
.\dotnet build # 編譯
./Gameserver
```
**向 Microsoft 顯示其他系統版本**
- [微軟教學](https://dotnet.microsoft.com/zh-tw/download/dotnet/thank-you/sdk-8.0.204-linux-x64-binaries)

## ❓幫助

- 支持Android系统
- 100040119（無法自動完成）（使用 /mission finish 100040119 進行修復）

## ❕️故障排除

獲取常見問題的解決方案或尋求幫助，請加入[我們的Discord伺服器](https://discord.gg/xRtZsmHBVj)

## 🙌感謝

- Weedwacker - 提供 kcp 實現
- [SqlSugar](https://github.com/donet5/SqlSugar) - 提供 ORM
- [LunarCore](https://github.com/Melledy/LunarCore) - 一些資料結構和算法
erver
