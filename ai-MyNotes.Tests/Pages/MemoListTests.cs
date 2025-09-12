using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using ai_MyNotes.Pages;
using ai_MyNotes.Services;
using ai_MyNotes.Models;
using AngleSharp.Dom;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;

namespace ai_MyNotes.Tests.Pages
{
    public class MemoListTests : TestContext
    {
        private Mock<MemoService> mockMemoService;
        private Mock<NavigationManager> mockNavigationManager;
        private Mock<IJSRuntime> mockJSRuntime;

        public MemoListTests()
        {
            // モックの初期化
            mockMemoService = new Mock<MemoService>();
            mockNavigationManager = new Mock<NavigationManager>();
            mockJSRuntime = new Mock<IJSRuntime>();

            // サービスを登録
            Services.AddSingleton(mockMemoService.Object);
            Services.AddSingleton(mockNavigationManager.Object);
            Services.AddSingleton(mockJSRuntime.Object);
        }

        [Fact]
        public void MemoList_ShouldRenderCorrectTitle()
        {
            // Arrange
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var title = component.Find("h2");
            Assert.Equal("メモ一覧", title.TextContent);
        }

        [Fact]
        public void MemoList_ShouldShowCreateButton()
        {
            // Arrange
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var createButton = component.Find("button");
            Assert.Contains("新規作成", createButton.TextContent);
        }

        [Fact]
        public void MemoList_ShouldShowLoadingSpinner_WhenLoading()
        {
            // Arrange - サービスが完了しないタスクを返すようにセットアップ
            var tcs = new TaskCompletionSource<List<Memo>>();
            mockMemoService.Setup(x => x.GetMemosAsync()).Returns(tcs.Task);

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var spinner = component.Find(".spinner-border");
            Assert.NotNull(spinner);
            Assert.Contains("読み込み中...", spinner.TextContent);
        }

        [Fact]
        public async Task MemoList_ShouldDisplayMemos_WhenMemosExist()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テストメモ1", Content = "テスト内容1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Memo { Id = 2, Title = "テストメモ2", Content = "テスト内容2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(testMemos);

            // Act
            var component = RenderComponent<MemoList>();
            
            // レンダリング完了を待つ
            await Task.Delay(100);
            component.Render();

            // Assert
            var memoCards = component.FindAll(".memo-card");
            Assert.Equal(2, memoCards.Count);
            
            var titles = component.FindAll(".card-title");
            Assert.Contains(titles, t => t.TextContent.Contains("テストメモ1"));
            Assert.Contains(titles, t => t.TextContent.Contains("テストメモ2"));
        }

        [Fact]
        public void MemoList_ShouldShowEmptyState_WhenNoMemos()
        {
            // Arrange
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var emptyMessage = component.Find("h4");
            Assert.Contains("メモがありません", emptyMessage.TextContent);
            
            var createButton = component.Find(".text-center button");
            Assert.Contains("最初のメモを作成", createButton.TextContent);
        }

        [Fact]
        public void MemoList_CreateNewMemo_ShouldNavigateToRoot()
        {
            // Arrange
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(new List<Memo>());
            mockNavigationManager.Setup(x => x.NavigateTo("/", false));

            // Act
            var component = RenderComponent<MemoList>();
            var createButton = component.Find("button");
            createButton.Click();

            // Assert
            mockNavigationManager.Verify(x => x.NavigateTo("/", false), Times.Once);
        }

        [Fact]
        public async Task MemoList_EditMemo_ShouldNavigateToEditPage()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テストメモ", Content = "テスト内容", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(testMemos);
            mockNavigationManager.Setup(x => x.NavigateTo("/edit/1", false));

            // Act
            var component = RenderComponent<MemoList>();
            await Task.Delay(100);
            component.Render();
            
            var memoCard = component.Find(".memo-card");
            memoCard.Click();

            // Assert
            mockNavigationManager.Verify(x => x.NavigateTo("/edit/1", false), Times.Once);
        }

        [Fact]
        public void MemoList_ShouldShowStatusMessage()
        {
            // Arrange
            mockMemoService.Setup(x => x.GetMemosAsync()).ThrowsAsync(new Exception("テストエラー"));

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var alert = component.Find(".alert");
            Assert.Contains("エラー", alert.TextContent);
        }

