# PEAK Cheat

[中文](#中文说明) | [English](#english)

---

## 中文说明

### 项目简介

`PEAK Cheat` 是一个面向 `PEAK` 的 Unity 运行时注入项目，使用 `C#` 和 `.NET Framework 4.7.2` 编写。项目的核心思路是在游戏进程内创建一个常驻 `GameObject`，挂载自定义 `MonoBehaviour`，然后在 `Update()` 中读取角色状态并修改运行时数据，在 `OnGUI()` 中绘制自定义菜单与 ESP 覆盖层。

从源码结构来看，这个仓库不仅包含主功能代码，还包含一份较完整的游戏反编译参考目录，便于开发阶段查找字段、属性、方法名并做反射适配。

### 项目内容介绍

本项目当前主要包含以下能力：

- 玩家功能
  - 无限体力
  - 上帝模式
  - 清除负面状态
  - 飞行模式
  - 穿墙模式
  - 速度提升
  - 跳跃提升
  - 自我复活
  - 向上传送 10 米
- ESP 功能
  - 玩家显示
  - 敌对生物显示
  - 物资箱/复活箱显示
  - 食物/重要道具显示
  - 营火存档点显示
  - 距离文本
  - 追踪射线
  - 血量条
- 菜单与配置
  - 插入式 IMGUI 菜单
  - 快捷键绑定
  - JSON 配置热加载
  - 多主题启动动画与菜单皮肤

### 技术栈

- 语言与框架
  - `C#`
  - `.NET Framework 4.7.2`
- 运行环境
  - [Unity](https://unity.com/)
  - `MonoBehaviour`
  - `OnGUI / IMGUI`
- 游戏程序集引用
  - `UnityEngine.dll`
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.IMGUIModule.dll`
  - `UnityEngine.TextRenderingModule.dll`
  - `UnityEngine.JSONSerializeModule.dll`
  - `UnityEngine.InputLegacyModule.dll`
  - `UnityEngine.PhysicsModule.dll`
  - `Assembly-CSharp.dll`
  - `PhotonUnityNetworking.dll`
  - `PhotonRealtime.dll`
- 核心实现方式
  - 通过注入后创建 `GameObject("PEAK_Wallhack_Injector")`
  - 通过 `AddComponent<WallhackBehaviour>()` 挂载主逻辑
  - 通过 Unity 运行时查找场景实体
  - 通过反射读写游戏内部字段
  - 通过 JSON 文件持久化配置
- 开发辅助
  - [ILSpy](https://github.com/icsharpcode/ILSpy) 命令行工具
  - 仓库内 `decompiled/` 反编译参考代码

### 工作原理

项目注入成功后，实际执行入口会创建一个不会随场景销毁的对象，并由 `WallhackBehaviour` 接管生命周期：

1. `Start()`
   - 初始化配置文件
   - 初始化 ESP 组件
   - 重置功能状态
2. `Update()`
   - 轮询快捷键
   - 控制菜单开关
   - 热加载配置
   - 对本地角色应用功能修改
3. `OnGUI()`
   - 绘制主题背景和菜单窗口
   - 绘制 ESP 覆盖层

ESP 数据主要来自 Unity 运行时对象扫描：

- `Character` 用于玩家/敌对生物
- `Campfire` 用于营火存档点
- `Luggage` 与 `RespawnChest` 用于物资箱
- `Item` 用于食物和重要道具

同时，项目还通过 `EspItemRegistry` 对拾取物进行分类、颜色推断和配置覆盖，支持内建模板与运行时自动回退策略。

### 项目结构

```text
PEAK.Cheat/
├─ Program.cs                # 注入入口、主行为、菜单、功能实现
├─ ConfigManager.cs          # JSON 配置定义、保存、加载、热更新
├─ Features/
│  └─ ESP.cs                 # ESP 绘制逻辑
├─ GameData/
│  ├─ Entity.cs              # 实体模型与运行时扫描
│  └─ EspItemRegistry.cs     # 道具分类、颜色、配置映射
├─ AuxMenu/
│  └─ AuxMenuThemeRuntime.cs # 菜单主题、动画、性能预算
├─ UI/
│  └─ ToggleSwitchMath.cs    # 开关控件动画计算
├─ Render/
│  └─ Renderer.cs            # 渲染后端抽象占位
├─ decompiled/               # 游戏反编译参考代码，不参与主项目编译
└─ .tools/ilspycmd/          # ILSpy 命令行工具
```

### 使用方法

#### 1. 编译项目

在仓库根目录执行：

```powershell
dotnet build .\PEAK.Cheat.csproj -c Debug
```

默认输出：

```text
bin\Debug\net472\PEAK.Cheat.dll
```

#### 2. 注入 DLL

将生成的程序集通过你常用的注入器加载到 `PEAK` 进程中。

源码中的真实注入入口为：

- `namespace`: `Loading`
- `classname`: `Loader`
- `methodname`: `Load`

完整注入命令可写为：

```powershell
smi.exe injector -p "PEAK.exe" -a ".\bin\Debug\net472\PEAK.Cheat.dll" -n "Loading" -c "Loader" -m "Load"
```

#### 3. 打开菜单

- `Insert`: 打开/关闭菜单
- `End`: 卸载辅助

#### 4. 配置文件

项目启动后会自动在游戏目录附近创建配置文件：

```text
Config/Wallhack.json
```

该文件支持保存、重新加载和恢复默认内容，程序运行中也会检测文件写入时间并自动热更新。

### SMI 注入器命令

当前仓库源码唯一需要使用的入口就是：

- `namespace`: `Loading`
- `classname`: `Loader`
- `methodname`: `Load`

完整示例命令：

```powershell
smi.exe injector -p "PEAK.exe" -a ".\bin\Debug\net472\PEAK.Cheat.dll" -n "Loading" -c "Loader" -m "Load"
```

参数说明：

- `-p`: 目标进程名
- `-a`: 要注入的程序集路径
- `-n`: 命名空间
- `-c`: 类名
- `-m`: 方法名

### 功能说明

#### 玩家功能

- 无限体力
  - 将体力恢复到上限
  - 将冲刺和跳跃体力消耗设置为 `0`
- 上帝模式
  - 通过反射尝试设置多个常见无敌字段
- 免疫负面状态
  - 清理角色身上的 affliction/status/debuff 集合
- 飞行模式
  - 关闭重力影响
  - `Space` 上升，`Ctrl` 下降
- 穿墙模式
  - 关闭碰撞体
  - 关闭角色刚体碰撞检测
  - 按相机平面方向进行无碰撞移动
- 速度/跳跃增强
  - 修改移动倍率、冲刺倍率、跳跃冲量

#### ESP 功能

- 玩家
  - 方框
  - 名称
  - 距离
  - 射线
  - 血量条
- 敌对生物
  - 仅显示敌对目标
  - 过滤环境伤害来源
  - 支持最大显示距离
- 食物与拾取物
  - 基于物品名和标签自动分类
  - 支持毒性判断
  - 支持内建配置与运行时回退模板
- 营火和箱体
  - 直接通过场景对象扫描获取位置

### 对开发者的构建方法

#### 环境要求

- Windows
- 已安装支持 `net472` 的 .NET SDK / Visual Studio
- 本地已安装 `PEAK`
- 能访问游戏的 `Managed` 目录

#### 关键前置条件

项目文件中直接引用了本机游戏目录下的 DLL。默认引用路径示例为：

```xml
d:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed\
```

如果你的游戏安装路径不同，需要先修改 `PEAK.Cheat.csproj` 内的 `HintPath`，把所有引用改成你自己的本地路径。

#### 推荐构建流程

1. 克隆仓库
2. 打开 `PEAK.Cheat.csproj`
3. 检查并修正所有 `HintPath`
4. 使用 `dotnet build` 或 Visual Studio 编译
5. 取出生成的 `PEAK.Cheat.dll`
6. 用注入器加载到游戏进程

#### 已验证的构建命令

当前仓库在本地可直接通过以下命令编译成功：

```powershell
dotnet build .\PEAK.Cheat.csproj -c Debug
```

生成结果：

```text
bin\Debug\net472\PEAK.Cheat.dll
```

### 开发者备注

- `decompiled/` 目录不会参与主项目编译，主要用于开发时查找游戏内部结构
- `Render/Renderer.cs` 目前是渲染后端抽象占位，并不是当前 GUI 的核心实现路径
- 菜单主界面使用的是 Unity 自带 `OnGUI`，不是外部 UI 框架
- 大量功能通过“多候选字段名”的反射方式兼容不同运行时成员命名
- 配置系统使用 `JsonUtility`，因此结构设计偏向 Unity 可序列化风格

### 免责声明

- 本项目仅用于源码学习、Unity 运行时研究与个人测试
- 请自行评估使用此类项目是否违反游戏服务条款、平台规则或社区规范
- 由使用、分发或修改本项目产生的任何后果，需由使用者自行承担

---

## English

### Overview

`PEAK Cheat` is a Unity runtime injection project for `PEAK`, written in `C#` on `.NET Framework 4.7.2`. The core idea is to inject a managed assembly, create a persistent `GameObject`, attach a custom `MonoBehaviour`, modify gameplay state in `Update()`, and draw a custom menu plus ESP overlay in `OnGUI()`.

The repository also includes a large decompiled reference tree of the game assemblies, which is useful when tracking fields, properties, and methods for reflection-based feature development.

### What This Project Includes

Current source code covers the following major areas:

- Player features
  - Infinite stamina
  - God mode
  - No afflictions
  - Fly mode
  - No-clip
  - Speed boost
  - Jump boost
  - Self revive
  - Teleport up by 10 meters
- ESP features
  - Player ESP
  - Hostile creature ESP
  - Loot box / respawn chest ESP
  - Food / pickup ESP
  - Campfire ESP
  - Distance labels
  - Tracers
  - Health bars
- Menu and config
  - IMGUI in-game menu
  - Hotkey binding
  - JSON config hot reload
  - Multi-theme startup animation and menu skin system

### Tech Stack

- Language and framework
  - `C#`
  - `.NET Framework 4.7.2`
- Runtime
  - [Unity](https://unity.com/)
  - `MonoBehaviour`
  - `OnGUI / IMGUI`
- Game assembly references
  - `UnityEngine.dll`
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.IMGUIModule.dll`
  - `UnityEngine.TextRenderingModule.dll`
  - `UnityEngine.JSONSerializeModule.dll`
  - `UnityEngine.InputLegacyModule.dll`
  - `UnityEngine.PhysicsModule.dll`
  - `Assembly-CSharp.dll`
  - `PhotonUnityNetworking.dll`
  - `PhotonRealtime.dll`
- Core implementation
  - Creates `GameObject("PEAK_Wallhack_Injector")` after injection
  - Attaches `WallhackBehaviour` as the primary runtime controller
  - Scans live Unity scene objects for entities
  - Uses reflection to read and write internal game members
  - Persists settings through JSON
- Development tooling
  - [ILSpy](https://github.com/icsharpcode/ILSpy) CLI
  - `decompiled/` reference source tree

### How It Works

Once injected, the runtime entry creates a persistent object and hands control to `WallhackBehaviour`:

1. `Start()`
   - Initializes config
   - Initializes ESP
   - Resets runtime feature state
2. `Update()`
   - Polls hotkeys
   - Handles menu visibility
   - Reloads config when the file changes
   - Applies local player modifications
3. `OnGUI()`
   - Draws the themed menu background and window
   - Draws the ESP overlay

ESP data is collected from live Unity objects such as:

- `Character` for players and hostile creatures
- `Campfire` for save campfires
- `Luggage` and `RespawnChest` for loot containers
- `Item` for food and important pickups

The project also uses `EspItemRegistry` to normalize pickup names, infer categories, resolve colors, and apply config-defined visual profiles.

### Project Structure

```text
PEAK.Cheat/
├─ Program.cs                # Injection entry, runtime behaviour, menu, features
├─ ConfigManager.cs          # JSON config schema, load/save/hot reload
├─ Features/
│  └─ ESP.cs                 # ESP drawing logic
├─ GameData/
│  ├─ Entity.cs              # Entity model and runtime scanning
│  └─ EspItemRegistry.cs     # Pickup classification and visual profile mapping
├─ AuxMenu/
│  └─ AuxMenuThemeRuntime.cs # Theme system, animation, performance budget
├─ UI/
│  └─ ToggleSwitchMath.cs    # Animated toggle switch math
├─ Render/
│  └─ Renderer.cs            # Placeholder renderer abstraction
├─ decompiled/               # Decompiled game references, excluded from build
└─ .tools/ilspycmd/          # ILSpy command-line tool
```

### Usage

#### 1. Build the project

Run this from the repository root:

```powershell
dotnet build .\PEAK.Cheat.csproj -c Debug
```

Default output:

```text
bin\Debug\net472\PEAK.Cheat.dll
```

#### 2. Inject the DLL

Load the compiled assembly into the `PEAK` process with your preferred injector.

The actual runtime entry in the current source is:

- `namespace`: `Loading`
- `classname`: `Loader`
- `methodname`: `Load`

The full injection command can be written as:

```powershell
smi.exe injector -p "PEAK.exe" -a ".\bin\Debug\net472\PEAK.Cheat.dll" -n "Loading" -c "Loader" -m "Load"
```

#### 3. Open the menu

- `Insert`: open or close the menu
- `End`: unload the cheat

#### 4. Config file

After startup, the project creates a config file near the game directory:

```text
Config/Wallhack.json
```

The file supports save, reload, reset, and hot reloading while the game is running.

### SMI Injector Command

The only entry you need in the current source is:

- `namespace`: `Loading`
- `classname`: `Loader`
- `methodname`: `Load`

Full example command:

```powershell
smi.exe injector -p "PEAK.exe" -a ".\bin\Debug\net472\PEAK.Cheat.dll" -n "Loading" -c "Loader" -m "Load"
```

Parameter summary:

- `-p`: target process name
- `-a`: assembly path
- `-n`: namespace
- `-c`: class name
- `-m`: method name

### Feature Notes

#### Player features

- Infinite stamina
  - Refills stamina to max
  - Sets sprint/jump stamina usage to `0`
- God mode
  - Tries multiple common invulnerability field names through reflection
- No afflictions
  - Clears affliction/status/debuff collections
- Fly mode
  - Disables gravity
  - `Space` moves up, `Ctrl` moves down
- No-clip
  - Disables colliders
  - Disables rigidbody collision checks
  - Moves the character along camera-relative directions
- Speed and jump boosts
  - Modifies movement, sprint, and jump-related runtime values

#### ESP features

- Players
  - Boxes
  - Names
  - Distance text
  - Tracers
  - Health bars
- Hostiles
  - Filters to hostile targets only
  - Ignores environment-damage-only sources
  - Supports configurable max distance
- Food and pickups
  - Auto-classified from item names and tags
  - Supports poisonous item detection
  - Supports built-in and inferred fallback visual profiles
- Campfires and chests
  - Read directly from scene object scanning

### Build Guide For Developers

#### Requirements

- Windows
- A .NET SDK / Visual Studio setup that can build `net472`
- A local `PEAK` installation
- Access to the game's `Managed` folder

#### Important prerequisite

The project file references game DLLs through absolute `HintPath` entries. The default sample path is:

```xml
d:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed\
```

If your game is installed elsewhere, update all `HintPath` entries in `PEAK.Cheat.csproj` before building.

#### Recommended build workflow

1. Clone the repository
2. Open `PEAK.Cheat.csproj`
3. Fix all `HintPath` values to match your local installation
4. Build with `dotnet build` or Visual Studio
5. Take the generated `PEAK.Cheat.dll`
6. Inject it into the game process

#### Verified command

The current repository builds successfully with:

```powershell
dotnet build .\PEAK.Cheat.csproj -c Debug
```

Output:

```text
bin\Debug\net472\PEAK.Cheat.dll
```

### Developer Notes

- `decompiled/` is excluded from the main build and mainly serves as reverse-engineering reference material
- `Render/Renderer.cs` is currently a placeholder abstraction, not the main path used by the in-game GUI
- The menu is built with Unity `OnGUI`, not a third-party UI framework
- Many features rely on reflection with multiple candidate member names for compatibility
- The config system uses `JsonUtility`, so the data model follows Unity-friendly serialization conventions

### Disclaimer

- This project is provided for source study, Unity runtime research, and personal testing
- You are responsible for evaluating whether usage violates game rules, platform policy, or terms of service
- Any consequences caused by using, modifying, or redistributing this project are your own responsibility
