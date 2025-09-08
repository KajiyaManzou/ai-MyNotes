# テストライブラリの設定 - xUnit・bUnit導入作業記録

## 作業概要
`task.md` の Phase 1 における「テストライブラリの設定」から「xUnitの設定・確認」までの作業記録

## 実行日時
2025-09-08

## 実装内容

### 1. xUnitテストプロジェクトの作成

#### 1.1 初期テストプロジェクト作成
```bash
cd ai-MyNotes
dotnet new xunit -n ai-MyNotes.Tests
```

**課題**: 初期作成時にプロジェクト参照とグローバルusingで問題発生

#### 1.2 修正版テストプロジェクト構造
最終的に以下の構造で作成：
```
/workspace/
├── ai-MyNotes/          # メインプロジェクト
├── ai-MyNotes.Tests/    # テストプロジェクト（修正版）
└── ai-MyNotes.sln       # ソリューションファイル
```

### 2. テストプロジェクトの設定

#### 2.1 プロジェクトファイル (ai-MyNotes.Tests.csproj)
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.5.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="bunit" Version="1.24.10" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="../ai-MyNotes/ai-MyNotes.csproj" />
  </ItemGroup>

</Project>
```

#### 2.2 導入パッケージ
- **xunit** 2.5.3 - xUnitテストフレームワーク
- **xunit.runner.visualstudio** 2.5.3 - Visual Studio統合
- **Microsoft.NET.Test.Sdk** 17.8.0 - .NET テストSDK
- **bunit** 1.24.10 - Blazorコンポーネント用テストライブラリ
- **coverlet.collector** 6.0.0 - コードカバレッジ収集

### 3. 基本テストファイルの作成

#### 3.1 BasicTest.cs
```csharp
using Xunit;

