using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ai_MyNotes.Models
{
    /// <summary>
    /// メモデータモデル
    /// </summary>
    public class Memo
    {
        /// <summary>
        /// メモID (主キー)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// メモのタイトル (本文の1行目から自動取得)
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "タイトルは100文字以内で入力してください")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// メモの本文
        /// </summary>
        [Required(ErrorMessage = "本文は必須です")]
        [StringLength(10000, ErrorMessage = "本文は10000文字以内で入力してください")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Memo()
        {
            var now = DateTime.Now;
            CreatedAt = now;
            UpdatedAt = now;
        }

        /// <summary>
        /// 本文からタイトルを抽出してセット
        /// </summary>
        public void UpdateTitleFromContent()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                Title = "無題";
                return;
            }

            var lines = Content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                Title = "無題";
                return;
            }

            var firstLine = lines[0].Trim();
            if (string.IsNullOrEmpty(firstLine))
            {
                Title = "無題";
                return;
            }

            Title = firstLine.Length <= 50 
                ? firstLine 
                : firstLine.Substring(0, 50) + "...";
        }

        /// <summary>
        /// 本文のプレビュー文字列を取得 (一覧表示用)
        /// </summary>
        /// <param name="maxLength">最大文字数 (デフォルト: 100)</param>
        /// <returns>プレビュー文字列</returns>
        public string GetPreview(int maxLength = 100)
        {
            if (string.IsNullOrWhiteSpace(Content))
                return "";

            var cleanContent = Content.Replace('\n', ' ').Replace('\r', ' ').Trim();
            
            if (cleanContent.Length <= maxLength)
                return cleanContent;
                
            return cleanContent.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// 更新日時を現在時刻に設定
        /// </summary>
        public void Touch()
        {
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// メモデータの妥当性を検証
        /// </summary>
        /// <returns>検証結果とエラーメッセージ</returns>
        public (bool IsValid, List<string> Errors) Validate()
        {
            var errors = new List<string>();

            // Titleの検証
            if (string.IsNullOrWhiteSpace(Title))
            {
                errors.Add("タイトルは必須です");
            }
            else if (Title.Length > 100)
            {
                errors.Add("タイトルは100文字以内で入力してください");
            }

            // Contentの検証
            if (string.IsNullOrWhiteSpace(Content))
            {
                errors.Add("本文は必須です");
            }
            else if (Content.Length > 10000)
            {
                errors.Add("本文は10000文字以内で入力してください");
            }

            // 作成日時の検証
            if (CreatedAt == default(DateTime))
            {
                errors.Add("作成日時が設定されていません");
            }
            else if (CreatedAt > DateTime.Now.AddMinutes(1)) // 1分の余裕を持たせる
            {
                errors.Add("作成日時が未来の日付です");
            }

            // 更新日時の検証
            if (UpdatedAt == default(DateTime))
            {
                errors.Add("更新日時が設定されていません");
            }
            else if (UpdatedAt > DateTime.Now.AddMinutes(1)) // 1分の余裕を持たせる
            {
                errors.Add("更新日時が未来の日付です");
            }

            // 作成日時と更新日時の論理的整合性チェック
            if (CreatedAt != default(DateTime) && UpdatedAt != default(DateTime) && UpdatedAt < CreatedAt)
            {
                errors.Add("更新日時は作成日時以降である必要があります");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// 高度なコンテンツバリデーション
        /// </summary>
        /// <returns>検証結果とエラーメッセージ</returns>
        public (bool IsValid, List<string> Errors) ValidateAdvanced()
        {
            var (isBasicValid, basicErrors) = Validate();
            var errors = basicErrors.ToList();

            // 基本バリデーションでコンテンツが存在する場合のみ高度な検証を実行
            if (!string.IsNullOrWhiteSpace(Content) && !basicErrors.Any(e => e.Contains("本文")))
            {
                // 改行のみのコンテンツをチェック
                if (Content.Trim().Length == 0)
                {
                    errors.Add("本文に有効な内容が含まれていません");
                }

                // 異常な制御文字の検出
                if (Content.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
                {
                    errors.Add("本文に無効な制御文字が含まれています");
                }

                // 極端に長い行の検出（パフォーマンス対策）
                var lines = Content.Split('\n');
                if (lines.Any(line => line.Length > 1000))
                {
                    errors.Add("1行あたり1000文字を超える行があります");
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// メモの内容を自動修正
        /// </summary>
        public void AutoCorrect()
        {
            // タイトルの自動修正
            if (string.IsNullOrWhiteSpace(Title) || Title == "無題")
            {
                UpdateTitleFromContent();
            }

            // コンテンツの正規化
            if (!string.IsNullOrEmpty(Content))
            {
                // 連続する空行を単一の空行に変換
                Content = System.Text.RegularExpressions.Regex.Replace(Content, @"\n{3,}", "\n\n");
                
                // 末尾の空白を削除
                Content = Content.TrimEnd();
            }

            // 日時の自動修正
            var now = DateTime.Now;
            if (CreatedAt == default(DateTime))
            {
                CreatedAt = now;
            }
            if (UpdatedAt == default(DateTime))
            {
                UpdatedAt = now;
            }

            // 論理的整合性の修正
            if (UpdatedAt < CreatedAt)
            {
                UpdatedAt = CreatedAt;
            }
        }

        /// <summary>
        /// JSON シリアライゼーション用のIsValidプロパティ
        /// </summary>
        [JsonIgnore]
        public bool IsValid => Validate().IsValid;

        /// <summary>
        /// バリデーションエラー一覧を取得
        /// </summary>
        [JsonIgnore]
        public List<string> ValidationErrors => Validate().Errors;
    }
}