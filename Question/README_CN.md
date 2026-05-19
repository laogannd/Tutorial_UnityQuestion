# VRQuestion - World Space 答题UI模块

适配 AutoHand 插件的即插即用 World Space 答题模块，支持单选/多选、可配置反馈、完整事件回调。

## 目录结构

```
Question/
├── Core/        基础数据类型
│   ├── QuestionType.cs          单选 / 多选枚举
│   ├── AnswerOption.cs          单个选项
│   └── AnswerResult.cs          答题结果数据
├── Config/      ScriptableObject 配置
│   ├── QuestionData.cs          单道题目
│   ├── QuestionSet.cs           题集 / 试卷
│   └── FeedbackConfig.cs        反馈外观参数
├── Events/      事件 / 接口
│   ├── QuestionEvents.cs               UnityEvent 子类
│   ├── IQuestionFeedback.cs            反馈策略扩展接口
│   ├── OptionVisualState.cs            视觉状态枚举
│   ├── ScriptableEventQuestionData.cs  Soap全局QuestionData事件资产
│   ├── EventListenerQuestionData.cs    Soap事件监听器（Inspector可视化）
│   └── QuestionDataEventInvoker.cs     UnityEvent发射器（与NotificationEventInvoker同模式）
└── UI/          运行时控件
    ├── OptionButton.cs          单选项按钮（IPointerClickHandler，自带LayoutElement）
    ├── QuestionPanel.cs         面板主控（内置FaceCamera/Soap事件订阅/FadeIn-FadeOut）
    ├── QuestionSetRunner.cs     题集流程控制
    ├── UIPokeBridge.cs          手指点触桥接（BoxCollider + HandTouchEvent → EventSystem）
    ├── FaceCamera.cs            朝向头显（已并入QuestionPanel，保留供独立Canvas复用）
    ├── SoapNotificationFeedback.cs   集成 Soap Notification 反馈
    └── HapticFeedback.cs        手柄震动反馈
```

## AutoHand 兼容原理

`OptionButton` 实现 Unity 标准接口 `IPointerClickHandler / IPointerEnterHandler / IPointerExitHandler`，AutoHand 的 `HandCanvasPointer + AutoInputModule` 通过 Unity EventSystem 标准管线分发事件。**无需任何额外适配代码**。

只要场景中：
1. `Canvas` 的 RenderMode 设为 `World Space`
2. 场景中存在 `AutoInputModule`（AutoHand 自动创建）
3. 手柄上挂有 `HandCanvasPointer`（AutoHand 提供 UIPointer 预制体）

QuestionPanel 即可直接被**手部射线**交互。

