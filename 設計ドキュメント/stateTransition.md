# ai-MyNotes 状態遷移図

## アプリケーション全体状態遷移

```mermaid
stateDiagram-v2
    [*] --> AppInitializing : アプリ起動
    
    AppInitializing --> MemoEdit_New : IndexedDB接続成功
    AppInitializing --> ErrorState : IndexedDB接続失敗
    
    state MemoEdit_New {
        [*] --> EmptyEditor : 新規メモ編集画面表示
        EmptyEditor --> UserTyping : ユーザー入力開始
        UserTyping --> SaveTimerActive : 3秒停止タイマー開始
        UserTyping --> FocusLost : フォーカス離脱
        SaveTimerActive --> AutoSaving : タイマー満了（3秒経過）
        FocusLost --> ImmediateSaving : タイマーキャンセル・即座保存
        AutoSaving --> Saved : 保存完了
        ImmediateSaving --> Saved : 保存完了
        Saved --> UserTyping : 継続入力
        UserTyping --> NavigatingToList : メモ一覧ボタンタップ
    }
    
    state MemoEdit_Existing {
        [*] --> LoadingMemo : 既存メモ読み込み
        LoadingMemo --> DisplayingContent : メモ内容表示
        LoadingMemo --> LoadError : 読み込み失敗
        DisplayingContent --> UserEditing : ユーザー編集開始
        UserEditing --> UpdateTimerActive : 3秒停止タイマー開始
        UserEditing --> FocusLost : フォーカス離脱
        UpdateTimerActive --> AutoUpdating : タイマー満了（3秒経過）
        FocusLost --> ImmediateUpdating : タイマーキャンセル・即座更新
        AutoUpdating --> Updated : 更新完了
        ImmediateUpdating --> Updated : 更新完了
        Updated --> UserEditing : 継続編集
        UserEditing --> NavigatingToList : メモ一覧ボタンタップ
    }
    
    state MemoList {
        [*] --> LoadingList : メモ一覧読み込み
        LoadingList --> DisplayingList : 一覧表示
        LoadingList --> EmptyList : メモ0件
        LoadingList --> LoadListError : 読み込み失敗
        
        DisplayingList --> CardSelected : メモカードタップ
        DisplayingList --> SwipeStarted : 左スワイプ開始
        DisplayingList --> NewMemoCreation : 新規作成ボタンタップ
        
        EmptyList --> NewMemoCreation : 新規作成ボタンタップ
        
        SwipeStarted --> SwipeInProgress : スワイプ進行中
        SwipeInProgress --> SwipeCanceled : スワイプ距離不足・キャンセル
        SwipeInProgress --> DeleteButtonRevealed : スワイプ距離100px以上
        SwipeCanceled --> DisplayingList : 通常表示に復帰
        
        DeleteButtonRevealed --> ConfirmingDelete : 削除ボタンタップ
        DeleteButtonRevealed --> SwipeCanceled : 他の場所タップ・右スワイプ
        
        ConfirmingDelete --> DeletingMemo : 削除確認
        ConfirmingDelete --> SwipeCanceled : 削除キャンセル
        
        DeletingMemo --> RefreshingList : 削除完了
        DeletingMemo --> DeleteError : 削除失敗
        
        RefreshingList --> DisplayingList : 一覧更新完了
        RefreshingList --> EmptyList : 削除後0件
    }
    
    %% 画面間遷移
    MemoEdit_New --> MemoList : NavigatingToList
    MemoEdit_Existing --> MemoList : NavigatingToList
    MemoList --> MemoEdit_New : NewMemoCreation
    MemoList --> MemoEdit_Existing : CardSelected
    
    %% エラー状態からの復旧
    ErrorState --> MemoEdit_New : リトライ成功
    LoadError --> MemoList : 一覧画面に戻る
    LoadListError --> MemoEdit_New : 新規作成画面に遷移
    DeleteError --> DisplayingList : エラー表示後一覧に戻る
```

## メモ編集画面の詳細状態遷移

