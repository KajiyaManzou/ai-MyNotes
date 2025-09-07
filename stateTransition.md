# ai-MyNotes アプリケーション状態遷移図

## 1. 全体状態遷移

```mermaid
stateDiagram-v2
    [*] --> AppInitializing : アプリ起動
    
    AppInitializing --> IndexedDBInitializing : データベース初期化
    IndexedDBInitializing --> AppReady : 初期化完了
    IndexedDBInitializing --> AppError : 初期化失敗
    
    AppReady --> MemoEditingState : メモ編集状態
    AppReady --> MemoListState : メモ一覧状態
    AppReady --> MemoListEditState : メモ一覧編集状態
    
    MemoEditingState --> MemoListState : 一覧表示
    MemoListState --> MemoEditingState : 編集開始
    MemoListState --> MemoListEditState : 編集モード
    MemoListEditState --> MemoListState : 通常モード
    
    AppError --> AppInitializing : 再初期化
    
    state MemoEditingState {
        [*] --> EditIdle
        EditIdle --> EditTyping : ユーザー入力開始
        EditTyping --> EditDebouncing : 入力一時停止
        EditDebouncing --> EditSaving : 3秒経過
        EditDebouncing --> EditTyping : 入力再開
        EditSaving --> EditSaveSuccess : 保存成功
        EditSaving --> EditSaveError : 保存失敗
        EditSaveSuccess --> EditIdle : 保存完了
        EditSaveError --> EditRetrying : リトライ処理
        EditRetrying --> EditSaving : 再保存試行
        EditRetrying --> EditIdle : リトライ中止
        
        EditIdle --> EditFocusOut : フォーカス離脱
        EditFocusOut --> EditSaving : 即座保存
        EditTyping --> EditFocusOut : フォーカス離脱
    }
    
    state MemoListState {
        [*] --> ListLoading
        ListLoading --> ListDisplaying : データ取得成功
        ListLoading --> ListError : データ取得失敗
        ListDisplaying --> ListEmpty : メモ0件
        ListDisplaying --> ListPopulated : メモ存在
        ListError --> ListRetrying : リトライ
        ListRetrying --> ListLoading : 再取得
        ListEmpty --> ListDisplaying : メモ作成後
        ListPopulated --> ListDisplaying : データ更新
    }
    
    state MemoListEditState {
        [*] --> EditModeIdle
        EditModeIdle --> DeletionConfirming : 削除ボタン押下
        DeletionConfirming --> DeletionExecuting : 削除確認
        DeletionConfirming --> EditModeIdle : 削除キャンセル
        DeletionExecuting --> DeletionSuccess : 削除成功
        DeletionExecuting --> DeletionError : 削除失敗
        DeletionSuccess --> EditModeIdle : 一覧更新
        DeletionError --> EditModeIdle : エラー表示後
    }
```

## 2. リアルタイム保存状態遷移

```mermaid
stateDiagram-v2
    [*] --> Idle : 初期状態
    
    Idle --> Typing : 文字入力開始
    Typing --> Debouncing : 入力停止
    Typing --> ImmediateSave : フォーカス離脱
    
    Debouncing --> Typing : 3秒以内に入力再開
    Debouncing --> AutoSave : 3秒経過
    
    AutoSave --> Saving : 保存処理開始
    ImmediateSave --> Saving : 即座保存開始
    
    Saving --> SaveSuccess : 保存成功
    Saving --> SaveError : 保存失敗
    
    SaveSuccess --> Idle : 保存完了
    SaveError --> Retrying : リトライ判断
    
    Retrying --> Saving : 自動リトライ
    Retrying --> SaveFailed : リトライ上限
    
    SaveFailed --> Idle : ユーザー操作待ち
    
    state Debouncing {
        [*] --> Waiting
        Waiting --> [*] : タイマーリセット
        Waiting --> TimerExpired : 3秒経過
        TimerExpired --> [*]
    }
    
    state Saving {
        [*] --> Validating : データ検証
        Validating --> Generating : バリデーション成功
        Validating --> ValidationError : バリデーション失敗
        Generating --> Persisting : ID・タイトル生成
        Persisting --> [*] : IndexedDB保存
        ValidationError --> [*]
    }
```

## 3. データ状態遷移

```mermaid
stateDiagram-v2
    [*] --> NoData : データなし状態
    
    NoData --> Creating : 新規メモ作成
    Creating --> Draft : 下書き状態
    Draft --> Saving : 保存処理
    Saving --> Persisted : 永続化完了
    
    Persisted --> Loading : データ読み込み
    Loading --> Loaded : 読み込み完了
    Loading --> LoadError : 読み込み失敗
    
    Loaded --> Editing : 編集開始
    Editing --> Modified : 内容変更
    Modified --> Saving : 保存処理
    
    Persisted --> Deleting : 削除処理
    Deleting --> Deleted : 削除完了
    Deleting --> DeleteError : 削除失敗
    
    Deleted --> [*] : データ削除
    LoadError --> Loading : リトライ
    DeleteError --> Persisted : 削除失敗時復帰
    
    state Draft {
        [*] --> Empty : 空状態
        Empty --> HasContent : 内容入力
        HasContent --> Empty : 内容削除
        HasContent --> HasTitle : タイトル生成
        HasTitle --> HasContent : タイトル更新
    }
    
    state Modified {
        [*] --> ContentChanged : 本文変更
        [*] --> TitleChanged : タイトル変更
        ContentChanged --> TitleChanged : タイトル再生成
        TitleChanged --> ContentChanged : 本文更新
        ContentChanged --> [*]
        TitleChanged --> [*]
    }
```