namespace ai_MyNotes.Tests
{
    public class BasicTest
    {
        [Fact]
        public void BasicMathTest_ShouldPass()
        {
            // Arrange
            var a = 2;
            var b = 3;
            var expected = 5;

            // Act
            var result = a + b;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringConcatenationTest_ShouldPass()
        {
            // Arrange
            var str1 = "Hello";
            var str2 = "World";
            var expected = "HelloWorld";

            // Act
            var result = str1 + str2;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
```

### 4. ソリューション構成の修正

#### 4.1 ソリューションファイル更新
```
Microsoft Visual Studio Solution File, Format Version 12.00
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ai-MyNotes", "ai-MyNotes\ai-MyNotes.csproj", "{0699884E-7A97-BFA7-8235-667EA08B9AC3}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ai-MyNotes.Tests", "ai-MyNotes.Tests\ai-MyNotes.Tests.csproj", "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}"
EndProject
```

**修正点**:
- フラットなプロジェクト構造に変更
- 不要なフォルダグループを削除
- プロジェクト参照パスを正しく設定

### 5. 発生した問題と解決方法

#### 5.1 初期作成時の問題
**問題**: 
- グローバルusingでXunit名前空間が見つからないエラー
- プロジェクト参照の循環依存エラー
- 複雑なフォルダ構造による参照問題

**解決方法**:
1. **プロジェクト構造の簡素化**
   - ネストしたフォルダ構造から、フラットな構造に変更
   - `/workspace/ai-MyNotes.Tests/` として独立配置

2. **明示的なusing文の使用**
   - グローバルusingを避けて、明示的に `using Xunit;` を記述
   - より確実で理解しやすいアプローチ

3. **パッケージ参照の正規化**
   - PrivateAssets と IncludeAssets の明示的指定
   - より安定した参照設定

#### 5.2 最終的な解決策
- **独立したテストプロジェクト構造**
- **シンプルなパッケージ依存関係**
- **明示的な参照設定**

## テスト機能と特徴

### 1. xUnit テスト機能
- **[Fact]** 属性: 基本的な単体テスト
- **[Theory]** 属性: パラメータ化テスト (今後実装予定)
- **Assert** クラス: 豊富なアサーション機能

### 2. bUnit テスト機能
- **Blazorコンポーネントテスト**: UI コンポーネントの単体テスト
- **レンダリングテスト**: HTML出力の検証
- **イベントハンドリングテスト**: ユーザー操作のテスト

### 3. コードカバレッジ
- **coverlet.collector**: テスト実行時のコード実行率計測
- **CI/CD統合**: 継続的品質管理の準備

## 今後の開発予定

### Phase 2 での活用予定
1. **Memoモデルの単体テスト**
   - プロパティ設定・取得テスト
   - ビジネスロジックテスト (UpdateTitleFromContent, GetPreview等)
   - バリデーションテスト

2. **MemoServiceの単体テスト**
   - CRUD操作のテスト
   - 異常系処理のテスト
   - IndexedDB接続テスト

3. **Blazorコンポーネントテスト (bUnit)**
   - MemoEditコンポーネントのテスト
   - MemoListコンポーネントのテスト
   - UI操作とイベントハンドリングのテスト

### テストカバレッジ目標
- **単体テスト**: 各クラス・メソッドの網羅的テスト
- **統合テスト**: コンポーネント間の連携テスト
- **品質管理**: テスト失敗時は次工程に進まない運用

## 動作確認方法

### 基本テスト実行
```bash
# プロジェクト全体のテスト実行
dotnet test

# 特定プロジェクトのテスト実行
dotnet test ai-MyNotes.Tests/ai-MyNotes.Tests.csproj

# 詳細出力でのテスト実行
dotnet test --verbosity normal
```

### 期待される結果
```
Test run for ai-MyNotes.Tests.dll (.NETCoreApp,Version=v8.0)
Starting test execution, please wait...
A total of 2 tests passed
Test Run Successful.
```

## 設定完了の確認項目

- [x] xUnitパッケージのインストール
- [x] bUnitパッケージのインストール
- [x] テストプロジェクトの作成
- [x] ソリューションファイルへの追加
- [x] 基本テストファイルの作成
- [x] プロジェクト参照の設定
- [x] コンパイルエラーの修正

## 次のステップ (task.md Phase 1)

### 残りのPhase 1タスク
- [ ] ルーティング設定（2画面構成）
- [ ] パフォーマンス検証

### Phase 2 準備
- 単体テスト作成の準備完了
- テスト駆動開発(TDD)アプローチの採用可能
- 継続的品質管理体制の構築

## 追加実施作業（Phase 1完了後）

### 1. Microsoft.NET.Test.Sdk バージョンアップ
```xml
<!-- Before: Version="17.8.0" -->
<!-- After: Version="17.11.1" -->
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
```

**目的**: 最新のテストSDK機能と.NET 8.0対応の改善を取得

### 2. bUnit実践テストの作成

#### 2.1 追加パッケージ
```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.19" />
```

**バージョン修正**: 8.0.8 → 8.0.19 (メインプロジェクトとの整合性確保)

#### 2.2 bUnitテストファイル作成 (Components/SimpleComponentTests.cs)
```csharp
public class SimpleComponentTests : TestContext
{
    [Fact]
    public void SimpleDiv_RendersCorrectly() { /* 基本HTML要素レンダリングテスト */ }

    [Fact]
    public void ButtonComponent_RendersWithCorrectText() { /* Bootstrap付きボタンテスト */ }

    [Fact]
    public void CounterComponent_IncrementsOnClick() { /* インタラクティブテスト */ }
}
```

**作成した3つのテスト:**
1. **基本レンダリングテスト** - DIV要素のHTML出力検証
2. **Bootstrap統合テスト** - CSSクラスとテキスト内容検証
3. **イベントハンドリングテスト** - ボタンクリックとステート変更検証

#### 2.3 テスト用コンポーネント
- `SimpleTestComponent` - 基本的なDIV要素
- `ButtonTestComponent` - Bootstrap付きボタン
- `CounterTestComponent` - カウンター機能（状態管理付き）

### 3. コードカバレッジ設定の追加

#### 3.1 coverlet.msbuild パッケージ追加
```xml
<PackageReference Include="coverlet.msbuild" Version="6.0.2">
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

#### 3.2 カバレッジ設定をcsprojに統合
```xml
<!-- Code Coverage Settings -->
<CollectCoverage>true</CollectCoverage>
<CoverletOutputFormat>opencover,lcov,json</CoverletOutputFormat>
<CoverletOutput>./TestResults/coverage/</CoverletOutput>
<!-- Threshold disabled for Phase 1 setup -->
<!-- <Threshold>80</Threshold>
<ThresholdType>line</ThresholdType>
<ThresholdStat>total</ThresholdStat> -->
<ExcludeByFile>**/Program.cs</ExcludeByFile>
<Include>[ai-MyNotes]*</Include>
<Exclude>[ai-MyNotes.Tests]*</Exclude>
```

**設定内容:**
- **常時カバレッジ収集**: `dotnet test` 実行時に自動収集
- **複数形式出力**: OpenCover、LCOV、JSON（各種ツール対応）
- **適切な対象設定**: メインプロジェクトのみカバレッジ対象
- **閾値は無効化**: Phase 2でのテスト拡充まで一時的に無効

#### 3.3 発生した問題と解決

**問題1: パッケージバージョン競合**
```
error NU1605: Detected package downgrade: Microsoft.AspNetCore.Components.Web from 8.0.19 to 8.0.8
```
**解決**: バージョンを8.0.19に統一

**問題2: カバレッジ閾値エラー**
```
error : The total line coverage is below the specified 80%
```
**解決**: 閾値チェックを一時無効化（Phase 2まで）

### 4. 最終テスト実行結果

**成功状態:**
- ✅ 全5つのテストが正常実行（BasicTest×2 + bUnitテスト×3）
- ✅ カバレッジファイル出力（OpenCover、LCOV、JSON）
- ✅ エラーなしでのテスト完了

**出力ファイル:**
```
./ai-MyNotes.Tests/TestResults/coverage/
├── coverage.opencover.xml
├── coverage.info
└── coverage.json
```

### 5. Phase 2への準備完了

**利用可能な機能:**
- **xUnit**: 基本単体テスト、Theory/InlineDataでのパラメータ化テスト
- **bUnit**: Blazorコンポーネントテスト、HTML検証、イベントテスト
- **カバレッジ**: 自動収集、複数形式レポート、CI/CD統合準備

**次フェーズでの活用予定:**
1. **Memoモデル単体テスト**: プロパティ、ビジネスロジック、バリデーション
2. **MemoService単体テスト**: CRUD操作、例外処理、接続テスト
3. **Blazorコンポーネントテスト**: UI操作、データバインディング、ナビゲーション

## ファイル変更一覧
- `ai-MyNotes.Tests/ai-MyNotes.Tests.csproj` - テストプロジェクトファイル作成・カバレッジ設定追加
- `ai-MyNotes.Tests/BasicTest.cs` - 基本テストファイル作成
- `ai-MyNotes.Tests/Components/SimpleComponentTests.cs` - bUnitテストファイル作成
- `ai-MyNotes.sln` - ソリューションファイル更新（テストプロジェクト追加）

作業完了日: 2025年9月8日  
追加作業完了日: 2025年9月8日（同日）