```mermaid
stateDiagram-v2
    [*] --> InitializingEditor : エディター初期化
    
    state InitializingEditor {
        [*] --> CheckingMode : モード判定
        CheckingMode --> NewMemoMode : 新規作成モード
        CheckingMode --> ExistingMemoMode : 編集モード（ID指定）
    }
    
    state NewMemoMode {
        [*] --> EmptyTextArea : 空のテキストエリア表示
        EmptyTextArea --> FirstInput : 初回入力
        FirstInput --> InputActive : 入力アクティブ状態
    }
    
    state ExistingMemoMode {
        [*] --> LoadingMemoData : メモデータ読み込み
        LoadingMemoData --> MemoLoaded : データ読み込み完了
        LoadingMemoData --> LoadError : 読み込み失敗
        MemoLoaded --> InputActive : 編集可能状態
        LoadError --> ErrorDisplay : エラーメッセージ表示
        ErrorDisplay --> EmptyTextArea : フォールバック
    }
    
    state InputActive {
        [*] --> WaitingInput : 入力待機
        WaitingInput --> TypingInProgress : キーボード入力中
        TypingInProgress --> TimerStarted : 入力停止・タイマー開始
        TypingInProgress --> FocusLostImmediate : フォーカス離脱
        
        TimerStarted --> TimerCounting : 3秒カウントダウン
        TimerStarted --> TypingResumed : 入力再開（タイマーリセット）
        TimerStarted --> FocusLostCancel : フォーカス離脱（タイマーキャンセル）
        
        TimerCounting --> AutoSaveTriggered : 3秒経過
        TimerCounting --> TypingResumed : 入力再開（タイマーキャンセル）
        TimerCounting --> FocusLostCancel : フォーカス離脱（タイマーキャンセル）
        
        FocusLostImmediate --> ImmediateSaveTriggered : 即座保存開始
        FocusLostCancel --> ImmediateSaveTriggered : 優先保存開始
        
        AutoSaveTriggered --> SavingInProgress : 自動保存実行
        ImmediateSaveTriggered --> SavingInProgress : 即座保存実行
        
        SavingInProgress --> SaveCompleted : 保存成功
        SavingInProgress --> SaveFailed : 保存失敗
        
        SaveCompleted --> WaitingInput : 入力待機に復帰
        SaveFailed --> RetryPrompt : リトライ確認
        
        RetryPrompt --> SavingInProgress : リトライ実行
        RetryPrompt --> WaitingInput : リトライ中止
        
        TypingResumed --> TypingInProgress : 入力継続
    }
    
    InitializingEditor --> NavigationRequested : メモ一覧ボタンタップ
    InputActive --> NavigationRequested : ナビゲーション要求
    
    NavigationRequested --> CheckingUnsavedData : 未保存データ確認
    CheckingUnsavedData --> SafeNavigation : 保存済み
    CheckingUnsavedData --> ForcedSave : 未保存データ検出
    
    ForcedSave --> SafeNavigation : 強制保存完了
    ForcedSave --> NavigationWithLoss : 保存失敗（データ損失）
    
    SafeNavigation --> [*] : 画面遷移実行
    NavigationWithLoss --> [*] : 画面遷移実行（警告表示）
```

## メモ一覧画面の詳細状態遷移

