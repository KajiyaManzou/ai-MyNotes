# 3.5 エラーハンドリング戦略実装

## 概要

ai-MyNotesアプリケーションにおける包括的なエラーハンドリング戦略を実装しました。IndexedDB操作エラーからグローバルエラーハンドリングまで、ユーザーフレンドリーなエラー処理システムを構築しています。

## 実装内容

### 1. カスタム例外階層の設計と実装

#### 1.1 基底例外クラス (`MyNotesException.cs`)

```csharp
public abstract class MyNotesException : Exception
{
    public string ErrorCode { get; }
    public string UserFriendlyMessage { get; }
    public Dictionary<string, object> ErrorDetails { get; }
    
    protected MyNotesException(
        string errorCode, 
        string userFriendlyMessage, 
        string technicalMessage, 
        Exception? innerException = null,
        Dictionary<string, object>? errorDetails = null)
}
```

**特徴:**
- エラーコード、ユーザー向けメッセージ、技術的メッセージを分離
- エラー詳細情報の構造化保存
- 内部例外の適切なチェーン

#### 1.2 IndexedDB例外 (`IndexedDbException`)

```csharp
public class IndexedDbException : MyNotesException
{
    // 接続失敗
    public static IndexedDbException ConnectionFailure(Exception? innerException = null)
    
    // 初期化失敗
    public static IndexedDbException InitializationFailure(Exception? innerException = null)
    
    // 容量不足
    public static IndexedDbException QuotaExceeded(Exception? innerException = null)
}
```

**実装されたエラーパターン:**
- `INDEXEDDB_CONNECTION_FAILED`: データベース接続エラー
- `INDEXEDDB_INIT_FAILED`: データベース初期化エラー
- `INDEXEDDB_QUOTA_EXCEEDED`: ストレージ容量不足

#### 1.3 データ操作例外 (`DataOperationException`)

```csharp
public class DataOperationException : MyNotesException
{
    public static DataOperationException SaveFailure(string operation, Exception? innerException = null)
    public static DataOperationException LoadFailure(string operation, Exception? innerException = null)
    public static DataOperationException NotFound(string resourceType, string identifier)
    public static DataOperationException ValidationFailure(IEnumerable<string> validationErrors)
}
```

**実装されたエラーパターン:**
- `DATA_SAVE_FAILED`: データ保存失敗
- `DATA_LOAD_FAILED`: データ読み込み失敗
- `DATA_NOT_FOUND`: データが見つからない
- `DATA_VALIDATION_FAILED`: バリデーション失敗

#### 1.4 ネットワーク例外 (`NetworkException`)

```csharp
public class NetworkException : MyNotesException
{
    public static NetworkException ConnectionTimeout(Exception? innerException = null)
    public static NetworkException ConnectionFailure(Exception? innerException = null)
}
```

### 2. エラーハンドリングサービスの実装

#### 2.1 サービスインターフェース (`IErrorHandlingService.cs`)

```csharp
public interface IErrorHandlingService
{
    Task<ErrorMessage> HandleErrorAsync(Exception exception, string context = "");
    Task<ErrorMessage> HandleIndexedDbErrorAsync(Exception exception, string operation);
    Task<ErrorMessage> HandleDataOperationErrorAsync(Exception exception, string operation, string? resourceId = null);
    List<ErrorLogEntry> GetErrorLog(int limit = 100);
    void ClearErrorLog();
    bool IsRecoverable(Exception exception);
}
```

#### 2.2 エラーハンドリングサービス実装 (`ErrorHandlingService.cs`)

**主要機能:**
- **統一エラー処理**: 全例外タイプの一元管理
- **エラーログ記録**: タイムスタンプ、コンテキスト、ユーザーエージェント情報
- **ユーザーメッセージ生成**: エラーコードに基づく適切なメッセージ生成
- **コンソールログ出力**: 開発時のデバッグ支援

```csharp
public async Task<ErrorMessage> HandleErrorAsync(Exception exception, string context = "")
{
    // エラーログに記録
    var logEntry = new ErrorLogEntry
    {
        Timestamp = DateTime.Now,
        Exception = exception,
        Context = context,
        ErrorCode = GetErrorCode(exception),
        UserAgent = await GetUserAgentAsync()
    };
    
    // ユーザー向けメッセージを生成
    return GenerateUserMessage(exception, context);
}
```