**手指直接点触**请参阅下方 [手指点触交互（UIPokeBridge）](#手指点触交互upokebridgecs) 章节。

## 快速搭建（即插即用）

### 1. 配置题目资产
在 Project 视图右键 → **Create → VR Question → Question Data**，填入题目文本、选项、是否正确、解析。

可选：创建 **Question Set** 把多道题打包成题集；创建 **Feedback Config** 配置反馈外观/音效/打分规则。

### 2. 搭建 Canvas
- 创建 `Canvas`，RenderMode 设为 `World Space`，加 `BoxCollider`（覆盖整个面板，作为射线命中体）
- 给 Canvas 设置 UI Layer，与 `HandCanvasPointer.UILayer` 保持一致

### 3. 制作 OptionButton 预制体
- 一个带 `Image / Graphic` 的 GameObject，挂 `OptionButton.cs`
- 子物体放 `TMP_Text` 显示选项内容、编号、勾选标记、对错图标
- 在 Inspector 把对应字段拖入 OptionButton 的引用

### 4. 制作 QuestionPanel
- Canvas 子物体上挂 `QuestionPanel.cs`
- 引用：题干 TMP、选项容器 RectTransform（VerticalLayoutGroup+ContentSizeFitter，对应 ScrollView 的 Content）、ScrollRect（可选）、OptionButton 预制体、提交按钮、（可选）重置/关闭按钮、AudioSource、FeedbackConfig
- Face Camera 区块：勾选 `Face Camera Enabled` 即让面板朝向玩家头显，`Lock Y Axis` 保持上方向竖直
- Soap Event Driven 区块（可选）：拖入 `ScriptableEventQuestionData` 资产 + 面板根节点的 `CanvasGroup`，即可通过 Raise 事件触发 FadeOut → 切题 → FadeIn
- 在 Inspector 拖入 QuestionData 作为初始题目

### 5. 运行
进入 Play Mode，举起 AutoHand 手柄、扣下扳机，射线指向选项点击即可作答。如已配置 UIPokeBridge，也可直接用手指点触按钮。

## 手指点触交互（UIPokeBridge.cs）

`UIPokeBridge` 将 AutoHand 的物理碰撞检测桥接到 Unity EventSystem，让任何实现了 `IPointerClickHandler` 的 UI 元素都能被手指直接点触激活。

### 工作原理

```
Hand Rigidbody 碰到 BoxCollider
  → CollisionTracker.OnCollisionFirstEnter
  → Hand.OnCollisionFirstEnter 查找 HandTouchEvent
  → HandTouchEvent.Touch(hand)
  → UIPokeBridge.OnHandTouch
  → ExecuteEvents.Execute<IPointerClickHandler>
  → OptionButton.OnPointerClick（触发选中逻辑）
```

### 配置步骤

**1. 给 OptionButton 预制体添加组件**
选中 OptionButton 预制体根节点 → Add Component → **VRQuestion > UI Poke Bridge**。
运行时自动创建 BoxCollider 和 HandTouchEvent，无需手动添加。

**2. 给 Submit / Reset / Close 按钮添加组件**
同上，在各按钮 GameObject 上添加 `UI Poke Bridge`。

**3. 检查物理碰撞矩阵**
Project Settings → Physics → Layer Collision Matrix：
确认 UI 元素所在 Layer 与 Hand 所在 Layer（通常为 `Hand`）之间**允许碰撞**。

### Inspector 参数说明

| 参数 | 默认值 | 说明 |
|------|--------|------|
| Collider Depth | 0.008 m | BoxCollider Z 轴厚度，建议 5–10 mm |
| Collider Forward Offset | 0.004 m | Collider 前移量，手指接触时有轻微下压感 |
| Poke Cooldown | 0.4 s | 两次点触之间的冷却，防止手指抖动重复触发 |
| Enable Hover Bridge | true | 碰触时同步触发 IPointerEnter/Exit，显示 Hover 高亮 |
| Poke Haptic Amplitude | 0.3 | 点触时手柄震动强度（0–1） |
| Poke Haptic Duration | 0.05 s | 点触震动时长 |
| Hover Haptic Amplitude | 0.1 | 悬停接触时震动强度 |
| Hover Haptic Duration | 0.02 s | 悬停震动时长 |

### 与射线交互共存

两种交互方式**天然互斥**，无需额外处理：
- 射线交互需要扣扳机，点触是物理碰撞，不会同时发生
- 单选逻辑对已选中按钮再次点击是幂等的
- 冷却时间额外防止边界情况下的重复触发

## 选项布局（ScrollView Content）

`_optionsContainer` 必须是 ScrollView 的 `Content` 节点，并满足：
- 挂 `VerticalLayoutGroup`：Child Alignment 推荐 `Upper Center`，开启 Control Child Size/Width，Force Expand Width；不要开 Force Expand Height
- 挂 `ContentSizeFitter`：Vertical Fit = `Preferred Size`，Horizontal Fit = `Unconstrained`
- OptionButton 预制体已加 `[RequireComponent(typeof(LayoutElement))]`，可在 Inspector 直接配置 `Preferred Height` 控制单项高度

切题时 `QuestionPanel` 会自动 `LayoutRebuilder.ForceRebuildLayoutImmediate` 并 `verticalNormalizedPosition = 1f` 回到顶部，确保选项从上到下顺序排列且滚动条复位。

## 事件驱动加载（Soap 全局事件）

任意位置 `_event.Raise(question)` 即可让所有订阅的 QuestionPanel 切题，等价于 NotificationEventInvoker 的模式：

### 1. 创建事件资产
Project → Create → **Soap → ScriptableEvents → QuestionData** 生成 `scriptable_event_question_data.asset`

### 2. 订阅
将事件资产拖到 QuestionPanel.`Soap Event Driven → Question Data Event`，并挂上 `Canvas Group` 实现 FadeIn/FadeOut。

### 3. 触发
- **UnityEvent**：任意 GameObject 挂 `QuestionDataEventInvoker`，配置事件资产 + 题目资产，调用 `Raise()` 即可
- **代码**：`questionDataEvent.Raise(myQuestion)` 直接触发
- **Inspector 监听**：另一侧挂 `EventListenerQuestionData`，在 Response 中绑定其他逻辑（如埋点、动画）

## 事件回调

### Inspector 可视化连线（UnityEvent）
```
QuestionPanel 上：
- OnQuestionPresented (QuestionData)
- OnOptionSelected   (AnswerOption)
- OnOptionDeselected (AnswerOption)
- OnAnswerSubmitted  (AnswerResult)
- OnAnswerCorrect    (AnswerResult)
- OnAnswerWrong      (AnswerResult)
- OnPanelClosed      ()
- OnTimeOut          ()
```

### 代码订阅（C# event）
```csharp
panel.QuestionPresented += q => Debug.Log($"展示题目: {q.QuestionText}");
panel.OptionSelected    += o => Debug.Log($"选中: {o.Content}");
panel.AnswerSubmitted   += r => Debug.Log($"得分: {r.Score}, 正确: {r.IsAllCorrect}");
```

## 扩展自定义反馈（IQuestionFeedback）

实现接口并通过 `RegisterFeedback` 注入，即可在不修改面板代码的情况下加入任意反馈逻辑：

```csharp
public class MyAnalyticsFeedback : MonoBehaviour, IQuestionFeedback
{
    public void OnAnswerSubmitted(AnswerResult result)
    {
        // 上报埋点 / 打分服务 / 保存进度...
    }
    // 其他方法可空实现
}
```

挂到 QuestionPanel 同节点上，在 OnEnable 中 `panel.RegisterFeedback(this)` 即可。

模块自带两个示例：
- `SoapNotificationFeedback` — 复用项目现有 `ScriptableEventNotification` 显示通知
- `HapticFeedback`           — 输出手柄震动指令

## 运行时切换题目

```csharp
// 单题
panel.Present(myQuestion);

// 题集
runner.StartRun(mySet);
runner.Next();
```

## 性能与规范

- 选项按钮采用对象池，切题不会频繁 Instantiate / Destroy
- 答题流程零 LINQ、零字符串拼接（除最终结果展示）
- Coroutine 在 Submit/Close/Reset 处全部停止，无遗漏引用
- 不在循环中分配 List；事件回调使用 IReadOnlyList 避免外部修改
- 所有 Unity 对象 null 检查使用 `==`，未使用 `is null`
- 配置缺失采取快速失败（Debug.LogError 并 return），不静默回退

## 与项目其他系统集成

- **Soap (Obvious)**：`SoapNotificationFeedback` 直接复用 `ScriptableEventNotification` + `NotificationData`
- **AutoHand**：`OptionButton` 通过标准 EventSystem 接口对接，无需引用 AutoHand 命名空间
- **VRNotificationPanel**：可作为答题反馈的全局展示出口，自动读取通知事件
