# Danheng Server

**__此项目正在开发中!__**

<p align="center">
<a href="https://visualstudio.com"><img src="https://img.shields.io/badge/Visual%20Studio-000000.svg?style=for-the-badge&logo=visual-studio&logoColor=white" /></a>
<a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-000000.svg?style=for-the-badge&logo=.NET&logoColor=white" /></a>
<a href="https://www.gnu.org/"><img src="https://img.shields.io/badge/GNU-000000.svg?style=for-the-badge&logo=GNU&logoColor=white" /></a>
</p>
<p align="center">
  <a href="https://discord.gg/xRtZsmHBVj"><img src="https://img.shields.io/badge/Discord%20Server-000000.svg?style=for-the-badge&logo=Discord&logoColor=white" /></a>
</p>

[EN](../README.md) | [簡中](README_zh-CN.md) | [繁中](README_zh-CN.md) | [JP](README_ja-JP.md)

## 💡功能

- [√] **商店**
- [√] **编队**
- [√] **抽卡** - 自定义概率: )
- [√] **战斗** - 场景技能中有一些错误
- [√] **场景** - 行走模拟器、交互、正确加载实体
- [√] **基本的角色培养** - 一些小bug，影响体验不大
- [√] **任务** - 已完成男主的许多任务，若你选择女主则可能会在某些任务中卡住，需要修复
- [-] **朋友** - 开发中...
- [-] **模拟宇宙** - 开发中...

- [ ] **更多**  - Comming soon...

## 🍗使用&安装

### 快速启动

1. 在 [Action](https://github.com/StopWuyu/DanhengServer/actions) 下载可执行文件
2. 打开下载完成的 `DanhengServer.zip` 解压至任意文件夹 __*最好是英文路径*__

> (可选) 在源代码的WebServer文件夹中下载 `certificate.p12` 使得以HTTPS模式启动 让你的传输更安全: )

3. 运行GameServer.exe
4. 运行代理 启动游戏 链接，享受！

### 构建

Danhengserver使用Dotnet构建

**前置：**

- [.NET](https://dotnet.microsoft.com/)
- [Git](https://git-scm.com/downloads)

##### Windows

```shell
git clone --recurse-submodules https://github.com/StopWuyu/DanhengServer.git
cd DanhengServer
.\dotnet build # 编译
```
##### Linux （Ubuntu20.04）
```shell
# 添加 Microsoft 包存储库添加 Microsoft 包存储库
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安装SDK
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0
```

- 编译并运行环境
```shell
git clone --recurse-submodules https://github.com/StopWuyu/DanhengServer.git
cd DanhengServer
.\dotnet build # コンパイル
./Gameserver
```
**向 Microsoft 显示其他系统版本**
- [微软教程](https://dotnet.microsoft.com/zh-cn/download/dotnet/thank-you/sdk-8.0.204-linux-x64-binaries)

## ❓帮助

- 支持安卓系统
- 100040119（无法自动完成）（使用 /mission finish 100040119 进行修复）

## ❕️故障排除

获取常见问题的解决方案或寻求帮助，请加入[我们的Discord服务器](https://discord.gg/xRtZsmHBVj)

## 🙌鸣谢

- Weedwacker - 提供 kcp 实现
- [SqlSugar](https://github.com/donet5/SqlSugar) - 提供 ORM
- [LunarCore](https://github.com/Melledy/LunarCore) - 一些数据结构和算法
