using Microsoft.AspNetCore.Components;

namespace ai_MyNotes.Tests.TestHelpers
{
    // NavigationManagerのモック実装
    public class MockNavigationManager : NavigationManager
    {
        public MockNavigationManager() : base()
        {
            Initialize("https://localhost/", "https://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // テスト用の空実装
        }
    }
}