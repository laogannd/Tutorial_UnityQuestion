# VRQuestion - World Space Quiz UI Module

A plug-and-play World Space quiz module for Unity, compatible with the AutoHand plugin. Supports single-choice and multiple-choice questions, configurable feedback, and full event callbacks.

## Directory Structure

```
Question/
├── Core/        Base data types
│   ├── QuestionType.cs          Single / Multiple choice enum
│   ├── AnswerOption.cs          Individual answer option
│   └── AnswerResult.cs          Answer result data
├── Config/      ScriptableObject assets
│   ├── QuestionData.cs          Single question
│   ├── QuestionSet.cs           Question set / exam
│   └── FeedbackConfig.cs        Feedback appearance parameters
├── Events/      Events / interfaces
│   ├── QuestionEvents.cs               UnityEvent subclasses
│   ├── IQuestionFeedback.cs            Feedback strategy extension interface
│   ├── OptionVisualState.cs            Visual state enum
│   ├── ScriptableEventQuestionData.cs  Soap global QuestionData event asset
│   ├── EventListenerQuestionData.cs    Soap event listener (Inspector-visible)
│   └── QuestionDataEventInvoker.cs     UnityEvent dispatcher
└── UI/          Runtime components
    ├── OptionButton.cs          Answer option button (IPointerClickHandler, requires LayoutElement)
    ├── QuestionPanel.cs         Main panel controller (FaceCamera / Soap events / FadeIn-FadeOut)
    ├── QuestionSetRunner.cs     Question set flow controller
    ├── UIPokeBridge.cs          Finger poke bridge (BoxCollider + HandTouchEvent → EventSystem)
    ├── FaceCamera.cs            Face headset (merged into QuestionPanel, kept for standalone Canvas reuse)
    ├── SoapNotificationFeedback.cs   Soap Notification integration
    └── HapticFeedback.cs        Controller haptic feedback
```

## AutoHand Compatibility

`OptionButton` implements Unity's standard interfaces `IPointerClickHandler / IPointerEnterHandler / IPointerExitHandler`. AutoHand's `HandCanvasPointer + AutoInputModule` dispatches events through the Unity EventSystem pipeline. **No extra adapter code required.**

Requirements in the scene:
1. `Canvas` RenderMode set to `World Space`
2. An `AutoInputModule` present (AutoHand creates one automatically)
3. A `HandCanvasPointer` on the controller (AutoHand provides a UIPointer prefab)

QuestionPanel then works with **ray-based hand interaction** out of the box.

