using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.QueryParameters
{
    public record CursorPaginationParameters : IValidatableObject
    {
        [Range(1, 1000)]
        public int? First { get; init; }
        public string? After { get; init; }
        [Range(1, 1000)]
        public int? Last { get; init; }
        public string? Before { get; init; }
        public bool IncludeTotal { get; init; }
        public bool IncludeNodes { get; init; } = true;
        public bool IncludeEdges { get; init; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.First != null && this.Last != null)
            {
                yield return new ValidationResult("Passing both `first` and `last` to paginate is not supported.", new[] { nameof(this.First), nameof(this.Last) });
            }

            if (this.After != null && this.First == null && this.Last == null)
            {
                yield return new ValidationResult(
                    $"Either `{nameof(this.First)}` or `{nameof(this.Last)}` is required when passing `{nameof(this.After)}`.",
                    new[] { nameof(this.After) });
            }

            if (this.Before != null && this.First == null && this.Last == null)
            {
                yield return new ValidationResult(
                    $"Either `{nameof(this.First)}` or `{nameof(this.Last)}` is required when passing `{nameof(this.Before)}`.",
                    new[] { nameof(this.Before) });
            }

            if (!this.IncludeEdges && !this.IncludeNodes)
            {
                yield return new ValidationResult(
                    $"Both `{nameof(this.IncludeEdges)}` and `{nameof(this.IncludeNodes)}` cannot be false.",
                    new[] { nameof(this.IncludeEdges), nameof(this.IncludeNodes) });
            }
        }
    }
}