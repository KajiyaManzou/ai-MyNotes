using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ai_MyNotes.Pages;
using ai_MyNotes.Models;
using ai_MyNotes.Services;
using Xunit;
using Moq;

namespace ai_MyNotes.Tests.Components.Pages
{
    public class MemoListTests : TestContext
    {
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<IJSRuntime> _mockJSRuntime;

        public MemoListTests()
        {
            _mockMemoService = new Mock<MemoService>(Mock.Of<TG.Blazor.IndexedDB.IndexedDBManager>());
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockJSRuntime = new Mock<IJSRuntime>();

            Services.AddSingleton(_mockMemoService.Object);
            Services.AddSingleton(_mockNavigationManager.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
        }

        [Fact]
        public void MemoList_EmptyList_ShowsEmptyState()
        {
            // Arrange
            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var emptyStateIcon = component.Find("i.bi-journal-text");
            Assert.NotNull(emptyStateIcon);

            var emptyMessage = component.Find("h4");
            Assert.Equal("メモがありません", emptyMessage.TextContent);

            var createButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("最初のメモを作成"));
            Assert.NotNull(createButton);
        }

        [Fact]
        public void MemoList_WithMemos_ShowsMemoCards()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo 
                { 
                    Id = 1, 
                    Title = "テストメモ1", 
                    Content = "これはテスト用のメモ内容です。",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now.AddHours(-1)
                },
                new Memo 
                { 
                    Id = 2, 
                    Title = "テストメモ2", 
                    Content = "2番目のテストメモです。",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                }
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(testMemos);

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var memoCards = component.FindAll(".memo-card");
            Assert.Equal(2, memoCards.Count);

            var firstCard = memoCards[0];
            var firstTitle = firstCard.QuerySelector("h5.card-title");
            Assert.Equal("テストメモ1", firstTitle?.TextContent);

            var secondCard = memoCards[1];
            var secondTitle = secondCard.QuerySelector("h5.card-title");
            Assert.Equal("テストメモ2", secondTitle?.TextContent);
        }

        [Fact]
        public void MemoList_RendersCorrectPageTitle()
        {
            // Arrange
            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var pageTitle = component.Find("title");
            Assert.Equal("メモ一覧", pageTitle.TextContent);

            var heading = component.Find("h2");
            Assert.Equal("メモ一覧", heading.TextContent);
        }

        [Fact]
        public void MemoList_ShowsNewCreateButton()
        {
            // Arrange
            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var createButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("新規作成"));
            Assert.NotNull(createButton);
            Assert.Contains("btn-primary", createButton.GetAttribute("class"));
            Assert.Contains("bi-plus-circle", createButton.InnerHtml);
        }

        [Fact]
        public void MemoList_MemoCardStructure_HasCorrectElements()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "これはテスト用のメモ内容です。長いテキストをテストするために、もう少し文字数を増やします。",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0),
                UpdatedAt = new DateTime(2025, 1, 2, 11, 0, 0)
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo> { testMemo });

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var memoCard = component.Find(".memo-card");
            Assert.NotNull(memoCard);

            // タイトル確認
            var title = memoCard.QuerySelector("h5.card-title");
            Assert.Equal("テストメモ", title?.TextContent);

            // プレビューテキスト確認
            var preview = memoCard.QuerySelector("p.card-text");
            Assert.NotNull(preview);

            // 作成日時確認
            var createdDate = memoCard.QuerySelectorAll("small.text-muted")
                                    .FirstOrDefault(el => el.TextContent.Contains("作成:"));
            Assert.NotNull(createdDate);
            Assert.Contains("01/01", createdDate.TextContent);

            // 更新日時確認
            var updatedDate = memoCard.QuerySelectorAll("small.text-muted")
                                    .FirstOrDefault(el => el.TextContent.Contains("更新:"));
            Assert.NotNull(updatedDate);
            Assert.Contains("01/02", updatedDate.TextContent);

            // 削除ボタン確認
            var deleteButton = memoCard.QuerySelector("button.btn-outline-danger");
            Assert.NotNull(deleteButton);
            Assert.Contains("bi-trash", deleteButton.InnerHtml);
        }

        [Fact]
        public void MemoList_MemoCardWithoutUpdate_DoesNotShowUpdateDate()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "テスト内容",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0) // 同じ時刻
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo> { testMemo });

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var memoCard = component.Find(".memo-card");
            var updatedDate = memoCard.QuerySelectorAll("small.text-muted")
                                    .FirstOrDefault(el => el.TextContent.Contains("更新:"));
            Assert.Null(updatedDate);
        }

        [Fact]
        public void MemoList_HasCorrectBootstrapStructure()
        {
            // Arrange
            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var container = component.Find(".container-fluid");
            Assert.NotNull(container);

            var row = component.Find(".row");
            Assert.NotNull(row);

            var col = component.Find(".col-12");
            Assert.NotNull(col);

            var headerDiv = component.Find(".d-flex.justify-content-between.align-items-center");
            Assert.NotNull(headerDiv);
        }

        [Fact]
        public void MemoList_LoadingState_ShowsSpinner()
        {
            // Arrange
            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo>());

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var markup = component.Markup;
            Assert.Contains("spinner-border", markup);
            Assert.Contains("読み込み中...", markup);
        }

        [Fact]
        public void MemoList_StatusMessage_AlertStructure()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テスト", Content = "内容" }
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(testMemos);

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var markup = component.Markup;
            Assert.Contains("alert", markup);
            Assert.Contains("alert-dismissible", markup);
        }

        [Fact]
        public void MemoList_ResponsiveGrid_HasCorrectClasses()
        {
            // Arrange
            var testMemos = new List<Memo>
            {
                new Memo { Id = 1, Title = "テスト1", Content = "内容1" },
                new Memo { Id = 2, Title = "テスト2", Content = "内容2" }
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(testMemos);

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var gridColumns = component.FindAll(".col-12.col-md-6.col-lg-4");
            Assert.Equal(2, gridColumns.Count);
        }

        [Fact]
        public void MemoList_MemoCard_HasHoverEffectClasses()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "テスト内容"
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo> { testMemo });

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var memoCard = component.Find(".memo-card");
            Assert.Contains("h-100", memoCard.GetAttribute("class"));
        }

        [Fact]
        public void MemoList_DeleteButton_HasCorrectAttributes()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "テスト内容"
            };

            _mockMemoService.Setup(x => x.GetMemosAsync())
                          .ReturnsAsync(new List<Memo> { testMemo });

            // Act
            var component = RenderComponent<MemoList>();

            // Assert
            var deleteButton = component.Find("button.btn-outline-danger");
            Assert.Contains("position-absolute", deleteButton.GetAttribute("class"));
            Assert.Contains("top-0", deleteButton.GetAttribute("class"));
            Assert.Contains("end-0", deleteButton.GetAttribute("class"));
            Assert.Equal("削除", deleteButton.GetAttribute("title"));
        }
    }
}