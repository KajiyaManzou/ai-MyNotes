using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ai_MyNotes.Tests.Components
{
    public class SimpleComponentTests : TestContext
    {
        [Fact]
        public void SimpleDiv_RendersCorrectly()
        {
            // Arrange & Act
            var component = RenderComponent<SimpleTestComponent>();

            // Assert
            Assert.Equal("<div>Hello from Test Component!</div>", component.Markup);
        }

        [Fact]
        public void ButtonComponent_RendersWithCorrectText()
        {
            // Arrange & Act
            var component = RenderComponent<ButtonTestComponent>();

            // Assert
            var button = component.Find("button");
            Assert.Equal("Click me!", button.TextContent);
            Assert.Equal("btn btn-primary", button.GetAttribute("class"));
        }

        [Fact]
        public void CounterComponent_IncrementsOnClick()
        {
            // Arrange
            var component = RenderComponent<CounterTestComponent>();
            var button = component.Find("button");
            var counterDisplay = component.Find("p");

            // Initial state
            Assert.Contains("Count: 0", counterDisplay.TextContent);

            // Act
            button.Click();

            // Assert
            Assert.Contains("Count: 1", counterDisplay.TextContent);

            // Act again
            button.Click();

            // Assert
            Assert.Contains("Count: 2", counterDisplay.TextContent);
        }
    }

    // テスト用の簡単なコンポーネントを定義
    public class SimpleTestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Hello from Test Component!");
            builder.CloseElement();
        }
    }

    public class ButtonTestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "btn btn-primary");
            builder.AddContent(2, "Click me!");
            builder.CloseElement();
        }
    }

    public class CounterTestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
        private int count = 0;

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            
            builder.OpenElement(1, "p");
            builder.AddContent(2, $"Count: {count}");
            builder.CloseElement();
            
            builder.OpenElement(3, "button");
            builder.AddAttribute(4, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, IncrementCount));
            builder.AddContent(5, "Increment");
            builder.CloseElement();
            
            builder.CloseElement();
        }

        private void IncrementCount()
        {
            count++;
        }
    }
}