using System;
using System.Collections.Generic;

namespace LibraryAPI.Models
{
    public class Journal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ISSN { get; set; }
        public string? RegistrationNumber { get; set; }
        public JournalFormat Format { get; set; }
        public JournalPeriodicity Periodicity { get; set; }
        public string? Description { get; set; }
        public string Publisher { get; set; }
        public DateTime? FoundationDate { get; set; }
        public bool IsOpenAccess { get; set; }
        public JournalCategory Category { get; set; }
        public string? TargetAudience { get; set; }
        public bool IsPeerReviewed { get; set; }
        public bool IsIndexedInRINTS { get; set; }
        public bool IsIndexedInScopus { get; set; }
        public bool IsIndexedInWebOfScience { get; set; }
        public string? Website { get; set; }
        public string? EditorInChief { get; set; }
        public List<string>? EditorialBoard { get; set; }
        public List<Issue> Issues { get; set; }
    }

    public class Issue
    {
        public int Id { get; set; }
        public int JournalId { get; set; }
        public Journal Journal { get; set; }
        public int VolumeNumber { get; set; }
        public int IssueNumber { get; set; }
        public DateTime PublicationDate { get; set; }
        public int PageCount { get; set; }
        public string? Cover { get; set; }
        public int? Circulation { get; set; }
        public string? SpecialTheme { get; set; }
        public List<Article> Articles { get; set; }
        public int? ShelfId { get; set; }
        public int? Position { get; set; }
    }

    public class Article
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public Issue Issue { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string? Abstract { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public List<string>? Keywords { get; set; }
        public string? DOI { get; set; }
        public ArticleType Type { get; set; }
        public string? FullText { get; set; }
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

    public enum ArticleType
    {
        Research,
        Review,
        CaseStudy,
        Editorial,
        Letter,
        Other
    }
}
