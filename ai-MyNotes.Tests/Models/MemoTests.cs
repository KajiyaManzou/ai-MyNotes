using ai_MyNotes.Models;
using Xunit;

namespace ai_MyNotes.Tests.Models
{
    public class MemoTests
    {
        [Fact]
        public void Constructor_ShouldSetCreatedAtAndUpdatedAt()
        {
            // Arrange & Act
            var memo = new Memo();

            // Assert
            Assert.True(memo.CreatedAt <= DateTime.Now);
            Assert.True(memo.UpdatedAt <= DateTime.Now);
            Assert.Equal(memo.CreatedAt, memo.UpdatedAt);
        }

        [Fact]
        public void Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var memo = new Memo();
            var testTitle = "Test Title";
            var testContent = "Test Content";

            // Act
            memo.Id = 1;
            memo.Title = testTitle;
            memo.Content = testContent;

            // Assert
            Assert.Equal(1, memo.Id);
            Assert.Equal(testTitle, memo.Title);
            Assert.Equal(testContent, memo.Content);
        }

        [Fact]
        public void UpdateTitleFromContent_WithValidContent_ShouldSetTitleToFirstLine()
        {
            // Arrange
            var memo = new Memo();
            memo.Content = "First Line\nSecond Line\nThird Line";

            // Act
            memo.UpdateTitleFromContent();

            // Assert
            Assert.Equal("First Line", memo.Title);
        }

        [Fact]
        public void UpdateTitleFromContent_WithEmptyContent_ShouldSetTitleToDefault()
        {
            // Arrange
            var memo = new Memo();
            memo.Content = "";

            // Act
            memo.UpdateTitleFromContent();

            // Assert
            Assert.Equal("無題", memo.Title);
        }

        [Fact]
        public void UpdateTitleFromContent_WithLongFirstLine_ShouldTruncateTitle()
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
        public void GetPreview_WithShortContent_ShouldReturnFullContent()
        {
            // Arrange
            var memo = new Memo();
            memo.Content = "Short content";

            // Act
            var preview = memo.GetPreview();

            // Assert
            Assert.Equal("Short content", preview);
        }

        [Fact]
        public void GetPreview_WithLongContent_ShouldTruncateWithEllipsis()
        {
            // Arrange
            var memo = new Memo();
            memo.Content = new string('a', 150);

            // Act
            var preview = memo.GetPreview(100);

            // Assert
            Assert.Equal(103, preview.Length); // 100 chars + "..."
            Assert.EndsWith("...", preview);
        }

        [Fact]
        public void Touch_ShouldUpdateUpdatedAt()
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

        [Theory]
        [InlineData("", "本文は必須です")]
        [InlineData("   ", "本文は必須です")]
        [InlineData(null, "本文は必須です")]
        public void Validate_WithInvalidContent_ShouldReturnError(string content, string expectedError)
        {
            // Arrange
            var memo = new Memo();
            memo.Content = content;
            memo.Title = "Valid Title";

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains(expectedError, errors);
        }

        [Fact]
        public void Validate_WithTooLongContent_ShouldReturnError()
        {
            // Arrange
            var memo = new Memo();
            memo.Content = new string('a', 10001);
            memo.Title = "Valid Title";

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("本文は10000文字以内で入力してください", errors);
        }

        [Theory]
        [InlineData("", "タイトルは必須です")]
        [InlineData("   ", "タイトルは必須です")]
        [InlineData(null, "タイトルは必須です")]
        public void Validate_WithInvalidTitle_ShouldReturnError(string title, string expectedError)
        {
            // Arrange
            var memo = new Memo();
            memo.Title = title;
            memo.Content = "Valid Content";

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains(expectedError, errors);
        }

        [Fact]
        public void Validate_WithTooLongTitle_ShouldReturnError()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = new string('a', 101);
            memo.Content = "Valid Content";

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("タイトルは100文字以内で入力してください", errors);
        }

        [Fact]
        public void Validate_WithValidData_ShouldReturnValid()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Valid Content";

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.True(isValid);
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_WithFutureCreatedAt_ShouldReturnError()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Valid Content";
            memo.CreatedAt = DateTime.Now.AddDays(1);

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("作成日時が未来の日付です", errors);
        }

        [Fact]
        public void Validate_WithUpdatedAtBeforeCreatedAt_ShouldReturnError()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Valid Content";
            memo.CreatedAt = DateTime.Now;
            memo.UpdatedAt = memo.CreatedAt.AddDays(-1);

            // Act
            var (isValid, errors) = memo.Validate();

            // Assert
            Assert.False(isValid);
            Assert.Contains("更新日時は作成日時以降である必要があります", errors);
        }

        [Fact]
        public void ValidateAdvanced_WithWhitespaceOnly_ShouldReturnBasicError()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "   \n\n\t  \n  "; // 空白文字のみ - 基本バリデーションで捕捉される

            // Act
            var (isValid, errors) = memo.ValidateAdvanced();

            // Assert
            Assert.False(isValid);
            Assert.Contains("本文は必須です", errors); // 基本バリデーションエラーが返される
        }

        [Fact]
        public void ValidateAdvanced_WithControlCharacters_ShouldReturnError()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Valid content with \x01 control character";

            // Act
            var (isValid, errors) = memo.ValidateAdvanced();

            // Assert
            Assert.False(isValid);
            Assert.Contains("本文に無効な制御文字が含まれています", errors);
        }

        [Fact]
        public void ValidateAdvanced_WithTooLongLine_ShouldReturnError()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Valid line\n" + new string('a', 1001);

            // Act
            var (isValid, errors) = memo.ValidateAdvanced();

            // Assert
            Assert.False(isValid);
            Assert.Contains("1行あたり1000文字を超える行があります", errors);
        }

        [Fact]
        public void AutoCorrect_WithEmptyTitle_ShouldUpdateTitleFromContent()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "";
            memo.Content = "First Line\nSecond Line";

            // Act
            memo.AutoCorrect();

            // Assert
            Assert.Equal("First Line", memo.Title);
        }

        [Fact]
        public void AutoCorrect_WithMultipleEmptyLines_ShouldNormalizeContent()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Line1\n\n\n\nLine2\n\n\n\nLine3";

            // Act
            memo.AutoCorrect();

            // Assert
            Assert.Equal("Line1\n\nLine2\n\nLine3", memo.Content);
        }

        [Fact]
        public void AutoCorrect_WithUpdatedAtBeforeCreatedAt_ShouldFixTiming()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "Valid Title";
            memo.Content = "Valid Content";
            var now = DateTime.Now;
            memo.CreatedAt = now;
            memo.UpdatedAt = now.AddDays(-1);

            // Act
            memo.AutoCorrect();

            // Assert
            Assert.Equal(memo.CreatedAt, memo.UpdatedAt);
        }

        [Fact]
        public void IsValid_Property_ShouldReturnValidationResult()
        {
            // Arrange
            var validMemo = new Memo();
            validMemo.Title = "Valid Title";
            validMemo.Content = "Valid Content";

            var invalidMemo = new Memo();
            invalidMemo.Title = "";
            invalidMemo.Content = "";

            // Act & Assert
            Assert.True(validMemo.IsValid);
            Assert.False(invalidMemo.IsValid);
        }

        [Fact]
        public void ValidationErrors_Property_ShouldReturnErrorList()
        {
            // Arrange
            var memo = new Memo();
            memo.Title = "";
            memo.Content = "";

            // Act
            var errors = memo.ValidationErrors;

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains("タイトルは必須です", errors);
            Assert.Contains("本文は必須です", errors);
        }
    }
}