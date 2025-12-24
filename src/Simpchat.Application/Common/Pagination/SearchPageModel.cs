using System.ComponentModel.DataAnnotations;

namespace Simpchat.Application.Common.Pagination
{
    public class SearchPageModel
    {
        [Required]
        [MinLength(3, ErrorMessage = "Search term must be at least 3 characters")]
        public string SearchTerm { get; set; } = string.Empty;

        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize { get; set; } = 10;
    }
}
