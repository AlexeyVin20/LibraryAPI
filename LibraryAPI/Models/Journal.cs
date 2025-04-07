using System;

namespace LibraryAPI.Models
{
    public class Journal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ISSN { get; set; }
        public string? RegistrationNumber { get; set; }
        public JournalFormat? Format { get; set; }
        public JournalPeriodicity? Periodicity { get; set; }
        public int? PagesPerIssue { get; set; }
        public string? Description { get; set; }
        public string Publisher { get; set; }
        public DateTime? FoundationDate { get; set; }
        public int? Circulation { get; set; }
        public bool IsOpenAccess { get; set; }
        public JournalCategory? Category { get; set; }
        public string? TargetAudience { get; set; }
        public bool? IsPeerReviewed { get; set; }
        public bool? IsIndexedInRINTS { get; set; }
        public bool? IsIndexedInScopus { get; set; }
        public bool? IsIndexedInWebOfScience { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }
        public string? Cover { get; set; }
        public int? ShelfId { get; set; }
        public int? Position { get; set; }
    }

    public enum JournalFormat
    {
        Print,
        Electronic,
        PrintAndElectronic
    }

    public enum JournalPeriodicity
    {
        Weekly,
        Biweekly,
        Monthly,
        Bimonthly,
        Quarterly,
        Semiannually,
        Annually
    }

    public enum JournalCategory
    {
        Scientific,
        Popular,
        Trade,
        Other
    }
}
