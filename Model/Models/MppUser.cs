﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Model.Models
{
    public partial class MppUser
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public int? AdminFlag { get; set; }
        public int? Active { get; set; }
        public string Password { get; set; }
        public int? ApproverFlag { get; set; }
        public string UserLevel { get; set; }
        public int? AuthType { get; set; }
        public string EmailId { get; set; }
        public string LanguageCode { get; set; }
    }
}