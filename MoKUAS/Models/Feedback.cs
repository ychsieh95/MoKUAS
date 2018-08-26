using MoKUAS.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoKUAS.Models
{
    public class Feedback : IValidatableObject
    {
        public string Email { get; set; }
        
        public int Type { get; set; }
        
        public string Content { get; set; }

        public DateTime DateTime { get; set; }

        public string Creator { get; set; }

        public string Guid { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.Email))
                yield return new ValidationResult("電子郵件信箱禁止為空");
            if (this.Email.GetUTF8BytesCount() > 450)
                yield return new ValidationResult("電子郵件信箱長度限制為 450 半形字元以內，請縮短後再重新提交");
            if (this.Type < 0 || Enum.GetNames(typeof(FeedbackType)).Length < this.Type)
                yield return new ValidationResult("錯誤的回應類型");
            if (string.IsNullOrWhiteSpace(this.Content))
                yield return new ValidationResult("回應內容禁止為空");
            if (this.Content.GetUTF8BytesCount() > 1000)
                yield return new ValidationResult("回應內容長度限制為 1000 半形字元以內，請縮短後再重新提交");
        }
    }

    public enum FeedbackType
    {
        Bugs,
        Suggestion,
        Question,
        Others
    }
}