using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryAPI.Models
{
    public class Journal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(20)]
        public string ISSN { get; set; }

        [StringLength(50)]
        public string? RegistrationNumber { get; set; }

        public JournalFormat Format { get; set; }

        public Periodicity? Periodicity { get; set; }

        public int? PagesPerIssue { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; }

        public DateTime? FoundationDate { get; set; }

        public int? Circulation { get; set; }

        public bool IsOpenAccess { get; set; }

        public JournalCategory? Category { get; set; }

        [StringLength(100)]
        public string? TargetAudience { get; set; }

        public bool? IsPeerReviewed { get; set; }

        public bool? IsIndexedInRINTS { get; set; }

        public bool? IsIndexedInScopus { get; set; }

        public bool? IsIndexedInWebOfScience { get; set; }

        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }

        [StringLength(200)]
        public string? CoverImageUrl { get; set; }
        public int ShelfId { get; internal set; }
        public int Position { get; internal set; }
    }

    public enum JournalFormat
    {
        Print,
        Electronic,
        Mixed
    }

    public enum Periodicity
    {
        Weekly,
        BiWeekly,
        Monthly,
        Quarterly,
        BiAnnually,
        Annually
    }

    public enum JournalCategory
    {
        Scientific,
        Popular,
        Entertainment,
        Professional,
        Educational,
        Literary,
        News
    }
}
