using ai_MyNotes.Models;
using ai_MyNotes.Services;
using Xunit;

namespace ai_MyNotes.Tests.Services
{
    /// <summary>
    /// MemoServiceの引数検証テスト
    /// 実際のIndexedDB操作は統合テストで実装する
    /// </summary>
    public class MemoServiceArgumentTests
    {
        [Fact]
        public void Constructor_WithNullDbManager_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MemoService(null!));
        }

        [Fact]
        public void MyNotesDatabase_Constants_ShouldHaveExpectedValues()
        {
            // Assert
            Assert.Equal("MyNotesDB", MyNotesDatabase.DatabaseName);
            Assert.Equal(1L, MyNotesDatabase.Version);
            Assert.Equal("memos", MyNotesDatabase.MemoStore);
        }

        [Fact]
        public void MemoModel_BasicValidation_ShouldWorkCorrectly()
        {
            // Arrange - Valid memo
            var validMemo = new Memo { Title = "Valid Title", Content = "Valid Content" };

            // Act - バリデーション部分のみテスト
            var (isValid, errors) = validMemo.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void MemoModel_InvalidData_ShouldFailValidation()
        {
            // Arrange - Invalid memo 
            var invalidMemo = new Memo { Title = "", Content = "" };

            // Act - バリデーション部分のみテスト
            var (isValid, errors) = invalidMemo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("タイトルは必須です", errors);
            Assert.Contains("本文は必須です", errors);
        }

        [Fact]
        public void MemoModel_TitleGeneration_ShouldWorkCorrectly()
        {
            // Arrange
            var memo = new Memo { Content = "First Line\nSecond Line\nThird Line" };

            // Act
            memo.UpdateTitleFromContent();

            // Assert
            Assert.Equal("First Line", memo.Title);
        }

        [Fact]
        public void MemoModel_EmptyContent_ShouldGenerateDefaultTitle()
        {
            // Arrange
            var memo = new Memo { Content = "" };

            // Act
            memo.UpdateTitleFromContent();

            // Assert
            Assert.Equal("無題", memo.Title);
        }

        [Fact]
        public void MemoModel_LongTitle_ShouldTruncate()
        {
            // Arrange
            var memo = new Memo();
            var longLine = new string('a', 60);
            memo.Content = longLine + "\nSecond Line";

            // Act
            memo.UpdateTitleFromContent();

            // Assert
            Assert.Equal(50 + 3, memo.Title.Length); // 50 chars + "..."
            Assert.EndsWith("...", memo.Title);
        }

        [Fact]
        public void MemoModel_Preview_ShouldWorkCorrectly()
        {
            // Arrange
            var memo = new Memo { Content = "Short content" };

            // Act
            var preview = memo.GetPreview();

            // Assert
            Assert.Equal("Short content", preview);
        }

        [Fact]
        public void MemoModel_LongPreview_ShouldTruncate()
        {
            // Arrange
            var memo = new Memo { Content = new string('a', 150) };

            // Act
            var preview = memo.GetPreview(100);

            // Assert
            Assert.Equal(103, preview.Length); // 100 chars + "..."
            Assert.EndsWith("...", preview);
        }

        [Fact]
        public void MemoModel_Touch_ShouldUpdateTimestamp()
        {
            // Arrange
            var memo = new Memo();
            var originalUpdatedAt = memo.UpdatedAt;
            Thread.Sleep(10); // 時間差を作る

            // Act
            memo.Touch();

            // Assert
            Assert.True(memo.UpdatedAt > originalUpdatedAt);
        }

        [Fact]
        public void MemoModel_AutoCorrect_ShouldFixContent()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "";
            memo.Content = "First Line\n\n\n\nSecond Line";

            // Act
            memo.AutoCorrect();

            // Assert
            Assert.Equal("First Line", memo.Title); // タイトルが自動生成される
            Assert.Equal("First Line\n\nSecond Line", memo.Content); // 連続空行が正規化される
        }

        [Fact]
        public void MemoModel_ValidationProperties_ShouldWork()
        {
            // Arrange
            var validMemo = new Memo { Title = "Valid Title", Content = "Valid Content" };
            var invalidMemo = new Memo { Title = "", Content = "" };

            // Act & Assert
            Assert.True(validMemo.IsValid);
            Assert.False(invalidMemo.IsValid);
            Assert.Empty(validMemo.ValidationErrors);
            Assert.NotEmpty(invalidMemo.ValidationErrors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void MemoModel_InvalidTitle_ShouldFailValidation(string invalidTitle)
        {
            // Arrange
            var memo = new Memo { Title = invalidTitle, Content = "Valid Content" };

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("タイトルは必須です", errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void MemoModel_InvalidContent_ShouldFailValidation(string invalidContent)
        {
            // Arrange
            var memo = new Memo { Title = "Valid Title", Content = invalidContent };

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("本文は必須です", errors);
        }

        [Fact]
        public void MemoModel_TooLongTitle_ShouldFailValidation()
        {
            // Arrange
            var memo = new Memo { Title = new string('a', 101), Content = "Valid Content" };

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("タイトルは100文字以内で入力してください", errors);
        }

        [Fact]
        public void MemoModel_TooLongContent_ShouldFailValidation()
        {
            // Arrange
            var memo = new Memo { Title = "Valid Title", Content = new string('a', 10001) };

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("本文は10000文字以内で入力してください", errors);
        }

        [Fact]
        public void MemoModel_AdvancedValidation_WithControlCharacters_ShouldFailValidation()
        {
            // Arrange
            var memo = new Memo { Title = "Valid Title", Content = "Valid content with \x01 control character" };

            // Act
            var (isValid, errors) = memo.ValidateAdvanced();

            // Assert
            Assert.False(isValid);
            Assert.Contains("本文に無効な制御文字が含まれています", errors);
        }

        [Fact]
        public void MemoModel_AdvancedValidation_WithTooLongLine_ShouldFailValidation()
        {
            // Arrange
            var memo = new Memo { Title = "Valid Title", Content = "Valid line\n" + new string('a', 1001) };

            // Act
            var (isValid, errors) = memo.ValidateAdvanced();

            // Assert
            Assert.False(isValid);
            Assert.Contains("1行あたり1000文字を超える行があります", errors);
        }
    }
}