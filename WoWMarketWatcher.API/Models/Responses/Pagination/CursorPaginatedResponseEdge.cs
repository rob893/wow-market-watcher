namespace WoWMarketWatcher.API.Models.Responses.Pagination
{
    /// <summary>
    /// Object representing an edge (a value and its cursor).
    /// </summary>
    /// <typeparam name="TEntity">Type of entity.</typeparam>
    public record CursorPaginatedResponseEdge<TEntity>
    {
        /// <summary>
        /// Gets the cursor.
        /// </summary>
        public string Cursor { get; init; } = default!;

        /// <summary>
        /// Gets the node.
        /// </summary>
        public TEntity Node { get; init; } = default!;
    }
}