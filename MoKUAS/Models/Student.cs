using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace MoKUAS.Models
{
    public class Student : IValidatableObject
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string SysYear { get; set; }
        
        public string SysSemester { get; set; }
        
        public string ChtName { get; set; }
        
        public string ChtClass { get; set; }

        public List<Cookie> Cookies { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Username))
                yield return new ValidationResult("帳號不可為空");
            if (string.IsNullOrEmpty(Password))
                yield return new ValidationResult("密碼不可為空");
        }
    }
}