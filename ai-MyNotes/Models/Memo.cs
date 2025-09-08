using System.ComponentModel.DataAnnotations;

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
    }
}