## 4. UI状態遷移

```mermaid
stateDiagram-v2
    [*] --> UIInitializing : UI初期化
    
    UIInitializing --> UIReady : 初期化完了
    UIReady --> EditScreenActive : 編集画面表示
    UIReady --> ListScreenActive : 一覧画面表示
    UIReady --> EditListScreenActive : 編集一覧画面表示
    
    EditScreenActive --> ListScreenActive : 一覧ボタン
    ListScreenActive --> EditScreenActive : 編集ボタン/カードタップ
    ListScreenActive --> EditListScreenActive : 編集モードボタン
    EditListScreenActive --> ListScreenActive : 戻るボタン
    
    state EditScreenActive {
        [*] --> TextAreaFocused
        TextAreaFocused --> TextAreaBlurred : フォーカス離脱
        TextAreaBlurred --> TextAreaFocused : フォーカス取得
        
        [*] --> SavingIndicatorHidden
        SavingIndicatorHidden --> SavingIndicatorVisible : 保存開始
        SavingIndicatorVisible --> SavingIndicatorHidden : 保存完了
        
        [*] --> NavigationEnabled
        NavigationEnabled --> NavigationDisabled : 保存中
        NavigationDisabled --> NavigationEnabled : 保存完了
    }
    
    state ListScreenActive {
        [*] --> ListLoading
        ListLoading --> ListDisplayed : データ表示
        ListDisplayed --> ListRefreshing : データ更新
        ListRefreshing --> ListDisplayed : 更新完了
        
        [*] --> CardsInteractive
        CardsInteractive --> CardsNonInteractive : 読み込み中
        CardsNonInteractive --> CardsInteractive : 読み込み完了
    }
    
    state EditListScreenActive {
        [*] --> DeleteButtonsVisible
        DeleteButtonsVisible --> ConfirmDialogVisible : 削除ボタン押下
        ConfirmDialogVisible --> DeleteButtonsVisible : ダイアログ閉じる
        ConfirmDialogVisible --> DeletingInProgress : 削除実行
        DeletingInProgress --> DeleteButtonsVisible : 削除完了
    }
```

## 5. エラー状態遷移

```mermaid
stateDiagram-v2
    [*] --> NoError : 正常状態
    
    NoError --> ValidationError : バリデーションエラー
    NoError --> SaveError : 保存エラー  
    NoError --> LoadError : 読み込みエラー
    NoError --> DeleteError : 削除エラー
    NoError --> NetworkError : ネットワークエラー
    NoError --> StorageError : ストレージエラー
    
    ValidationError --> ErrorDisplayed : エラーメッセージ表示
    SaveError --> ErrorDisplayed : エラーメッセージ表示
    LoadError --> ErrorDisplayed : エラーメッセージ表示
    DeleteError --> ErrorDisplayed : エラーメッセージ表示
    NetworkError --> OfflineMode : オフラインモード
    StorageError --> FallbackMode : フォールバックモード
    
    ErrorDisplayed --> RetryAvailable : リトライ可能
    ErrorDisplayed --> NoError : エラー解決
    
    RetryAvailable --> Retrying : リトライ実行
    Retrying --> NoError : リトライ成功
    Retrying --> ErrorDisplayed : リトライ失敗
    
    OfflineMode --> NoError : 接続復旧
    FallbackMode --> NoError : ストレージ復旧
    
    state ErrorDisplayed {
        [*] --> MessageVisible
        MessageVisible --> MessageHidden : タイムアウト/ユーザー操作
        MessageHidden --> MessageVisible : 新しいエラー
    }
    
    state OfflineMode {
        [*] --> QueueingActions : アクション待機
        QueueingActions --> SyncPending : 同期待ち
        SyncPending --> QueueingActions : 同期失敗
    }
```

## 状態管理実装の考慮事項

### 1. 状態の持続性
- **一時的状態**: UI表示状態、入力中フラグ等
- **永続的状態**: メモデータ、ユーザー設定等
- **セッション状態**: 編集中メモID、画面履歴等

### 2. 状態遷移のトリガー
- **ユーザーアクション**: ボタンクリック、テキスト入力、フォーカス変更
- **システムイベント**: タイマー満了、保存完了、エラー発生
- **外部イベント**: ネットワーク状態変化、ストレージ容量変化

### 3. 並行状態の管理
- **リアルタイム保存**: バックグラウンドで実行
- **UI操作**: ユーザーインタラクションを並行処理
- **エラーハンドリング**: 他の処理と独立して実行

### 4. 状態の一貫性
- **データ整合性**: メモリ内データとIndexedDBの同期
- **UI整合性**: 表示データと実際のデータの同期
- **状態復旧**: アプリ再起動時の状態復元

### 5. パフォーマンス考慮
- **状態更新の最適化**: 不要な再描画を避ける
- **メモリ管理**: 大量メモ時の効率的な状態管理
- **非同期処理**: UI応答性を維持した状態遷移

### 6. デバッグ・テスト
- **状態ログ**: 状態遷移の追跡とデバッグ
- **状態モック**: テスト用の状態シミュレーション
- **状態検証**: 期待する状態遷移の自動テスト