**ユーザー向けメッセージの例:**
- **データベース接続エラー**: "データベースに接続できませんでした。ブラウザの設定を確認するか、ページを再読み込みしてください。"
- **ストレージ容量不足**: "ストレージ容量が不足しています。不要なデータを削除するか、ブラウザの設定を確認してください。"
- **保存エラー**: "データの保存に失敗しました。しばらく時間をおいてから再試行してください。"

**推奨アクションの提供:**
```csharp
private List<string> GetSuggestedActions(string errorCode)
{
    return errorCode switch
    {
        "INDEXEDDB_CONNECTION_FAILED" => new List<string>
        {
            "ページを再読み込み",
            "ブラウザの設定を確認",
            "他のタブを閉じる"
        },
        "INDEXEDDB_QUOTA_EXCEEDED" => new List<string>
        {
            "不要なメモを削除",
            "ブラウザのキャッシュをクリア",
            "ストレージ設定を確認"
        },
        // その他のエラーパターン...
    };
}
```

### 3. グローバルエラーハンドラーの実装

#### 3.1 グローバルエラーハンドラー (`GlobalErrorHandler.cs`)

**主要機能:**
- **未処理例外の捕捉**: AppDomain.UnhandledException
- **非同期例外の処理**: TaskScheduler.UnobservedTaskException  
- **JavaScript例外統合**: window.addEventListener経由
- **エラー統計の収集**: エラー頻度、種類別集計
- **イベント駆動通知**: OnUnhandledException, OnCriticalError

```csharp
public class GlobalErrorHandler
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IErrorHandlingService _errorHandlingService;
    private readonly Queue<ErrorRecord> _unhandledErrors;
    
    public event EventHandler<ErrorEventArgs>? OnUnhandledException;
    public event EventHandler<ErrorEventArgs>? OnCriticalError;
    
    public void Initialize()
    {
        // .NET例外ハンドリングの設定
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        
        // JavaScript例外ハンドリングの設定
        _ = Task.Run(async () => await SetupJavaScriptErrorHandlingAsync());
    }
}
```

**JavaScript例外統合:**
```javascript
window.addEventListener('error', function(event) {
    console.error('Global JavaScript error:', event.error);
    if (window.blazorErrorHandler) {
        window.blazorErrorHandler.reportJavaScriptError(
            event.error.toString(), 
            event.filename, 
            event.lineno
        );
    }
});

window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    if (window.blazorErrorHandler) {
        window.blazorErrorHandler.reportJavaScriptError(
            'Promise rejection: ' + event.reason, 
            'unknown', 
            0
        );
    }
});
```

**エラー統計機能:**
```csharp
public ErrorStatistics GetErrorStatistics()
{
    var errors = _unhandledErrors.ToList();
    var now = DateTime.Now;
    
    return new ErrorStatistics
    {
        TotalErrors = errors.Count,
        ErrorsLast24Hours = errors.Where(e => (now - e.Timestamp).TotalHours <= 24).Count(),
        ErrorsLastHour = errors.Where(e => (now - e.Timestamp).TotalMinutes <= 60).Count(),
        CriticalErrors = errors.Count(e => e.IsCritical),
        MostCommonError = GetMostCommonErrorType(errors),
        ErrorsByType = GetErrorsByType(errors)
    };
}
```

### 4. MemoServiceのエラーハンドリング統合

#### 4.1 サービス改修

**インターフェース導入:**
```csharp
public interface IMemoService
{
    Task<bool> InitializeDatabaseAsync();
    Task<List<Memo>> GetMemosAsync();
    Task<Memo?> GetMemoByIdAsync(int id);
    Task<Memo> CreateMemoAsync(Memo memo);
    Task<Memo> UpdateMemoAsync(Memo memo);
    Task<bool> DeleteMemoAsync(int id);
    // その他のメソッド...
}
```

