﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoperiaDocumentation.Models
{
    public class FolderModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Status { get; set; } // "New" or "Modified"

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public FolderModel ParentFolder { get; set; }

    }
}
