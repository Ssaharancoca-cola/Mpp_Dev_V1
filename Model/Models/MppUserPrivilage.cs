﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Model.Models
{
    public partial class MppUserPrivilage
    {
        public int EntityTypeId { get; set; }
        public string UserId { get; set; }
        public int? UpdateFlag { get; set; }
        public int? CreateFlag { get; set; }
        public int? ReadFlag { get; set; }
        public int? ImportFlag { get; set; }
        public int EditLevel { get; set; }
        public int? RoleId { get; set; }
        public string Approver { get; set; }
    }
}