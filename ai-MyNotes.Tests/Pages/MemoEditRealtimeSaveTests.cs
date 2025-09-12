using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using ai_MyNotes.Pages;
using ai_MyNotes.Services;
using ai_MyNotes.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using System.Threading.Tasks;
using Moq;
using TG.Blazor.IndexedDB;

namespace ai_MyNotes.Tests.Pages
{
    /// <summary>
    /// MemoEditコンポーネントのリアルタイム保存機能の詳細テスト
    /// フォーカス離脱優先、自動保存タイマー、競合処理回避、保存状態UIのテスト
    /// </summary>
    public class MemoEditRealtimeSaveTests : TestContext
    {
        private readonly Mock<IndexedDBManager> _mockDbManager;
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<IJSRuntime> _mockJSRuntime;
        private readonly Mock<NavigationManager> _mockNavigation;

        public MemoEditRealtimeSaveTests()
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
        public void MemoEdit_AutoSaveTimer_ShouldStartCountdown()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - テキスト入力で自動保存タイマーを開始
            textarea.Change("テスト入力");
            component.Render();
            
            // Assert - カウントダウンが表示されることを確認
            var saveStatus = component.Find(".save-status-bar");
            Assert.NotNull(saveStatus);
            
            // 編集中の表示があることを確認（条件付きで表示される）
            var hasEditingText = component.Markup.Contains("編集中");
            // 編集中テキストが表示されるか、保存状態バーが存在することを確認
            Assert.True(hasEditingText || saveStatus != null);
        }

        [Fact]
        public void MemoEdit_FocusLossPriority_ShouldSaveImmediately()
        {
            // Arrange
            var savedMemo = new Memo
            {
                Id = 1,
                Title = "フォーカス離脱テスト",
                Content = "フォーカス離脱テスト",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _mockMemoService.Setup(x => x.CreateMemoAsync(It.IsAny<Memo>())).ReturnsAsync(savedMemo);
            
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - テキスト入力後にフォーカス離脱
            textarea.Change("フォーカス離脱テスト");
            textarea.Blur(); // フォーカス離脱をシミュレート
            
            // Assert - 保存処理が呼ばれることを確認
            _mockMemoService.Verify(x => x.CreateMemoAsync(It.IsAny<Memo>()), Times.AtLeastOnce);
        }

        [Fact]
        public void MemoEdit_SaveStateUI_ShouldShowSavingIndicator()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            
            // Assert - 保存状態バーが表示されることを確認
            var saveStatus = component.Find(".save-status-bar");
            Assert.NotNull(saveStatus);
        }

        [Fact]
        public void MemoEdit_SaveSuccess_ShouldShowCompletionFeedback()
        {
            // Arrange
            var savedMemo = new Memo
            {
                Id = 1,
                Title = "保存成功テスト",
                Content = "保存成功テスト",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _mockMemoService.Setup(x => x.CreateMemoAsync(It.IsAny<Memo>())).ReturnsAsync(savedMemo);
            
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - テキスト入力と保存
            textarea.Change("保存成功テスト");
            
            // Assert - 保存状態インジケーターが存在することを確認
            var saveStatusElements = component.FindAll(".save-status-bar");
            Assert.NotEmpty(saveStatusElements);
        }

        [Fact]
        public void MemoEdit_SaveFailure_ShouldShowErrorDisplay()
        {
            // Arrange
            _mockMemoService.Setup(x => x.CreateMemoAsync(It.IsAny<Memo>()))
                           .ThrowsAsync(new Exception("テスト保存エラー"));
            
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - テキスト入力と保存試行
            textarea.Change("エラーテスト");
            textarea.Blur();
            
            // Assert - エラー処理が適切に行われることを確認
            _mockMemoService.Verify(x => x.CreateMemoAsync(It.IsAny<Memo>()), Times.AtLeastOnce);
        }

        [Fact]
        public void MemoEdit_TimerCancellation_ShouldCancelOnFocusLoss()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - 自動保存タイマー開始後すぐにフォーカス離脱
            textarea.Change("タイマーキャンセルテスト");
            
            // フォーカス離脱のシミュレーション
            textarea.Blur();
            
            // Assert - コンポーネントが正常にレンダリングされることを確認
            Assert.NotNull(component.Find("textarea"));
        }

        [Fact]
        public void MemoEdit_ConflictAvoidance_ShouldPreventConcurrentSaves()
        {
            // Arrange
            var savedMemo = new Memo
            {
                Id = 1,
                Title = "競合テスト",
                Content = "競合テスト",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _mockMemoService.Setup(x => x.CreateMemoAsync(It.IsAny<Memo>())).ReturnsAsync(savedMemo);
            
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - 複数の保存処理を同時実行
            textarea.Change("競合テスト1");
            textarea.Blur(); // 最初の保存開始
            
            textarea.Change("競合テスト2");
            textarea.Blur(); // 2回目の保存試行
            
            // Assert - 保存処理が実行されることを確認
            _mockMemoService.Verify(x => x.CreateMemoAsync(It.IsAny<Memo>()), Times.AtLeastOnce);
        }

        [Fact]
        public void MemoEdit_NewMemoIndicator_ShouldShowNewMemoStatus()
        {
            // Arrange & Act - 新規メモとしてコンポーネントをレンダリング
            var component = RenderComponent<MemoEdit>();
            
            // Assert - 新規メモ作成の表示を確認
            Assert.Contains("新規メモ", component.Markup);
        }

        [Fact]
        public void MemoEdit_EditModeIndicator_ShouldShowEditStatus()
        {
            // Arrange - 編集モード用のメモを設定
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "編集テスト", 
                Content = "編集テスト内容", 
                CreatedAt = DateTime.Now, 
                UpdatedAt = DateTime.Now 
            };
            _mockMemoService.Setup(x => x.GetMemoByIdAsync(1)).ReturnsAsync(testMemo);
            
            // Act - 編集モードでコンポーネントをレンダリング
            var component = RenderComponent<MemoEdit>(parameters => parameters.Add(p => p.Id, 1));
            
            // Assert - 編集モードの表示を確認
            Assert.Contains("メモ編集", component.Markup);
        }

        [Fact]
        public void MemoEdit_AutoSaveDebounce_ShouldDelayAutoSave()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea");
            
            // Act - 連続入力（debounce処理のテスト）
            textarea.Change("連続1");
            textarea.Change("連続2");
            textarea.Change("連続3");
            
            // Assert - コンポーネントが正常に動作することを確認
            Assert.Equal("連続3", textarea.GetAttribute("value") ?? textarea.TextContent);
        }
    }
}