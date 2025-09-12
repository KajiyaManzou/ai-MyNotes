using ai_MyNotes.Pages;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace ai_MyNotes.Tests.Pages
{
    /// <summary>
    /// MemoEditコンポーネントの静的分析テスト
    /// 実際のレンダリングを行わずに、型の構造や属性を検証
    /// </summary>
    public class MemoEditStaticTests
    {
        [Fact]
        public void MemoEdit_HasCorrectPageRoutes()
        {
            // Act - ページルートの属性をテスト
            var component = typeof(MemoEdit);
            var routeAttributes = component.GetCustomAttributes(typeof(RouteAttribute), false);
            
            // Assert
            Assert.NotEmpty(routeAttributes);
            
            var routes = routeAttributes.Cast<RouteAttribute>().Select(r => r.Template);
            Assert.Contains("/", routes);
            Assert.Contains("/edit/{id:int?}", routes);
        }

        [Fact]
        public void MemoEdit_ImplementsIDisposable()
        {
            // Act - IDisposableの実装を確認
            var component = typeof(MemoEdit);
            
            // Assert
            Assert.True(typeof(IDisposable).IsAssignableFrom(component));
        }

        [Fact]
        public void MemoEdit_HasIdParameter()
        {
            // Act - Idパラメーターの存在を確認
            var component = typeof(MemoEdit);
            var property = component.GetProperty("Id");
            
            // Assert
            Assert.NotNull(property);
            Assert.Equal(typeof(int?), property.PropertyType);
            
            // ParameterAttributeが設定されているかを確認
            var parameterAttribute = property.GetCustomAttributes(typeof(ParameterAttribute), false);
            Assert.NotEmpty(parameterAttribute);
        }

        [Fact] 
        public void MemoEditMarkup_ContainsBootstrapClasses()
        {
            // Act - テンプレートファイルの内容を直接検証
            var markup = GetMemoEditMarkup();
            
            // Assert
            Assert.Contains("container-fluid", markup);
            Assert.Contains("btn btn-outline-secondary", markup);
            Assert.Contains("form-control", markup);
            Assert.Contains("form-floating", markup);
            Assert.Contains("badge bg-info", markup);
        }

        [Fact]
        public void MemoEditMarkup_ContainsRequiredElements()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert
            Assert.Contains("textarea", markup);
            Assert.Contains("memoContent", markup);
            Assert.Contains("一覧", markup);
            Assert.Contains("削除", markup);
            Assert.Contains("メモ内容", markup);
        }

        [Fact]
        public void MemoEditMarkup_ContainsBootstrapIcons()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert
            Assert.Contains("bi bi-list-ul", markup);
            Assert.Contains("bi bi-trash", markup);
            Assert.Contains("bi bi-check-circle", markup);
            Assert.Contains("bi bi-exclamation-triangle", markup);
            Assert.Contains("bi bi-type", markup);
        }

        [Fact]
        public void MemoEditMarkup_ContainsConditionalRendering()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert - 条件付きレンダリングのRazor構文を確認
            Assert.Contains("@if (IsEditMode)", markup);
            Assert.Contains("@if (!string.IsNullOrEmpty(statusMessage))", markup);
            Assert.Contains("@if (isSaving)", markup);
            Assert.Contains("@if (hasUnsavedChanges)", markup);
        }

        [Fact]
        public void MemoEditMarkup_HasCorrectFormStructure()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert
            Assert.Contains("form-floating", markup);
            Assert.Contains("form-label fw-semibold", markup);
            Assert.Contains("placeholder=\"メモを入力してください...\"", markup);
            Assert.Contains("spellcheck=\"false\"", markup);
        }

        [Fact]
        public void MemoEditMarkup_HasCharacterCounter()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert
            Assert.Contains("文字数:", markup);
            Assert.Contains("/ 10,000", markup);
            Assert.Contains("CurrentMemo.Content?.Length", markup);
        }

        [Fact]
        public void MemoEditMarkup_HasEventHandlers()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert - イベントハンドラーの存在確認
            Assert.Contains("@oninput=\"OnContentInput\"", markup);
            Assert.Contains("@onfocusout=\"OnFocusOut\"", markup);
            Assert.Contains("@onclick=\"NavigateToList\"", markup);
            Assert.Contains("@onclick=\"DeleteMemo\"", markup);
            Assert.Contains("@onclick=\"ClearStatus\"", markup);
        }

        [Fact]
        public void MemoEditMarkup_HasCorrectBinding()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert - データバインディングの確認
            Assert.Contains("@bind=\"CurrentMemo.Content\"", markup);
        }

        [Fact]
        public void MemoEditMarkup_HasAccessibilityAttributes()
        {
            // Act
            var markup = GetMemoEditMarkup();
            
            // Assert - アクセシビリティ属性
            Assert.Contains("role=\"alert\"", markup);
            Assert.Contains("role=\"status\"", markup);
            Assert.Contains("for=\"memoContent\"", markup);
            Assert.Contains("id=\"memoContent\"", markup);
        }

        private string GetMemoEditMarkup()
        {
            // MemoEdit.razorファイルの内容を読み取る
            var filePath = "/workspace/ai-MyNotes/Pages/MemoEdit.razor";
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return "";
        }
    }
}