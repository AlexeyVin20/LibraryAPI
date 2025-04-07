using System;
using System.Collections.Generic;
using LibraryAPI.Models;

namespace LibraryAPI.DTOs
{
    // DTO для отображения основной информации о журнале
    public class JournalDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? ISSN { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Format { get; set; }
        public string? Periodicity { get; set; }
        public string? Description { get; set; }
        public string Publisher { get; set; }
        public DateTime? FoundationDate { get; set; }
        public bool IsOpenAccess { get; set; }
        public string? Category { get; set; }
        public string? TargetAudience { get; set; }
        public bool IsPeerReviewed { get; set; }
        public bool IsIndexedInRINTS { get; set; }
        public bool IsIndexedInScopus { get; set; }
        public bool IsIndexedInWebOfScience { get; set; }
        public string? Website { get; set; }
        public string? EditorInChief { get; set; }
        public List<string>? EditorialBoard { get; set; }
        public List<IssueShortDto>? Issues { get; set; }
    }

    // DTO для создания нового журнала
    public class JournalCreateDto
    {
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
    }

    // DTO для обновления существующего журнала
    public class JournalUpdateDto
    {
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
    }

    // DTO для отображения полной информации о выпуске
    public class IssueDto
    {
        public int Id { get; set; }
        public int JournalId { get; set; }
        public string JournalTitle { get; set; }
        public int VolumeNumber { get; set; }
        public int IssueNumber { get; set; }
        public DateTime PublicationDate { get; set; }
        public int PageCount { get; set; }
        public string? Cover { get; set; }
        public int? Circulation { get; set; }
        public string? SpecialTheme { get; set; }
        public int? ShelfId { get; set; }
        public int? Position { get; set; }
        public List<ArticleShortDto>? Articles { get; set; }
    }

    // DTO для краткой информации о выпуске (для списков)
    public class IssueShortDto
    {
        public int Id { get; set; }
        public int VolumeNumber { get; set; }
        public int IssueNumber { get; set; }
        public DateTime PublicationDate { get; set; }
        public string? Cover { get; set; }
        public string? SpecialTheme { get; set; }
    }

    // DTO для создания нового выпуска
    public class IssueCreateDto
    {
        public int JournalId { get; set; }
        public int VolumeNumber { get; set; }
        public int IssueNumber { get; set; }
        public DateTime PublicationDate { get; set; }
        public int PageCount { get; set; }
        public string? Cover { get; set; }
        public int? Circulation { get; set; }
        public string? SpecialTheme { get; set; }
        public int? ShelfId { get; set; }
        public int? Position { get; set; }
    }

    // DTO для обновления существующего выпуска
    public class IssueUpdateDto
    {
        public int VolumeNumber { get; set; }
        public int IssueNumber { get; set; }
        public DateTime PublicationDate { get; set; }
        public int PageCount { get; set; }
        public string? Cover { get; set; }
        public int? Circulation { get; set; }
        public string? SpecialTheme { get; set; }
        public int? ShelfId { get; set; }
        public int? Position { get; set; }
    }

    // DTO для отображения полной информации о статье
    public class ArticleDto
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string? Abstract { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public List<string>? Keywords { get; set; }
        public string? DOI { get; set; }
        public string? ArticleType { get; set; }
        public string? FullText { get; set; }
        public IssueShortDto Issue { get; set; }
    }

    // DTO для краткой информации о статье (для списков)
    public class ArticleShortDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public string? DOI { get; set; }
    }

    // DTO для создания новой статьи
    public class ArticleCreateDto
    {
        public int IssueId { get; set; }
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

    // DTO для обновления существующей статьи
    public class ArticleUpdateDto
    {
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
}