**エラーハンドリング統合:**
```csharp
public class MemoService : IMemoService
{
    private readonly IndexedDBManager _dbManager;
    private readonly IErrorHandlingService _errorHandlingService;
    
    public async Task<Memo> CreateMemoAsync(Memo memo)
    {
        try
        {
            // バリデーション実行
            var (isValid, errors) = memo.Validate();
            if (!isValid)
            {
                var validationException = DataOperationException.ValidationFailure(errors);
                await _errorHandlingService.HandleDataOperationErrorAsync(validationException, "Create memo validation");
                throw validationException;
            }
            
            // データベース操作...
            await _dbManager.AddRecord(record);
            return memo;
        }
        catch (Exception ex) when (!(ex is ArgumentNullException || ex is DataOperationException))
        {
            var saveException = DataOperationException.SaveFailure("Create memo", ex);
            await _errorHandlingService.HandleDataOperationErrorAsync(saveException, "Create memo", memo.Id.ToString());
            throw saveException;
        }
    }
}
```

### 5. 依存性注入とサービス登録

#### 5.1 Program.cs更新

```csharp
// アプリケーションサービスの登録
builder.Services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
builder.Services.AddScoped<IMemoService, MemoService>();
builder.Services.AddScoped<GlobalErrorHandler>();

var app = builder.Build();

// グローバルエラーハンドラーの設定
var globalErrorHandler = app.Services.GetRequiredService<GlobalErrorHandler>();
globalErrorHandler.Initialize();
```

#### 5.2 Razorページ更新

```razor
@page "/"
@page "/edit/{id:int?}"
@using ai_MyNotes.Models
@using ai_MyNotes.Services
@inject IMemoService MemoService
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
```

### 6. テスト対応とインターフェース化

#### 6.1 テストフレンドリーな設計

**Mock対応:**
```csharp
// テストでのサービス登録例
Services.AddSingleton<IMemoService>(Mock.Of<IMemoService>());
Services.AddSingleton<IErrorHandlingService>(Mock.Of<IErrorHandlingService>());
```

**引数検証テスト:**
```csharp
[Fact]
public void Constructor_WithNullDbManager_ShouldThrowArgumentNullException()
{
    // Arrange
    var mockErrorHandlingService = Mock.Of<IErrorHandlingService>();
    
    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => new MemoService(null!, mockErrorHandlingService));
}
```

### 7. エラーメッセージとUIコンポーネント

#### 7.1 エラーメッセージ構造

```csharp
public class ErrorMessage
{
    public string Title { get; set; } = string.Empty;           // エラータイトル
    public string Message { get; set; } = string.Empty;         // メッセージ本文
    public ErrorType Type { get; set; } = ErrorType.Error;      // エラータイプ
    public bool IsRecoverable { get; set; } = true;             // 回復可能性
    public string ErrorCode { get; set; } = string.Empty;       // エラーコード
    public List<string> Actions { get; set; } = new List<string>(); // 推奨アクション
}

public enum ErrorType
{
    Info,       // 情報
    Warning,    // 警告
    Error,      // エラー
    Critical    // クリティカル
}
```

## 技術的考慮事項

### パフォーマンス最適化
- エラーログのキューサイズ制限（最大50件）
- 非同期エラー処理によるUI応答性維持
- JavaScript例外処理の軽量化

### セキュリティ考慮
- エラーメッセージでの機密情報漏洩防止
- ユーザーフレンドリーメッセージと技術情報の分離
- エラーログの適切なローテーション

### 可用性向上
- エラーハンドラー自体のエラー処理
- 回復可能エラーの判定機能
- 段階的なエラー処理（フォールバック）

## 成果と効果

### 実装完了項目
- ✅ IndexedDB操作エラーハンドリング（接続失敗、データ操作失敗、読み込み失敗）
- ✅ グローバルエラーハンドラーの実装
- ✅ ユーザー向けエラーメッセージの設計
- ✅ カスタム例外階層の構築
- ✅ エラーログ記録機能
- ✅ JavaScript例外統合
- ✅ サービスインターフェース化とテスト対応
- ✅ 依存性注入の適切な構成

### ユーザビリティ向上
- エラー発生時の明確な案内メッセージ
- 具体的な解決策の提示
- 回復可能エラーの自動処理
- ユーザーアクションの負荷軽減

### 開発・運用効率向上
- 構造化されたエラー情報の収集
- デバッグ情報のコンソール出力
- エラー統計による問題傾向の把握
- テストフレンドリーなアーキテクチャ

このエラーハンドリング戦略により、ai-MyNotesアプリケーションは堅牢で保守性の高いエラー処理システムを獲得し、ユーザー体験の大幅な向上を実現しました。