```mermaid
stateDiagram-v2
    [*] --> InitializingList : 一覧画面初期化
    
    InitializingList --> LoadingMemoList : メモ一覧読み込み
    LoadingMemoList --> CheckingMemoCount : データ取得完了
    LoadingMemoList --> LoadError : データ取得失敗
    
    CheckingMemoCount --> EmptyStateDisplay : メモ0件
    CheckingMemoCount --> ListRendering : メモ1件以上
    
    state EmptyStateDisplay {
        [*] --> ShowingEmptyMessage : 「メモがありません」表示
        ShowingEmptyMessage --> NewMemoCreation : 新規作成ボタンタップ
    }
    
    state ListRendering {
        [*] --> SortingMemos : 作成日時降順ソート
        SortingMemos --> RenderingCards : カード形式レンダリング
        RenderingCards --> DisplayCompleted : 表示完了
        
        DisplayCompleted --> CardInteraction : ユーザー操作待機
        
        state CardInteraction {
            [*] --> WaitingUserAction : 操作待機
            WaitingUserAction --> CardTapped : カードタップ
            WaitingUserAction --> SwipeDetected : 左スワイプ検知
            WaitingUserAction --> NewMemoRequested : 新規作成ボタン
            
            CardTapped --> NavigatingToEdit : 編集画面遷移
            NewMemoRequested --> NavigatingToNew : 新規作成画面遷移
            
            SwipeDetected --> SwipeValidation : スワイプ距離判定
            SwipeValidation --> SwipeRejected : 100px未満
            SwipeValidation --> SwipeAccepted : 100px以上
            
            SwipeRejected --> WaitingUserAction : 操作待機に復帰
            SwipeAccepted --> DeleteButtonShown : 削除ボタン表示
            
            state DeleteButtonShown {
                [*] --> ButtonVisible : 削除ボタン表示中
                ButtonVisible --> DeleteButtonTapped : 削除ボタンタップ
                ButtonVisible --> SwipeCancellation : キャンセル操作
                
                SwipeCancellation --> HidingButton : ボタン非表示
                HidingButton --> WaitingUserAction : 通常表示復帰
                
                DeleteButtonTapped --> ConfirmationDialog : 確認ダイアログ表示
                
                state ConfirmationDialog {
                    [*] --> ShowingDialog : ダイアログ表示中
                    ShowingDialog --> DeleteConfirmed : 削除確認
                    ShowingDialog --> DeleteCanceled : 削除キャンセル
                    
                    DeleteCanceled --> HidingDialog : ダイアログ非表示
                    HidingDialog --> ButtonVisible : 削除ボタン表示に復帰
                    
                    DeleteConfirmed --> ExecutingDelete : 削除処理実行
                    ExecutingDelete --> DeleteSuccessful : 削除成功
                    ExecutingDelete --> DeleteFailed : 削除失敗
                    
                    DeleteSuccessful --> RefreshingList : 一覧更新
                    DeleteFailed --> ErrorHandling : エラー処理
                    
                    ErrorHandling --> ShowingError : エラーメッセージ表示
                    ShowingError --> ButtonVisible : 削除ボタン表示に復帰
                }
            }
        }
        
        RefreshingList --> LoadingMemoList : 一覧再読み込み
    }
    
    NavigatingToEdit --> [*] : 編集画面へ遷移
    NavigatingToNew --> [*] : 新規作成画面へ遷移
    NewMemoCreation --> [*] : 新規作成画面へ遷移
    
    LoadError --> ErrorRecovery : エラー回復処理
    ErrorRecovery --> RetryLoading : 再試行
    ErrorRecovery --> FallbackNavigation : フォールバック遷移
    
    RetryLoading --> LoadingMemoList : 一覧読み込み再実行
    FallbackNavigation --> [*] : 新規作成画面へ遷移
```

## 左スワイプ削除の詳細状態遷移

```mermaid
stateDiagram-v2
    [*] --> CardNormalState : 通常のカード表示
    
    CardNormalState --> SwipeStartDetected : タッチ開始・左方向検知
    CardNormalState --> OtherGesture : その他のタッチ操作
    
    OtherGesture --> CardNormalState : 通常操作として処理
    
    SwipeStartDetected --> SwipeTracking : スワイプ追跡開始
    
    state SwipeTracking {
        [*] --> MeasuringDistance : 距離測定中
        MeasuringDistance --> SwipeInProgress : スワイプ継続中
        MeasuringDistance --> SwipeAborted : スワイプ中断
        
        SwipeInProgress --> DistanceCheck : 距離判定
        DistanceCheck --> InsufficientDistance : 100px未満
        DistanceCheck --> SufficientDistance : 100px以上
        
        InsufficientDistance --> SwipeFailed : スワイプ失敗
        SufficientDistance --> SwipeSuccessful : スワイプ成功
        
        SwipeAborted --> SwipeCanceled : スワイプキャンセル
    }
    
    SwipeFailed --> AnimatingBack : 元位置に戻るアニメーション
    SwipeCanceled --> AnimatingBack : 元位置に戻るアニメーション
    AnimatingBack --> CardNormalState : 通常状態に復帰
    
    SwipeSuccessful --> RevealingDeleteButton : 削除ボタン表示アニメーション
    RevealingDeleteButton --> DeleteButtonVisible : 削除ボタン表示完了
    
    state DeleteButtonVisible {
        [*] --> WaitingButtonAction : ボタン操作待機
        WaitingButtonAction --> DeleteButtonPressed : 削除ボタンタップ
        WaitingButtonAction --> OutsideAreaTapped : 他エリアタップ
        WaitingButtonAction --> RightSwipeDetected : 右スワイプ検知
        
        OutsideAreaTapped --> HidingButton : ボタン非表示処理
        RightSwipeDetected --> HidingButton : ボタン非表示処理
        HidingButton --> AnimatingBack : 元位置復帰アニメーション
        
        DeleteButtonPressed --> ShowingConfirmDialog : 確認ダイアログ表示
        
        state ShowingConfirmDialog {
            [*] --> DialogVisible : ダイアログ表示中
            DialogVisible --> ConfirmPressed : 削除確認ボタン
            DialogVisible --> CancelPressed : キャンセルボタン
            
            CancelPressed --> HidingDialog : ダイアログ非表示
            HidingDialog --> WaitingButtonAction : ボタン操作待機に復帰
            
            ConfirmPressed --> ProcessingDelete : 削除処理開始
            
            state ProcessingDelete {
                [*] --> DeletingFromDB : IndexedDBから削除
                DeletingFromDB --> DeleteCompleted : 削除完了
                DeletingFromDB --> DeleteError : 削除エラー
                
                DeleteCompleted --> UpdatingUI : UI更新処理
                UpdatingUI --> RemovingCardAnimation : カード削除アニメーション
                RemovingCardAnimation --> ListRefreshed : 一覧更新完了
                
                DeleteError --> ShowingErrorMessage : エラーメッセージ表示
                ShowingErrorMessage --> WaitingButtonAction : ボタン操作待機に復帰
            }
        }
    }
    
    ListRefreshed --> [*] : 一覧画面更新完了
    CardNormalState --> [*] : 他の操作による画面遷移
```

