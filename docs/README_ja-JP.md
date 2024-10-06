# Danheng Server-Beta

**__このプロジェクトは開発中です！__**

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
<td valign="center"><a href="../README.md"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1fa-1f1f8.png" width="16"/> English</td>
 
<td valign="center"><a href="README_zh-CN.md"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 简中</td>
 
<td valign="center"><a href="README_zh-TW.md"><img src="https://em-content.zobj.net/thumbs/120/twitter/351/flag-china_1f1e8-1f1f3.png" width="16"/> 繁中</td>
 
<td valign="center"><img src="https://github.com/twitter/twemoji/blob/master/assets/72x72/1f1ef-1f1f5.png" width="16"/> 日本語</td>
</td>
</table>
</div>

## 💡機能

- [√] **ストア機能**
- [√] **チームを編成する**
- [√] **ドロー** - カスタム可能な確率: )
- [√] **と戦う** - シーンスキルにいくつかのエラーがあります
- [√] **シーン** - 歩行シミュレータ、インタラクティブ、正確なエンティティロード
- [√] **基本的な役割育成** - ちょっとバグがありますが、影響はあまりありません
- [√] **タスク＃タスク＃** - 一部のタスクにはエラーがある可能性があります。ベロブルグの主要なタスクはすべて完了しています。男性主人公と女性主人公にはまだテストされていません
- [-] **交友機能**
- [-] **忘却の庭 & 虚構叙事 &終末の幻影**
- [-] **模擬宇宙 & 黄金機械 & 差分宇宙**

- [ ] **詳細**  - Coming soon...

「アニメーション ゲーム」の新しいバージョンがリリースされると、最初は一部の機能がサポートされなくなります。引き続き、私たちの投稿にご注意ください。  バージョン 2.3 以降、ベータ版に適したプライベート ブランチを設立し、準備が完了次第、メイン ウェアハウスにマージされます。

## 🍗使用&インストール

### クイックスタート

1. [Action](https://github.com/StopWuyu/DanhengServer-Beta/actions) で実行可能ファイルをダウンロードする
2. ダウンロードが完了した` DanhengServer-Beta.zip `を開いて任意のフォルダに解凍します __*英文パスが望ましい*__

> (オプション) ソースコードのWebServerフォルダに` certificate.p 12 `をダウンロードすることで、HTTPSモードで起動して転送をより安全にすることができます: )

3. GameServer.exeの実行
4. エージェント起動ゲームの実行

### 構築＃コウチク＃

Danhengserver Dotnetを使用した構築

**プリソフトウェア：**

- [.NET](https://dotnet.microsoft.com/)
- [Git](https://git-scm.com/downloads)

##### Windows

```shell
git clone --recurse-submodules https://github.com/StopWuyu/DanhengServer-Beta.git
cd DanhengServer
.\dotnet build # コンパイル
```

##### Linux （Ubuntu20.04）
- インストール環境
```shell
# Microsoftパッケージリポジトリの追加
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# SDKのインストール
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

- 環境をコンパイルして実行する
```shell
git clone --recurse-submodules https://github.com/StopWuyu/DanhengServer-Beta.git
cd DanhengServer-Beta
.\dotnet build # コンパイル
./Gameserver
```
**他のシステムバージョンはMicrosoftに表示してください**
- [Microsoftのチュートリアル](https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/sdk-8.0.204-linux-x64-binaries)

## ❓ヘルプ

- アンドロイドシステムのサポート
- 100040119（自動完了できません）（/mission finish 100040119 を使用した修正）

## ❕️トラブルシューティング

一般的な問題の解決策を入手するか、ヘルプを参照してください。[デルのDiscordサーバ](https://discord.gg/xRtZsmHBVj)

## 🙌に感謝

- Weedwacker - kcp実装の提供
- [SqlSugar](https://github.com/donet5/SqlSugar) - ORMの提供
- [LunarCore](https://github.com/Melledy/LunarCore) - いくつかのデータ構造とアルゴリズム
