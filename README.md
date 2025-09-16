# 分支视频游戏系统 (Branching Video Game System)

基于AVPro Video插件的交互式分支视频游戏系统，支持选择题和QTE（快速反应事件）。

## 功能特性

- 🎬 **视频播放**: 使用AVPro Video插件播放高质量视频
- 🔀 **分支剧情**: 基于用户选择的动态故事分支
- ⚡ **QTE系统**: 支持按键、序列和时机三种QTE类型
- 🎨 **现代UI**: 美观的用户界面，支持淡入淡出动画
- 📱 **响应式设计**: 自适应不同屏幕尺寸
- 🌐 **灵活配置**: 支持本地和CDN视频资源

## 系统要求

- Unity 2022.3 LTS 或更高版本
- AVPro Video 插件
- TextMeshPro 包

## 快速开始

### 1. 场景设置

在Unity中打开项目后，有两种方式设置场景：

#### 方法A: 自动设置（推荐）
1. 在场景中创建一个空的GameObject
2. 添加 `SceneSetupHelper` 脚本
3. 在Inspector中勾选 "Auto Setup On Start"
4. 运行场景，系统将自动创建所有必要的GameObject和组件

#### 方法B: 手动设置
1. 创建以下GameObject结构：
```
Scene
├── GameManager (BranchingVideoGameManager, GameDataLoader)
├── VideoPlayer (MediaPlayer, VideoPlayerController)
│   └── VideoDisplay (RawImage, DisplayUGUI)
├── UICanvas (Canvas, GameUIController)
│   ├── ChoicePanel
│   │   ├── QuestionText (TextMeshProUGUI)
│   │   └── ChoiceButtonContainer (VerticalLayoutGroup)
│   └── QTEPanel
│       ├── QTEInstructionText (TextMeshProUGUI)
│       ├── QTEProgressBar (Slider)
│       └── QTEKeyText (TextMeshProUGUI)
```

### 2. 配置游戏数据

#### 使用内置JSON文件
1. 将游戏配置JSON文件放入 `Assets/StreamingAssets/` 目录
2. 在 `BranchingVideoGameManager` 中设置 `gameDataJson` 引用

#### 使用外部URL
1. 在 `BranchingVideoGameManager` 中设置 `gameDataUrl`
2. 确保URL可访问且返回有效的JSON数据

### 3. 视频资源设置

#### 本地视频
1. 创建 `Assets/StreamingAssets/Videos/` 目录
2. 将视频文件放入该目录
3. 在JSON配置中不设置 `cdnBase` 或设置为空

#### CDN视频
1. 上传视频到CDN服务
2. 在JSON配置的 `meta.cdnBase` 中设置CDN基础URL

## JSON配置格式

### 基础结构
```json
{
  "meta": {
    "title": "游戏标题",
    "cdnBase": "https://your-cdn.com/videos",
    "startNodeId": "N01"
  },
  "nodes": [
    {
      "id": "N01",
      "video": "video1.mp4",
      "question": "问题文本",
      "choices": [
        {"label": "选项1", "next": "N02"},
        {"label": "选项2", "next": "N03"}
      ]
    }
  ]
}
```

### QTE配置
```json
{
  "id": "N02",
  "video": "video2.mp4",
  "question": "QTE挑战！",
  "qte": {
    "type": "button",
    "duration": 3.0,
    "successNext": "N03_SUCCESS",
    "failNext": "N03_FAIL"
  }
}
```

### QTE类型说明

#### 1. Button QTE
```json
{
  "type": "button",
  "duration": 3.0,
  "successNext": "success_node",
  "failNext": "fail_node"
}
```
- 玩家需要在绿色区域（最后20%时间）按空格键

#### 2. Sequence QTE
```json
{
  "type": "sequence",
  "duration": 5.0,
  "sequence": ["A", "S", "D", "F"],
  "successNext": "success_node",
  "failNext": "fail_node"
}
```
- 玩家需要按照指定顺序按键

#### 3. Timing QTE
```json
{
  "type": "timing",
  "duration": 4.0,
  "successNext": "success_node",
  "failNext": "fail_node"
}
```
- 玩家需要在合适时机（中间40%时间）按空格键

## 脚本说明

### 核心脚本

- **BranchingVideoGameManager**: 主游戏管理器，控制游戏流程
- **VideoPlayerController**: 视频播放控制器，封装AVPro Video功能
- **GameUIController**: UI控制器，管理选择界面和QTE显示
- **GameData**: 数据结构定义
- **GameDataLoader**: 数据加载工具
- **SceneSetupHelper**: 场景自动设置工具

### 主要方法

#### BranchingVideoGameManager
- `StartGame()`: 开始游戏
- `RestartGame()`: 重启游戏
- `PlayNode(string nodeId)`: 播放指定节点
- `PauseGame()` / `ResumeGame()`: 暂停/恢复游戏

#### VideoPlayerController
- `PlayVideo(string videoUrl)`: 播放视频
- `StopVideo()`: 停止视频
- `IsVideoPlaying()`: 检查播放状态

#### GameUIController
- `ShowChoices()`: 显示选择界面
- `ShowQTE()`: 显示QTE界面
- `HideAllUI()`: 隐藏所有UI

## 自定义和扩展

### 添加新的QTE类型
1. 在 `QTEData` 类中添加新的类型字段
2. 在 `GameUIController.ShowQTE()` 中添加处理逻辑
3. 实现对应的协程方法

### 自定义UI样式
1. 修改 `SceneSetupHelper` 中的UI创建方法
2. 调整颜色、字体、布局等属性
3. 添加动画和特效

### 扩展数据结构
1. 在 `GameData.cs` 中添加新字段
2. 更新JSON配置格式
3. 在相关脚本中添加处理逻辑

## 故障排除

### 常见问题

1. **视频无法播放**
   - 检查AVPro Video插件是否正确安装
   - 确认视频文件格式受支持
   - 检查文件路径是否正确

2. **UI不显示**
   - 确认Canvas设置正确
   - 检查UI元素的RectTransform配置
   - 验证脚本引用是否正确连接

3. **JSON解析失败**
   - 使用JSON验证器检查格式
   - 确认文件编码为UTF-8
   - 检查特殊字符是否正确转义

4. **QTE不响应**
   - 确认Input System设置
   - 检查QTE面板是否激活
   - 验证按键映射是否正确

### 调试工具

- 使用 `[ContextMenu]` 方法进行运行时调试
- 查看Console日志获取详细错误信息
- 使用Unity Profiler监控性能

## 性能优化

1. **视频优化**
   - 使用适当的视频压缩格式
   - 考虑使用流式加载
   - 预加载关键视频片段

2. **UI优化**
   - 使用对象池管理按钮
   - 避免频繁的UI重建
   - 优化动画性能

3. **内存管理**
   - 及时释放不用的视频资源
   - 使用异步加载避免卡顿
   - 监控内存使用情况

## 许可证

本项目基于MIT许可证开源。

## 贡献

欢迎提交Issue和Pull Request来改进这个项目。

## 联系方式

如有问题或建议，请通过GitHub Issues联系。
