using System;
using LibraryAPI.Models;

namespace LibraryAPI.DTOs
{
    // DTO для отображения основной информации о журнале
    public class JournalDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISSN { get; set; }
        public string RegistrationNumber { get; set; }
        public string? Format { get; set; }
        public string? Periodicity { get; set; }
        public int? PagesPerIssue { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? FoundationDate { get; set; }
        public int? Circulation { get; set; }
        public bool IsOpenAccess { get; set; }
        public string? Category { get; set; }
        public string? TargetAudience { get; set; }
        public bool? IsPeerReviewed { get; set; }
        public bool? IsIndexedInRINTS { get; set; }
        public bool? IsIndexedInScopus { get; set; }
        public bool? IsIndexedInWebOfScience { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }
        public string? CoverImageUrl { get; set; }
        public int ShelfId { get; internal set; }
        public int Position { get; internal set; }
    }

    // DTO для создания нового журнала
    public class JournalCreateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISSN { get; set; }
        public string RegistrationNumber { get; set; }
        public string? Format { get; set; }
        public string? Periodicity { get; set; }
        public int? PagesPerIssue { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? FoundationDate { get; set; }
        public int? Circulation { get; set; }
        public bool IsOpenAccess { get; set; }
        public string? Category { get; set; }
        public string? TargetAudience { get; set; }
        public bool? IsPeerReviewed { get; set; }
        public bool? IsIndexedInRINTS { get; set; }
        public bool? IsIndexedInScopus { get; set; }
        public bool? IsIndexedInWebOfScience { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    // DTO для обновления существующего журнала
    public class JournalUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISSN { get; set; }
        public string RegistrationNumber { get; set; }
        public string? Format { get; set; }
        public string? Periodicity { get; set; }
        public int? PagesPerIssue { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public DateTime? FoundationDate { get; set; }
        public int? Circulation { get; set; }
        public bool IsOpenAccess { get; set; }
        public string? Category { get; set; }
        public string? TargetAudience { get; set; }
        public bool? IsPeerReviewed { get; set; }
        public bool? IsIndexedInRINTS { get; set; }
        public bool? IsIndexedInScopus { get; set; }
        public bool? IsIndexedInWebOfScience { get; set; }
        public DateTime? PublicationDate { get; set; }
        public int? PageCount { get; set; }
        public string? CoverImageUrl { get; set; }
    }
}