## データ保存状態遷移（リアルタイム保存）

```mermaid
stateDiagram-v2
    [*] --> DataIdle : データ待機状態
    
    DataIdle --> InputDetected : ユーザー入力検知
    InputDetected --> TimerActive : 3秒タイマー開始
    
    state TimerActive {
        [*] --> CountingDown : カウントダウン中
        CountingDown --> TimerExpired : 3秒経過
        CountingDown --> TimerReset : 追加入力検知
        CountingDown --> TimerCanceled : フォーカス離脱
        
        TimerReset --> CountingDown : カウント再開
        TimerExpired --> AutoSaveTriggered : 自動保存開始
        TimerCanceled --> ForcedSaveTriggered : 強制保存開始（優先）
    }
    
    AutoSaveTriggered --> ValidatingData : データ検証
    ForcedSaveTriggered --> ValidatingData : データ検証
    
    state ValidatingData {
        [*] --> CheckingContent : コンテンツ確認
        CheckingContent --> ContentValid : 有効なデータ
        CheckingContent --> ContentEmpty : 空データ
        CheckingContent --> ContentInvalid : 無効データ
        
        ContentValid --> ExtractingTitle : タイトル抽出
        ContentEmpty --> SkippingSave : 保存スキップ
        ContentInvalid --> SkippingSave : 保存スキップ
        
        ExtractingTitle --> TitleExtracted : 1行目抽出完了
        TitleExtracted --> PreparingSaveData : 保存データ準備
    }
    
    PreparingSaveData --> ExecutingSave : 保存実行
    
    state ExecutingSave {
        [*] --> WritingToIndexedDB : IndexedDBへ書き込み
        WritingToIndexedDB --> SaveSuccessful : 書き込み成功
        WritingToIndexedDB --> SaveFailed : 書き込み失敗
        
        SaveSuccessful --> UpdateTimestamp : タイムスタンプ更新
        UpdateTimestamp --> NotifySuccess : 成功通知
        
        SaveFailed --> CheckingRetry : リトライ判定
        CheckingRetry --> RetryingSave : リトライ実行
        CheckingRetry --> NotifyFailure : リトライ断念
        
        RetryingSave --> WritingToIndexedDB : 再書き込み試行
    }
    
    NotifySuccess --> ShowSaveIndicator : 保存完了インジケーター
    ShowSaveIndicator --> DataIdle : 待機状態復帰
    
    NotifyFailure --> ShowErrorIndicator : エラーインジケーター
    ShowErrorIndicator --> DataIdle : 待機状態復帰
    
    SkippingSave --> DataIdle : 待機状態復帰
    
    %% 継続入力時の状態遷移
    DataIdle --> InputDetected : 継続入力
```

## エラー状態遷移

