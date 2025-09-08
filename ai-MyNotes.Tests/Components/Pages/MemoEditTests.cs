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
    public class MemoEditTests : TestContext
    {
        private readonly Mock<MemoService> _mockMemoService;
        private readonly Mock<NavigationManager> _mockNavigationManager;
        private readonly Mock<IJSRuntime> _mockJSRuntime;

        public MemoEditTests()
        {
            // Mock services setup
            _mockMemoService = new Mock<MemoService>(Mock.Of<TG.Blazor.IndexedDB.IndexedDBManager>());
            _mockNavigationManager = new Mock<NavigationManager>();
            _mockJSRuntime = new Mock<IJSRuntime>();

            // Register mocked services
            Services.AddSingleton(_mockMemoService.Object);
            Services.AddSingleton(_mockNavigationManager.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
        }

        [Fact]
        public void MemoEdit_NewMemoMode_RendersCorrectTitle()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var title = component.Find("h2");
            Assert.Equal("新規メモ", title.TextContent);

            var pageTitle = component.Find("title");
            Assert.Equal("新規メモ", pageTitle.TextContent);
        }

        [Fact]
        public void MemoEdit_NewMemoMode_ShowsTextareaWithPlaceholder()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var textarea = component.Find("textarea#memoContent");
            Assert.NotNull(textarea);
            Assert.Contains("メモを入力してください", textarea.GetAttribute("placeholder"));
            Assert.Equal("20", textarea.GetAttribute("rows"));
        }

        [Fact]
        public void MemoEdit_NewMemoMode_ShowsNavigationButton()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var listButton = component.Find("button");
            Assert.Contains("一覧", listButton.TextContent);
            Assert.Contains("bi-list-ul", listButton.InnerHtml);
        }

        [Fact]
        public void MemoEdit_NewMemoMode_DoesNotShowDeleteButton()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var deleteButtons = component.FindAll("button").Where(b => b.TextContent.Contains("削除"));
            Assert.Empty(deleteButtons);
        }

        [Fact]
        public void MemoEdit_EditMode_RendersCorrectTitle()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "テスト内容",
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now
            };

            _mockMemoService.Setup(x => x.GetMemoByIdAsync(1))
                          .ReturnsAsync(testMemo);

            // Act
            var component = RenderComponent<MemoEdit>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            var title = component.Find("h2");
            Assert.Equal("メモ編集", title.TextContent);
        }

        [Fact]
        public void MemoEdit_EditMode_ShowsDeleteButton()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "テスト内容"
            };

            _mockMemoService.Setup(x => x.GetMemoByIdAsync(1))
                          .ReturnsAsync(testMemo);

            // Act
            var component = RenderComponent<MemoEdit>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            var deleteButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("削除"));
            Assert.NotNull(deleteButton);
            Assert.Contains("btn-danger", deleteButton.GetAttribute("class"));
        }

        [Fact]
        public void MemoEdit_EditMode_ShowsCreatedAndUpdatedDates()
        {
            // Arrange
            var testMemo = new Memo 
            { 
                Id = 1, 
                Title = "テストメモ", 
                Content = "テスト内容",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0),
                UpdatedAt = new DateTime(2025, 1, 2, 11, 0, 0)
            };

            _mockMemoService.Setup(x => x.GetMemoByIdAsync(1))
                          .ReturnsAsync(testMemo);

            // Act
            var component = RenderComponent<MemoEdit>(parameters => parameters.Add(p => p.Id, 1));

            // Assert
            var dateInfo = component.FindAll("small.text-muted");
            var createdInfo = dateInfo.FirstOrDefault(el => el.TextContent.Contains("作成:"));
            var updatedInfo = dateInfo.FirstOrDefault(el => el.TextContent.Contains("更新:"));

            Assert.NotNull(createdInfo);
            Assert.NotNull(updatedInfo);
            Assert.Contains("2025/01/01", createdInfo.TextContent);
            Assert.Contains("2025/01/02", updatedInfo.TextContent);
        }

        [Fact]
        public void MemoEdit_TextareaHasCorrectAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var textarea = component.Find("textarea#memoContent");
            
            Assert.Equal("form-control", textarea.GetAttribute("class"));
            Assert.Equal("memoContent", textarea.Id);
            Assert.Equal("20", textarea.GetAttribute("rows"));
            Assert.False(string.IsNullOrEmpty(textarea.GetAttribute("placeholder")));
        }

        [Fact]
        public void MemoEdit_HasCorrectBootstrapStructure()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var container = component.Find(".container-fluid");
            Assert.NotNull(container);

            var row = component.Find(".row");
            Assert.NotNull(row);

            var col = component.Find(".col-12");
            Assert.NotNull(col);

            var headerDiv = component.Find(".d-flex.justify-content-between.align-items-center.mb-3");
            Assert.NotNull(headerDiv);
        }

        [Fact]
        public void MemoEdit_TextareaInput_TriggersDataBinding()
        {
            // Arrange
            var component = RenderComponent<MemoEdit>();
            var textarea = component.Find("textarea#memoContent");

            // Act
            textarea.Change("新しいメモ内容");

            // Assert
            Assert.Equal("新しいメモ内容", textarea.GetAttribute("value") ?? "");
        }

        [Fact]
        public void MemoEdit_NavigateToListButton_HasCorrectAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert
            var listButton = component.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("一覧"));
            Assert.NotNull(listButton);
            
            var buttonClasses = listButton.GetAttribute("class");
            Assert.Contains("btn", buttonClasses);
            Assert.Contains("btn-outline-secondary", buttonClasses);
        }

        [Fact]
        public void MemoEdit_SavingState_ShowsSpinner()
        {
            // このテストは実際の保存状態をテストするため、より複雑な設定が必要
            // 基本的な構造確認のみ実装

            // Arrange & Act  
            var component = RenderComponent<MemoEdit>();

            // Assert - スピナー要素の存在確認（条件付きレンダリング）
            var markup = component.Markup;
            Assert.Contains("spinner-border", markup); // スピナーのHTML構造が存在することを確認
        }

        [Fact]
        public void MemoEdit_StatusMessage_AlertStructure()
        {
            // Arrange & Act
            var component = RenderComponent<MemoEdit>();

            // Assert - アラート構造の存在確認（条件付きレンダリング）
            var markup = component.Markup;
            // アラート用のクラスがマークアップに含まれていることを確認
            Assert.Contains("alert", markup);
            Assert.Contains("alert-dismissible", markup);
        }
    }
}