For **direct finger poke interaction**, see the [Finger Poke Interaction (UIPokeBridge)](#finger-poke-interaction-uipokebridge) section below.

## Quick Setup (Plug and Play)

### 1. Create Question Assets
In the Project view, right-click → **Create → VR Question → Question Data**. Fill in the question text, options, correct flags, and explanation.

Optional: create a **Question Set** to bundle multiple questions; create a **Feedback Config** to configure appearance, audio, and scoring rules.

### 2. Set Up the Canvas
- Create a `Canvas`, set RenderMode to `World Space`, add a `BoxCollider` covering the full panel (used as the raycast hit volume).
- Assign the Canvas to the UI Layer, matching `HandCanvasPointer.UILayer`.

### 3. Create the OptionButton Prefab
- A GameObject with an `Image / Graphic` component, with `OptionButton.cs` attached.
- Add child objects for `TMP_Text` (content, index label, selection mark, correct/wrong icons).
- Wire all Inspector references on the OptionButton component.

### 4. Set Up QuestionPanel
- Attach `QuestionPanel.cs` to a child of the Canvas.
- References: question TMP label, options container RectTransform (VerticalLayoutGroup + ContentSizeFitter on ScrollView Content), ScrollRect (optional), OptionButton prefab, Submit button, Reset/Close buttons (optional), AudioSource, FeedbackConfig.
- **Face Camera**: enable `Face Camera Enabled` to make the panel face the player's headset; `Lock Y Axis` keeps the panel upright.
- **Soap Event Driven** (optional): drag in a `ScriptableEventQuestionData` asset and the panel root's `CanvasGroup` to enable FadeOut → switch question → FadeIn on `Raise()`.
- Drag a QuestionData asset into the `Question` field as the initial question.

### 5. Play
Enter Play Mode, raise the AutoHand controller, pull the trigger, and point the ray at an option to answer. If UIPokeBridge is configured, you can also poke buttons directly with your fingers.

## Finger Poke Interaction (UIPokeBridge)

`UIPokeBridge` bridges AutoHand's physical collision detection to Unity's EventSystem, enabling any UI element that implements `IPointerClickHandler` to be activated by direct finger contact.

### How It Works

```
Hand Rigidbody hits BoxCollider
  → CollisionTracker.OnCollisionFirstEnter
  → Hand.OnCollisionFirstEnter finds HandTouchEvent
  → HandTouchEvent.Touch(hand)
  → UIPokeBridge.OnHandTouch
  → ExecuteEvents.Execute<IPointerClickHandler>
  → OptionButton.OnPointerClick (triggers selection logic)
```

### Setup Steps

**1. Add the component to the OptionButton prefab**
Select the OptionButton prefab root → Add Component → **VRQuestion > UI Poke Bridge**.
BoxCollider and HandTouchEvent are created automatically at runtime.

**2. Add the component to Submit / Reset / Close buttons**
Same process — add `UI Poke Bridge` to each button GameObject.

**3. Check the Physics collision matrix**
Project Settings → Physics → Layer Collision Matrix:
Ensure the layer of the UI elements and the Hand layer (usually `Hand`) are **allowed to collide**.

### Inspector Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| Collider Depth | 0.008 m | BoxCollider Z-axis thickness; 5–10 mm recommended |
| Collider Forward Offset | 0.004 m | Forward shift of the collider; gives a subtle press-in feel |
| Poke Cooldown | 0.4 s | Minimum time between pokes; prevents jitter re-triggers |
| Enable Hover Bridge | true | Fires IPointerEnter/Exit on contact for hover highlight |
| Poke Haptic Amplitude | 0.3 | Haptic strength on poke (0–1) |
| Poke Haptic Duration | 0.05 s | Haptic duration on poke |
| Hover Haptic Amplitude | 0.1 | Haptic strength on hover contact |
| Hover Haptic Duration | 0.02 s | Haptic duration on hover |

### Coexistence with Ray Interaction

Both interaction modes **work simultaneously without conflict**:
- Ray interaction requires pulling the trigger; poke is a physical collision — they cannot happen at the same time.
- Single-choice logic is idempotent for an already-selected button.
- The cooldown provides additional protection against edge-case double-fires.

## Option Layout (ScrollView Content)

`_optionsContainer` must be the ScrollView's `Content` node with:
- `VerticalLayoutGroup`: Child Alignment `Upper Center`, Control Child Size/Width enabled, Force Expand Width enabled; **do not** enable Force Expand Height.
- `ContentSizeFitter`: Vertical Fit = `Preferred Size`, Horizontal Fit = `Unconstrained`.
- OptionButton prefab has `[RequireComponent(typeof(LayoutElement))]`; set `Preferred Height` in the Inspector to control per-item height.

On question switch, `QuestionPanel` automatically calls `LayoutRebuilder.ForceRebuildLayoutImmediate` and resets `verticalNormalizedPosition = 1f` to scroll back to the top.

## Event-Driven Loading (Soap Global Events)

Call `_event.Raise(question)` from anywhere to switch the question on all subscribed QuestionPanels.

### 1. Create the Event Asset
Project → Create → **Soap → ScriptableEvents → QuestionData** → generates `scriptable_event_question_data.asset`.

### 2. Subscribe
Drag the event asset into QuestionPanel's `Soap Event Driven → Question Data Event` field, and attach a `CanvasGroup` for FadeIn/FadeOut.

### 3. Trigger
- **UnityEvent**: attach `QuestionDataEventInvoker` to any GameObject, configure the event asset and question asset, call `Raise()`.
- **Code**: `questionDataEvent.Raise(myQuestion)`.
- **Inspector listener**: attach `EventListenerQuestionData` on another object and bind additional logic (analytics, animation) in the Response field.

## Event Callbacks

### Inspector (UnityEvent)
```
QuestionPanel exposes:
- OnQuestionPresented (QuestionData)
- OnOptionSelected   (AnswerOption)
- OnOptionDeselected (AnswerOption)
- OnAnswerSubmitted  (AnswerResult)
- OnAnswerCorrect    (AnswerResult)
- OnAnswerWrong      (AnswerResult)
- OnPanelClosed      ()
- OnTimeOut          ()
```

### Code (C# event)
```csharp
panel.QuestionPresented += q => Debug.Log($"Question: {q.QuestionText}");
panel.OptionSelected    += o => Debug.Log($"Selected: {o.Content}");
panel.AnswerSubmitted   += r => Debug.Log($"Score: {r.Score}, Correct: {r.IsAllCorrect}");
```

## Custom Feedback Extension (IQuestionFeedback)

Implement the interface and inject it via `RegisterFeedback` to add any feedback logic without modifying the panel:

```csharp
public class MyAnalyticsFeedback : MonoBehaviour, IQuestionFeedback
{
    public void OnAnswerSubmitted(AnswerResult result)
    {
        // Report analytics / scoring service / save progress...
    }
    // Other methods can be left empty
}
```

Attach it to the same GameObject as QuestionPanel, then call `panel.RegisterFeedback(this)` in `OnEnable`.

Two built-in examples are included:
- `SoapNotificationFeedback` — reuses the project's `ScriptableEventNotification` to show a notification.
- `HapticFeedback` — sends haptic impulse commands to the controller.

## Runtime Question Switching

```csharp
// Single question
panel.Present(myQuestion);

// Question set
runner.StartRun(mySet);
runner.Next();
```

## Performance and Conventions

- Option buttons use an object pool; switching questions does not Instantiate or Destroy.
- Zero LINQ and zero string concatenation in the answer flow (except final result display).
- All coroutines are stopped on Submit / Close / Reset with no dangling references.
- No List allocations inside loops; event callbacks use `IReadOnlyList` to prevent external mutation.
- All Unity object null checks use `==`, never `is null`.
- Missing configuration causes fast failure (`Debug.LogError` + return) rather than silent fallback.

## Integration with Other Systems

- **Soap (Obvious)**: `SoapNotificationFeedback` directly reuses `ScriptableEventNotification` + `NotificationData`.
- **AutoHand**: `OptionButton` integrates via standard EventSystem interfaces; no AutoHand namespace reference required. `UIPokeBridge` adds physical poke support without modifying AutoHand source.
- **VRNotificationPanel**: can serve as the global display outlet for answer feedback, consuming the notification event automatically.