```mermaid
stateDiagram-v2
    [*] --> NormalOperation : 正常動作
    
    NormalOperation --> ErrorDetected : エラー発生
    
    state ErrorDetected {
        [*] --> ErrorClassification : エラー分類
        ErrorClassification --> DatabaseError : DB接続・操作エラー
        ErrorClassification --> NetworkError : ネットワークエラー
        ErrorClassification --> UIError : UI操作エラー
        ErrorClassification --> ValidationError : データ検証エラー
        ErrorClassification --> SwipeError : スワイプ操作エラー
    }
    
    state DatabaseError {
        [*] --> DBConnectionFailed : DB接続失敗
        [*] --> DBWriteFailed : DB書き込み失敗
        [*] --> DBReadFailed : DB読み込み失敗
        [*] --> DBDeleteFailed : DB削除失敗
        
        DBConnectionFailed --> RetryConnection : 接続リトライ
        DBWriteFailed --> RetrySave : 保存リトライ
        DBReadFailed --> FallbackDisplay : フォールバック表示
        DBDeleteFailed --> RetryDelete : 削除リトライ
        
        RetryConnection --> ConnectionRestored : 接続復旧
        RetryConnection --> PermanentFailure : 恒久的失敗
        
        RetrySave --> SaveRestored : 保存復旧
        RetrySave --> SaveAbandoned : 保存断念
        
        RetryDelete --> DeleteRestored : 削除復旧  
        RetryDelete --> DeleteAbandoned : 削除断念
    }
    
    state NetworkError {
        [*] --> OfflineDetected : オフライン検知
        OfflineDetected --> OfflineMode : オフラインモード移行
        OfflineMode --> OnlineRestored : オンライン復旧
        OfflineMode --> ContinueOffline : オフライン継続
    }
    
    state UIError {
        [*] --> RenderingError : レンダリングエラー
        [*] --> NavigationError : 画面遷移エラー
        [*] --> InputError : 入力処理エラー
        
        RenderingError --> RefreshUI : UI再描画
        NavigationError --> FallbackRoute : フォールバックルート
        InputError --> ResetInput : 入力リセット
    }
    
    state ValidationError {
        [*] --> InvalidInput : 無効入力
        [*] --> DataCorruption : データ破損
        
        InvalidInput --> ShowValidationMessage : 検証メッセージ表示
        DataCorruption --> DataRecovery : データ回復処理
    }
    
    state SwipeError {
        [*] --> SwipeDetectionFailed : スワイプ検知失敗
        [*] --> SwipeAnimationError : アニメーションエラー
        
        SwipeDetectionFailed --> FallbackToButton : ボタン操作フォールバック
        SwipeAnimationError --> SkipAnimation : アニメーションスキップ
    }
    
    %% 復旧処理
    ConnectionRestored --> NormalOperation
    SaveRestored --> NormalOperation
    DeleteRestored --> NormalOperation
    OnlineRestored --> NormalOperation
    RefreshUI --> NormalOperation
    FallbackRoute --> NormalOperation
    ResetInput --> NormalOperation
    ShowValidationMessage --> NormalOperation
    FallbackToButton --> NormalOperation
    SkipAnimation --> NormalOperation
    
    %% 恒久的失敗処理
    PermanentFailure --> GracefulDegradation : 機能縮退
    SaveAbandoned --> DataLossWarning : データ損失警告
    DeleteAbandoned --> OperationFailed : 操作失敗通知
    ContinueOffline --> OfflineNotification : オフライン通知
    DataRecovery --> NormalOperation
    
    GracefulDegradation --> NormalOperation : 縮退モードで継続
    DataLossWarning --> NormalOperation : 警告後継続
    OperationFailed --> NormalOperation : 失敗通知後継続
    OfflineNotification --> NormalOperation : 通知後継続
```

---

## 状態遷移設計のポイント

### 1. リアルタイム保存の優先制御
- **フォーカス離脱優先**: テキストエリアからフォーカスが外れた際は、3秒タイマーをキャンセルして即座に保存
- **タイマーリセット**: 継続入力時は既存タイマーをキャンセルし、新しい3秒タイマーを開始
- **保存状態の明確化**: 保存中・保存完了・保存失敗の各状態を明確に分離

### 2. 左スワイプ削除の精密制御
- **距離判定**: 100px以上のスワイプで削除ボタン表示
- **キャンセル機能**: スワイプ途中でのキャンセル、他エリアタップでのキャンセル
- **確認プロセス**: 削除ボタンタップ後の確認ダイアログによる安全性確保

### 3. エラーハンドリングの網羅
- **分類別処理**: DB・ネットワーク・UI・バリデーション・スワイプの各エラーを個別処理
- **復旧戦略**: リトライ・フォールバック・縮退モードによる段階的復旧
- **ユーザビリティ**: エラー時でも基本機能を継続可能な設計

### 4. 2画面構成の最適化
- **シンプルな遷移**: メモ編集 ↔ メモ一覧の直接的な遷移
- **状態保持**: 各画面の状態を適切に管理し、遷移時のデータ整合性を確保
- **レスポンシブ対応**: iOS Chrome環境での最適な動作を保証

この状態遷移図により、ai-MyNotesアプリの全ての動作パターンと例外処理が明確に定義され、開発時の実装指針として活用できます。