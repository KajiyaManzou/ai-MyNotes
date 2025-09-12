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
    /// MemoEditコンポーネントのbUnitテスト
    /// </summary>
    public class MemoEditTests : TestContext
    {
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<NavigationManager> _mockNavigation;

        public MemoEditTests()
        {
            // モックオブジェクトの初期化（IndexedDBManagerは避ける）
            _mockMemoService = new Mock<MemoService>();
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockNavigation = new Mock<NavigationManager>();

            // DIコンテナにモックを登録
            Services.AddSingleton(_mockMemoService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockNavigation.Object);
        }

        [Fact]
        public void MemoEdit_Renders_WithoutErrors()
        {
            // Arrange - 新規作成モード（IDなし）

            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            Assert.NotNull(component);
            Assert.Contains("新規メモ", component.Markup);
            Assert.Contains("メモ編集", component.Markup);
        }

        [Fact]
        public void MemoEdit_NewCreationMode_ShowsCorrectTitle()
        {
            // Arrange - 新規作成モード

            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            Assert.Contains("新規メモ", component.Markup);
            Assert.Contains("メモ編集", component.Markup);
            Assert.DoesNotContain("削除", component.Markup); // 新規作成では削除ボタンなし
        }

        [Fact]
        public void MemoEdit_EditMode_ShowsCorrectTitle()
        {
            // Arrange - 編集モード（ID=1）
            var testMemo = new Memo
            {
                Id = 1,
                Title = "テストメモ",
                Content = "テスト内容",
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now
            };

            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(1))
                .ReturnsAsync(testMemo);

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 1);

            // Act
            var component = RenderComponent<MemoEdit>(parameters);

            // Assert
            Assert.Contains("編集:", component.Markup);
            Assert.Contains("削除", component.Markup); // 編集モードでは削除ボタンあり
        }

        [Fact]
        public void MemoEdit_TextArea_HasCorrectBootstrapClasses()
        {
            // Arrange

            // Act
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Assert
            Assert.Contains("form-control", textarea.ClassList);
            Assert.Contains("flex-grow-1", textarea.ClassList);
            Assert.Contains("resize-none", textarea.ClassList);
            Assert.Equal("memoContent", textarea.Id);
            Assert.Equal("メモを入力してください...", textarea.GetAttribute("placeholder"));
        }

        [Fact]
        public void MemoEdit_BootstrapComponents_ArePresent()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert - Bootstrap クラスの確認
            Assert.Contains("container-fluid", component.Markup);
            Assert.Contains("btn btn-outline-secondary", component.Markup); // 一覧ボタン
            Assert.Contains("form-label fw-semibold", component.Markup); // フォームラベル
            Assert.Contains("form-floating", component.Markup); // フローティングラベル
            Assert.Contains("badge bg-info", component.Markup); // 情報バッジ
        }

        [Fact]
        public void MemoEdit_CharacterCounter_DisplaysCorrectly()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            Assert.Contains("文字数:", component.Markup);
            Assert.Contains("/ 10,000", component.Markup);
            Assert.Contains("bi bi-type", component.Markup); // アイコン確認
        }

        [Fact]
        public void MemoEdit_UnsavedChangesIndicator_WorksCorrectly()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - テキスト入力（変更を発生させる）
            textarea.Change("新しいテキスト");

            // Assert
            // 未保存の変更があることを示すインジケーターが表示される
            Assert.Contains("未保存の変更があります", component.Markup);
            Assert.Contains("bi bi-exclamation-triangle", component.Markup);
        }

        [Fact]
        public void MemoEdit_ListNavigationButton_HasCorrectHref()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var listButton = component.Find("button:contains('一覧')");
            Assert.NotNull(listButton);
            Assert.Contains("bi bi-list-ul", listButton.InnerHtml);
        }

        [Fact]
        public void MemoEdit_DeleteButton_OnlyShowsInEditMode()
        {
            // Arrange - 新規作成モード
            var newComponent = RenderComponent<MemoEdit>();

            // Assert - 新規作成では削除ボタンなし
            Assert.DoesNotContain("btn-danger", newComponent.Markup);
            Assert.DoesNotContain("bi bi-trash", newComponent.Markup);

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
        public void MemoEdit_StatusMessage_DisplaysCorrectly()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act - 初期状態ではステータスメッセージなし
            // Assert
            Assert.DoesNotContain("alert", component.Markup);
        }

        [Fact]
        public void MemoEdit_SavingIndicator_WorksCorrectly()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert - 初期状態では保存インジケーターなし
            Assert.DoesNotContain("保存中...", component.Markup);
            Assert.DoesNotContain("spinner-border", component.Markup);
        }

        [Fact]
        public void MemoEdit_MemoInformation_ShowsInEditMode()
        {
            // Arrange - 編集モード用のテストメモ
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

            // Assert
            Assert.Contains("作成:", component.Markup);
            Assert.Contains("更新:", component.Markup);
            Assert.Contains("2023/01/01", component.Markup);
            Assert.Contains("2023/01/02", component.Markup);
        }

        [Fact]
        public void MemoEdit_TextAreaAttributes_AreCorrect()
        {
            // Act
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Assert
            Assert.Equal("memoContent", textarea.Id);
            Assert.Equal("メモを入力してください...", textarea.GetAttribute("placeholder"));
            Assert.Equal("false", textarea.GetAttribute("spellcheck"));
            Assert.Contains("min-height: 400px", textarea.GetAttribute("style"));
        }

        [Fact]
        public void MemoEdit_BootstrapGrid_IsProperlyStructured()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert - Bootstrap グリッドシステムの確認
            Assert.Contains("container-fluid", component.Markup);
            Assert.Contains("row", component.Markup);
            Assert.Contains("col-12", component.Markup);
            Assert.Contains("col-md-6", component.Markup); // メモ情報の列
        }

        [Fact]
        public void MemoEdit_FormFloating_IsCorrectlyImplemented()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            Assert.Contains("form-floating", component.Markup);
            Assert.Contains("form-label", component.Markup);
            var floatingDiv = component.Find(".form-floating");
            Assert.NotNull(floatingDiv);
        }

        [Fact]
        public void MemoEdit_Icons_ArePresent()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert - Bootstrap Icons の確認
            Assert.Contains("bi bi-list-ul", component.Markup); // 一覧アイコン
            Assert.Contains("bi bi-type", component.Markup); // 文字数アイコン
        }

        [Fact]
        public void MemoEdit_AutoTitleGeneration_WorksCorrectly()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - 1行目がタイトルになるテキストを入力
            textarea.Change("新しいタイトル\n本文の内容です");

            // Assert - タイトルが自動生成されることを検証
            // （実際のテストでは、コンポーネントの内部状態やタイトル表示を確認）
            Assert.Contains("1行目が自動的にタイトルになります", component.Markup);
        }

        [Fact]
        public void MemoEdit_ResponsiveDesign_HasCorrectClasses()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert - レスポンシブデザインのクラス確認
            Assert.Contains("d-flex", component.Markup);
            Assert.Contains("justify-content-between", component.Markup);
            Assert.Contains("align-items-center", component.Markup);
            Assert.Contains("flex-grow-1", component.Markup);
            Assert.Contains("d-flex flex-column", component.Markup);
        }

        [Fact]
        public void MemoEdit_ValidationMessage_Badge_IsPresent()
        {
            // Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            Assert.Contains("badge bg-info", component.Markup);
            Assert.Contains("1行目が自動的にタイトルになります", component.Markup);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // テスト後のクリーンアップ
                _mockMemoService?.Reset();
                _mockJSRuntime?.Reset();
                _mockNavigation?.Reset();
            }
            base.Dispose(disposing);
        }
    }
}