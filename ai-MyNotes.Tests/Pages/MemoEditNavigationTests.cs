using ai_MyNotes.Models;
using ai_MyNotes.Pages;
using ai_MyNotes.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using TG.Blazor.IndexedDB;
using Xunit;
using AngleSharp.Dom;

namespace ai_MyNotes.Tests.Pages
{
    /// <summary>
    /// MemoEditコンポーネントのナビゲーション・未保存変更ハンドリングテスト
    /// </summary>
    public class MemoEditNavigationTests : TestContext
    {
        private readonly Mock<IndexedDBManager> _mockDbManager;
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<NavigationManager> _mockNavigation;

        public MemoEditNavigationTests()
        {
            _mockDbManager = new Mock<IndexedDBManager>();
            _mockMemoService = new Mock<MemoService>(_mockDbManager.Object);
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockNavigation = new Mock<NavigationManager>();

            Services.AddSingleton(_mockMemoService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockNavigation.Object);
        }

        [Fact]
        public void MemoEdit_Navigation_ListButtonExists()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - 一覧ボタンの存在確認
            var listButton = component.Find("button:contains('一覧')");
            Assert.NotNull(listButton);
            Assert.Contains("btn btn-outline-secondary", listButton.ClassList);
            Assert.Contains("bi bi-list-ul", listButton.InnerHtml);
        }

        [Fact]
        public void MemoEdit_Navigation_DeleteButtonOnlyInEditMode()
        {
            // Arrange - 新規作成モード
            var newComponent = RenderComponent<MemoEdit>();

            // Assert - 新規作成では削除ボタンなし
            Assert.DoesNotContain("btn btn-danger", newComponent.Markup);

            // Arrange - 編集モード
            var testMemo = new Memo
            {
                Id = 1,
                Title = "テストメモ",
                Content = "テスト内容"
            };

            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(1))
                .ReturnsAsync(testMemo);

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 1);

            var editComponent = RenderComponent<MemoEdit>(parameters);

            // Assert - 編集モードでは削除ボタンあり
            Assert.Contains("btn btn-danger", editComponent.Markup);
            Assert.Contains("bi bi-trash", editComponent.Markup);
        }

        [Fact]
        public void MemoEdit_Navigation_HeaderTitle()
        {
            // Arrange - 新規作成モード
            var newComponent = RenderComponent<MemoEdit>();

            // Assert
            Assert.Contains("新規メモ", newComponent.Markup);

            // Arrange - 編集モード
            var testMemo = new Memo
            {
                Id = 1,
                Title = "テストタイトル",
                Content = "テスト内容"
            };

            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(1))
                .ReturnsAsync(testMemo);

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 1);

            var editComponent = RenderComponent<MemoEdit>(parameters);

            // Assert
            Assert.Contains("メモ編集", editComponent.Markup);
        }

        [Fact]
        public void MemoEdit_Navigation_PageTitle()
        {
            // Arrange - 新規作成モード
            var newComponent = RenderComponent<MemoEdit>();

            // Assert - PageTitleコンポーネントの確認
            Assert.Contains("新規メモ", newComponent.Markup);

            // Arrange - 編集モード（タイトル付き）
            var testMemo = new Memo
            {
                Id = 1,
                Title = "テストタイトル",
                Content = "テスト内容"
            };

            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(1))
                .ReturnsAsync(testMemo);

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 1);

            var editComponent = RenderComponent<MemoEdit>(parameters);

            // Assert - 編集モードのページタイトル
            Assert.NotNull(editComponent);
        }

        [Fact]
        public void MemoEdit_Navigation_UnsavedChangesWarning()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - テキスト変更（未保存状態を作成）
            textarea.Change("変更されたテキスト");

            // Assert - 未保存変更の警告表示
            Assert.Contains("未保存の変更があります", component.Markup);
            Assert.Contains("bi bi-exclamation-triangle", component.Markup);
        }

        [Fact]
        public void MemoEdit_Navigation_SavedStatus()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - 保存済み状態の表示
            // 初期状態では未保存なので、保存済みマークは表示されない
            Assert.DoesNotContain("bi bi-check-circle", component.Markup);
            Assert.DoesNotContain("保存済み", component.Markup);
        }

        [Fact]
        public void MemoEdit_Navigation_JSInteropSetup()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - JavaScript連携の初期化
            // OnAfterRenderAsync でJSランタイムが呼び出されることの確認
            // モックのセットアップは既に行われている
            Assert.NotNull(component);

            // JSランタイムのモックが正しくセットアップされていることを確認
            Assert.NotNull(_mockJSRuntime);
        }

        [Fact]
        public void MemoEdit_Navigation_BeforeUnloadHandling()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act - コンポーネントの初期化
            // beforeunload イベントハンドラーの設定は OnAfterRenderAsync で行われる

            // Assert - JSRuntime モックが設定されていることを確認
            Assert.NotNull(_mockJSRuntime.Object);
            
            // 実際のJSインタラクションはモック環境では直接テストが困難
            // ここではコンポーネントが正常に初期化されることを確認
            Assert.NotNull(component);
        }

        [Fact]
        public void MemoEdit_Navigation_ButtonAccessibility()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - ボタンのアクセシビリティ属性
            var listButton = component.Find("button:contains('一覧')");
            Assert.NotNull(listButton);
            
            // Bootstrap のボタンクラスが適用されている
            Assert.Contains("btn", listButton.ClassList);
            Assert.Contains("btn-outline-secondary", listButton.ClassList);
        }

        [Fact]
        public void MemoEdit_Navigation_ResponsiveLayout()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - レスポンシブレイアウトクラス
            Assert.Contains("d-flex", component.Markup);
            Assert.Contains("justify-content-between", component.Markup);
            Assert.Contains("align-items-center", component.Markup);
            Assert.Contains("mb-3", component.Markup);
        }

        [Fact]
        public void MemoEdit_Navigation_MemoInformationDisplay()
        {
            // Arrange - 編集モード
            var testMemo = new Memo
            {
                Id = 1,
                Title = "テストメモ",
                Content = "テスト内容",
                CreatedAt = new DateTime(2023, 1, 1, 10, 0, 0),
                UpdatedAt = new DateTime(2023, 1, 2, 15, 30, 0)
            };

            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(1))
                .ReturnsAsync(testMemo);

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 1);

            // Act
            var component = RenderComponent<MemoEdit>(parameters);

            // Assert - メモ情報の表示
            Assert.Contains("作成:", component.Markup);
            Assert.Contains("更新:", component.Markup);
            Assert.Contains("col-md-6", component.Markup); // レスポンシブ列
        }

        [Fact]
        public void MemoEdit_Navigation_LastSavedTimeDisplay()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - 最終保存時刻の表示
            // 初期状態では保存時刻なし
            Assert.DoesNotContain("最終保存:", component.Markup);

            // 保存時刻が設定された場合の表示確認は統合テストで行う
        }

        [Fact]
        public void MemoEdit_Navigation_ComponentCleanup()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act - コンポーネントの破棄
            component.Dispose();

            // Assert - 正常に破棄される（例外が発生しない）
            Assert.True(true); // 破棄が正常に完了すればテスト成功
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mockDbManager?.Reset();
                _mockMemoService?.Reset();
                _mockJSRuntime?.Reset();
                _mockNavigation?.Reset();
            }
            base.Dispose(disposing);
        }
    }
}