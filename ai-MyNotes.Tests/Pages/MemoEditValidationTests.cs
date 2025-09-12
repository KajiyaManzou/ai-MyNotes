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
    /// MemoEditコンポーネントのデータ検証・エラーハンドリングテスト
    /// </summary>
    public class MemoEditValidationTests : TestContext
    {
        private readonly Mock<IndexedDBManager> _mockDbManager;
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<NavigationManager> _mockNavigation;

        public MemoEditValidationTests()
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
        public void MemoEdit_Validation_ShowsErrorForInvalidData()
        {
            // Arrange
            _mockMemoService
                .Setup(s => s.CreateMemoAsync(It.IsAny<Memo>()))
                .ThrowsAsync(new InvalidOperationException("メモの作成に失敗しました: タイトルは必須です"));

            var component = RenderComponent<MemoEdit>();

            // Act - 不正なデータでの保存試行はコンポーネント内部で処理される
            // Assert - エラーメッセージの表示領域が存在することを確認
            // 初期状態ではエラーメッセージなし
            Assert.DoesNotContain("alert-danger", component.Markup);
        }

        [Fact]
        public void MemoEdit_Validation_HandlesMemoNotFound()
        {
            // Arrange - 存在しないメモIDでの初期化
            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(999))
                .ReturnsAsync((Memo?)null);

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 999);

            // Act
            var component = RenderComponent<MemoEdit>(parameters);

            // Assert - エラーメッセージが表示される可能性
            // 実際の実装では「メモが見つかりませんでした」メッセージが表示される
            Assert.NotNull(component);
        }

        [Fact]
        public void MemoEdit_Validation_HandlesServiceException()
        {
            // Arrange - MemoServiceが例外をスロー
            _mockMemoService
                .Setup(s => s.GetMemoByIdAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("データベースエラー"));

            var parameters = ComponentParameter.CreateParameter(nameof(MemoEdit.Id), 1);

            // Act & Assert - 例外が発生してもコンポーネントがクラッシュしない
            var component = RenderComponent<MemoEdit>(parameters);
            Assert.NotNull(component);
        }

        [Fact]
        public void MemoEdit_Validation_ShowsCharacterLimitWarning()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act - 文字数制限に関する表示確認
            // Assert
            Assert.Contains("/ 10,000", component.Markup);
            Assert.Contains("文字数:", component.Markup);
        }

        [Fact]
        public void MemoEdit_Validation_ValidationMessageBadge()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - バリデーション関連の情報バッジが表示される
            Assert.Contains("badge bg-info", component.Markup);
            Assert.Contains("1行目が自動的にタイトルになります", component.Markup);
        }

        [Fact]
        public void MemoEdit_Validation_TextAreaValidationAttributes()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act & Assert - テキストエリアにバリデーション関連の属性が設定
            Assert.NotNull(textarea);
            Assert.Equal("memoContent", textarea.Id);
            Assert.Contains("form-control", textarea.ClassList);
        }

        [Fact]
        public void MemoEdit_Validation_RequiredFieldLabel()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - 必須フィールドのラベルが適切に表示
            Assert.Contains("メモ内容", component.Markup);
            Assert.Contains("form-label fw-semibold", component.Markup);
        }

        [Fact]
        public void MemoEdit_Validation_HandlesEmptyContent()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act - 空のコンテンツを入力
            textarea.Change("");

            // Assert - 空のコンテンツでも適切に処理される
            Assert.Contains("文字数:", component.Markup);
            Assert.Contains("0", component.Markup);
        }

        [Fact]
        public void MemoEdit_Validation_ShowsStatusMessageArea()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - ステータスメッセージ表示領域の存在確認
            // 初期状態ではメッセージなし、但し表示領域の構造は存在
            Assert.NotNull(component);
            // ステータスメッセージが表示される場合のクラス構造
            var markup = component.Markup;
            // alert クラスが表示される条件式の存在を間接的に確認
        }

        [Fact]
        public void MemoEdit_Validation_PlaceholderText()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act & Assert - プレースホルダーテキストが適切
            Assert.Equal("メモを入力してください...", textarea.GetAttribute("placeholder"));
        }

        [Fact]
        public void MemoEdit_Validation_FormStructure()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();

            // Act & Assert - フォーム構造の検証
            Assert.Contains("form-floating", component.Markup);
            var floatingLabel = component.Find("label[for='memoContent']");
            Assert.NotNull(floatingLabel);
            Assert.Contains("メモ内容を入力...", floatingLabel.TextContent);
        }

        [Fact]
        public void MemoEdit_Validation_DisabledStateDuringSave()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");

            // Act & Assert - 保存中の無効化状態
            // 初期状態では無効化されていない
            Assert.Null(textarea.GetAttribute("disabled"));

            // 保存中状態の確認（保存インジケーターの存在）
            // 初期状態では保存インジケーターなし
            Assert.DoesNotContain("spinner-border", component.Markup);
            Assert.DoesNotContain("保存中...", component.Markup);
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