        [Fact]
        public void MemoList_ShouldClearStatusMessage_WhenCloseButtonClicked()
        {
            // Arrange
            mockMemoService.Setup(x => x.GetMemosAsync()).ThrowsAsync(new Exception("テストエラー"));

            // Act
            var component = RenderComponent<MemoList>();
            var closeButton = component.Find(".btn-close");
            closeButton.Click();

            // Assert
            Assert.Empty(component.FindAll(".alert"));
        }

        [Fact]
        public async Task MemoList_ShouldShowDeleteConfirmation_WhenDeleteButtonClicked()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テストメモ", Content = "テスト内容", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(testMemos);
            mockJSRuntime.Setup(x => x.InvokeAsync<bool>("confirm", It.IsAny<object?[]?>()))
                        .ReturnsAsync(true);

            // Act
            var component = RenderComponent<MemoList>();
            await Task.Delay(100);
            component.Render();
            
            var deleteButton = component.Find(".memo-delete-btn");
            await deleteButton.ClickAsync(new MouseEventArgs());

            // Assert
            mockJSRuntime.Verify(x => x.InvokeAsync<bool>("confirm", 
                It.Is<object?[]?>(args => args != null && args[0] != null && args[0].ToString()!.Contains("テストメモ"))), 
                Times.Once);
        }

        [Fact]
        public async Task MemoList_ShouldDeleteMemo_WhenConfirmed()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テストメモ", Content = "テスト内容", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(testMemos);
            mockMemoService.Setup(x => x.DeleteMemoAsync(1)).Returns(Task.FromResult(true));
            mockJSRuntime.Setup(x => x.InvokeAsync<bool>("confirm", It.IsAny<object?[]?>()))
                        .ReturnsAsync(true);

            // Act
            var component = RenderComponent<MemoList>();
            await Task.Delay(100);
            component.Render();
            
            var deleteButton = component.Find(".memo-delete-btn");
            await deleteButton.ClickAsync(new MouseEventArgs());

            // Assert
            mockMemoService.Verify(x => x.DeleteMemoAsync(1), Times.Once);
        }

        [Fact]
        public async Task MemoList_ShouldNotDeleteMemo_WhenCancelled()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テストメモ", Content = "テスト内容", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(testMemos);
            mockJSRuntime.Setup(x => x.InvokeAsync<bool>("confirm", It.IsAny<object?[]?>()))
                        .ReturnsAsync(false);

            // Act
            var component = RenderComponent<MemoList>();
            await Task.Delay(100);
            component.Render();
            
            var deleteButton = component.Find(".memo-delete-btn");
            await deleteButton.ClickAsync(new MouseEventArgs());

            // Assert
            mockMemoService.Verify(x => x.DeleteMemoAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task MemoList_ShouldDisplayMemoPreview()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストタイトル", 
                Content = "これは長いテスト内容です。プレビューとして表示されるはずです。", 
                CreatedAt = DateTime.Now, 
                UpdatedAt = DateTime.Now 
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(new List<Memo> { testMemo });

            // Act
            var component = RenderComponent<MemoList>();
            await Task.Delay(100);
            component.Render();

            // Assert
            var preview = component.Find(".memo-preview");
            Assert.Contains("これは長いテスト内容です", preview.TextContent);
        }

        [Fact]
        public async Task MemoList_ShouldShowCreatedAndUpdatedDates()
        {
            // Arrange
            var createdDate = new DateTime(2024, 1, 1, 10, 0, 0);
            var updatedDate = new DateTime(2024, 1, 2, 11, 0, 0);
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストタイトル", 
                Content = "テスト内容", 
                CreatedAt = createdDate, 
                UpdatedAt = updatedDate 
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(new List<Memo> { testMemo });

            // Act
            var component = RenderComponent<MemoList>();
            await Task.Delay(100);
            component.Render();

            // Assert
            var dateElements = component.FindAll("small.text-muted");
            Assert.Contains(dateElements, d => d.TextContent.Contains("作成:"));
            Assert.Contains(dateElements, d => d.TextContent.Contains("更新:"));
        }

        [Fact]
        public void MemoList_ShouldHaveResponsiveGridLayout()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テスト1", Content = "内容1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new Memo { Id = 2, Title = "テスト2", Content = "内容2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };
            mockMemoService.Setup(x => x.GetMemosAsync()).ReturnsAsync(testMemos);

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var gridColumns = component.FindAll(".col-12.col-md-6.col-lg-4");
            Assert.Equal(2, gridColumns.Count);
        }
    }
}