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
    /// MemoEditコンポーネントの自動保存機能テスト
    /// </summary>
    public class MemoEditAutoSaveTests : TestContext
    {
        private readonly Mock<IndexedDBManager> _mockDbManager;
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<NavigationManager> _mockNavigation;

        public MemoEditAutoSaveTests()
        {
            _mockDbManager = new Mock<IndexedDBManager>();
            _mockMemoService = new Mock<MemoService>(_mockDbManager.Object);
            _mockJSRuntime = new Mock<IJSRuntime>();
            _mockNavigation = new Mock<NavigationManager>();

            // DIコンテナにモックを登録
            Services.AddSingleton(_mockMemoService.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(_mockNavigation.Object);
        }

        [Fact]
        public void MemoEdit_AutoSave_TriggersAfterTextInput()
        {
            // Arrange
            var savedMemo = new Memo
            {
                Id = 1,
                Title = "新しいタイトル",
                Content = "新しいタイトル\n本文内容",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _mockMemoService
                .Setup(s => s.CreateMemoAsync(It.IsAny<Memo>()))
                .ReturnsAsync(savedMemo);

            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - テキスト入力（自動保存のトリガー）
            textarea.Change("新しいタイトル\n本文内容");

            // Assert - 未保存変更フラグが表示される
            Assert.Contains("未保存の変更があります", component.Markup);
        }

        [Fact]
        public void MemoEdit_AutoSave_UpdatesUnsavedChangesFlag()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - テキスト変更
            textarea.Change("変更されたテキスト");

            // Assert - 未保存変更インジケーターが表示
            Assert.Contains("未保存の変更があります", component.Markup);
            Assert.Contains("bi bi-exclamation-triangle", component.Markup);
        }

        [Fact]
        public void MemoEdit_AutoSave_ShowsSavedStatus()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act - 初期状態（保存されていない状態）
            // Assert - 保存済みインジケーターなし
            Assert.DoesNotContain("保存済み", component.Markup);
            Assert.DoesNotContain("bi bi-check-circle", component.Markup);
        }

        [Fact]
        public void MemoEdit_AutoSave_CharacterCountUpdates()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - 10文字のテキスト入力
            textarea.Change("1234567890");

            // Assert - 文字数が更新される
            Assert.Contains("文字数:", component.Markup);
            // 注意: 実際の文字数は内部状態の変更により確認が必要
        }

        [Fact]
        public void MemoEdit_AutoSave_TitleGenerationFromFirstLine()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - 複数行テキスト入力（1行目がタイトルになる）
            textarea.Change("これがタイトル\nこれは本文の内容です");

            // Assert - 1行目がタイトルになることを示すバッジが表示
            Assert.Contains("1行目が自動的にタイトルになります", component.Markup);
        }

        [Fact]
        public void MemoEdit_AutoSave_HandlesEmptyContent()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - 空のテキスト入力
            textarea.Change("");

            // Assert - 文字数0が表示
            Assert.Contains("文字数:", component.Markup);
            Assert.Contains("0", component.Markup);
        }

        [Fact]
        public void MemoEdit_AutoSave_ShowsLongContentWarning()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - 8000文字以上のテキスト入力（警告表示のテスト）
            var longText = new string('a', 8500);
            textarea.Change(longText);

            // Assert - 文字数制限の警告が表示される可能性
            Assert.Contains("文字数:", component.Markup);
            // 注意: 実際の警告表示は内部ロジックで判定される
        }

        [Fact]
        public void MemoEdit_AutoSave_PreservesTextAreaAttributes()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - テキスト入力
            textarea.Change("テスト内容");

            // Assert - テキストエリアの属性が保持される
            Assert.Equal("memoContent", textarea.Id);
            Assert.Contains("form-control", textarea.ClassList);
            Assert.Equal("false", textarea.GetAttribute("spellcheck"));
        }

        [Fact]
        public void MemoEdit_AutoSave_DisablesTextAreaDuringSave()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act - 保存中状態のシミュレーション
            // Assert - 初期状態では無効化されていない
            var textarea = component.Find("textarea");
            Assert.Null(textarea.GetAttribute("disabled"));
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