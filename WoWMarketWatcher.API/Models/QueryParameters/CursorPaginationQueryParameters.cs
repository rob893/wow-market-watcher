using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WoWMarketWatcher.API.Models.QueryParameters
{
    // TODO: Handle url encode/decode (cursors with + for example break)
    public record CursorPaginationQueryParameters : IValidatableObject
    {
        [Range(1, 1000)]
        public int? First { get; init; }

        public string? After { get; init; }

        [Range(1, 1000)]
        public int? Last { get; init; }

        public string? Before { get; init; }

        public bool IncludeTotal { get; init; }

        public bool IncludeNodes { get; init; } = true;

        public bool IncludeEdges { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.First != null && this.Last != null)
            {
                yield return new ValidationResult("Passing both `first` and `last` to paginate is not supported.", new[] { nameof(this.First), nameof(this.Last) });
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