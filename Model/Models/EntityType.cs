﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Model.Models
{
    public partial class EntityType
    {
        public int Id { get; set; }
        public int EditLevel { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string DimensionName { get; set; }
        public string InputTableName { get; set; }
        public int IsStrongEntity { get; set; }
        public int IsManyToMany { get; set; }
        public int IsSubType { get; set; }
        public int IsSuperType { get; set; }
        public int? DefaultSortBy { get; set; }
        public string DefaultSortOrder { get; set; }
        public string DimensionDisplayName { get; set; }
        public int? DisplayOrder { get; set; }
        public string EntityHierarchyGroup { get; set; }
    }
}