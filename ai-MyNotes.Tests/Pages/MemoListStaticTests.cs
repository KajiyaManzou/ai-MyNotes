using ai_MyNotes.Pages;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace ai_MyNotes.Tests.Pages
{
    /// <summary>
    /// MemoListコンポーネントの静的分析テスト
    /// 実際のレンダリングを行わずに、型の構造や属性を検証
    /// Bootstrap + 左スワイプ削除対応のテスト
    /// </summary>
    public class MemoListStaticTests
    {
        [Fact]
        public void MemoList_HasCorrectPageRoute()
        {
            // Act - ページルートの属性をテスト
            var component = typeof(MemoList);
            var routeAttributes = component.GetCustomAttributes(typeof(RouteAttribute), false);
            
            // Assert
            Assert.NotEmpty(routeAttributes);
            
            var routes = routeAttributes.Cast<RouteAttribute>().Select(r => r.Template);
            Assert.Contains("/list", routes);
        }

        [Fact]
        public void MemoList_ImplementsIDisposable()
        {
            // Act - IDisposableの実装を確認
            var component = typeof(MemoList);
            
            // Assert
            Assert.True(typeof(IDisposable).IsAssignableFrom(component));
        }

        [Fact] 
        public void MemoListMarkup_ContainsBootstrapGridSystem()
        {
            // Act - テンプレートファイルの内容を直接検証
            var markup = GetMemoListMarkup();
            
            // Assert - Bootstrap Grid System
            Assert.Contains("container-fluid", markup);
            Assert.Contains("row", markup);
            Assert.Contains("col-12", markup);
            Assert.Contains("col-md-6", markup);
            Assert.Contains("col-lg-4", markup);
            Assert.Contains("mb-3", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsBootstrapCardComponents()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Bootstrap Card Components
            Assert.Contains("card", markup);
            Assert.Contains("card-body", markup);
            Assert.Contains("card-title", markup);
            Assert.Contains("card-text", markup);
            Assert.Contains("h-100", markup); // Full height cards
        }

        [Fact]
        public void MemoListMarkup_ContainsBootstrapButtons()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Bootstrap Buttons
            Assert.Contains("btn btn-primary", markup); // New memo button
            Assert.Contains("btn btn-outline-danger", markup); // Delete button
            Assert.Contains("btn btn-link", markup); // Swipe delete button
            Assert.Contains("btn-sm", markup); // Small button variant
        }

        [Fact]
        public void MemoListMarkup_ContainsBootstrapIcons()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Bootstrap Icons
            Assert.Contains("bi bi-plus-circle", markup); // Create icon
            Assert.Contains("bi bi-calendar-event", markup); // Date icon
            Assert.Contains("bi bi-pencil-square", markup); // Edit icon
            Assert.Contains("bi bi-trash", markup); // Delete icon
            Assert.Contains("bi bi-trash-fill", markup); // Swipe delete icon
            Assert.Contains("bi bi-journal-text", markup); // Empty state icon
        }

        [Fact]
        public void MemoListMarkup_ContainsSwipeDeleteStructure()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Left Swipe Delete Structure
            Assert.Contains("memo-card-container", markup);
            Assert.Contains("delete-action", markup);
            Assert.Contains("data-memo-id", markup);
            Assert.Contains("overflow-hidden", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsTouchEventHandlers()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Touch Event Handlers
            Assert.Contains("@ontouchstart", markup);
            Assert.Contains("@ontouchmove", markup);
            Assert.Contains("@ontouchend", markup);
            Assert.Contains("@ontouchcancel", markup);
            Assert.Contains("OnTouchStart", markup);
            Assert.Contains("OnTouchMove", markup);
            Assert.Contains("OnTouchEnd", markup);
            Assert.Contains("OnTouchCancel", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsClickHandlers()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Click Event Handlers
            Assert.Contains("@onclick=\"CreateNewMemo\"", markup);
            Assert.Contains("@onclick=\"() => EditMemo(memo.Id)\"", markup);
            Assert.Contains("@onclick=\"(e) => DeleteMemo(memo, e)\"", markup);
            Assert.Contains("@onclick:stopPropagation=\"true\"", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsConditionalRendering()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Conditional Rendering
            Assert.Contains("@if (!string.IsNullOrEmpty(statusMessage))", markup);
            Assert.Contains("@if (isLoading)", markup);
            Assert.Contains("@if (memos?.Any() == true)", markup);
            Assert.Contains("@if (memo.UpdatedAt != memo.CreatedAt)", markup);
            Assert.Contains("@foreach (var memo in memos)", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsLoadingState()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Loading State
            Assert.Contains("spinner-border", markup);
            Assert.Contains("読み込み中...", markup);
            Assert.Contains("visually-hidden", markup);
            Assert.Contains("d-flex justify-content-center", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsEmptyState()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Empty State
            Assert.Contains("メモがありません", markup);
            Assert.Contains("新規作成ボタンを押してメモを作成しましょう", markup);
            Assert.Contains("最初のメモを作成", markup);
            Assert.Contains("text-center py-5", markup);
            Assert.Contains("display-1 text-muted", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsAlertComponents()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Alert Components
            Assert.Contains("alert", markup);
            Assert.Contains("alert-danger", markup);
            Assert.Contains("alert-info", markup);
            Assert.Contains("alert-dismissible", markup);
            Assert.Contains("fade show", markup);
            Assert.Contains("btn-close", markup);
            Assert.Contains("role=\"alert\"", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsMemoDisplayContent()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Memo Display Content
            Assert.Contains("@memo.Title", markup);
            Assert.Contains("@memo.GetPreview(120)", markup);
            Assert.Contains("@memo.CreatedAt.ToString(\"MM/dd HH:mm\")", markup);
            Assert.Contains("@memo.UpdatedAt.ToString(\"MM/dd HH:mm\")", markup);
            Assert.Contains("text-truncate", markup);
            Assert.Contains("flex-grow-1", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsResponsiveDesign()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Responsive Design Classes
            Assert.Contains("d-flex", markup);
            Assert.Contains("justify-content-between", markup);
            Assert.Contains("align-items-center", markup);
            Assert.Contains("position-relative", markup);
            Assert.Contains("position-absolute", markup);
            Assert.Contains("top-0", markup);
            Assert.Contains("end-0", markup);
            Assert.Contains("h-100", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsAccessibilityAttributes()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Accessibility Attributes
            Assert.Contains("role=\"alert\"", markup);
            Assert.Contains("role=\"status\"", markup);
            Assert.Contains("title=\"削除\"", markup);
            Assert.Contains("visually-hidden", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsCustomCSSClasses()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Custom CSS Classes
            Assert.Contains("memo-card-container", markup);
            Assert.Contains("memo-card", markup);
            Assert.Contains("memo-preview", markup);
            Assert.Contains("memo-delete-btn", markup);
            Assert.Contains("delete-action", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsSwipeAnimationStyles()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Swipe Animation Styles
            Assert.Contains("transform: translateX(100%)", markup);
            Assert.Contains("transition: transform 0.3s ease", markup);
            Assert.Contains("background: #dc3545", markup);
            Assert.Contains("width: 80px", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsMediaQueries()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - CSS Media Queries (escaped in Razor)
            Assert.Contains("@@media (max-width: 768px)", markup);
            Assert.Contains("@@media (min-width: 769px)", markup);
            Assert.Contains("touch-action: pan-y", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsCSSTransitions()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - CSS Transitions and Animations
            Assert.Contains("transition:", markup);
            Assert.Contains("transform:", markup);
            Assert.Contains("box-shadow:", markup);
            Assert.Contains("opacity:", markup);
            Assert.Contains("ease-in-out", markup);
            Assert.Contains("ease-out", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsTouchInteractionClasses()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Touch Interaction Classes
            Assert.Contains("user-select: none", markup);
            Assert.Contains("-webkit-user-select: none", markup);
            Assert.Contains("-webkit-line-clamp:", markup);
            Assert.Contains("-webkit-box-orient: vertical", markup);
        }

        [Fact]
        public void MemoListMarkup_ContainsPageTitle()
        {
            // Act
            var markup = GetMemoListMarkup();
            
            // Assert - Page Title
            Assert.Contains("<PageTitle>メモ一覧</PageTitle>", markup);
        }

        private string GetMemoListMarkup()
        {
            // MemoList.razorファイルの内容を読み取る
            var filePath = "/workspace/ai-MyNotes/Pages/MemoList.razor";
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return "";
        }
    }
}