using System;

namespace paSearch
{
    /// <summary>
    /// Settings for the Power Automate Search plugin
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// The last organization URL that was connected
        /// </summary>
        public string LastUsedOrganizationWebappUrl { get; set; }

        /// <summary>
        /// The last selected search category (default: -1 for "All")
        /// </summary>
        public int DefaultSearchCategory { get; set; } = -1;

        /// <summary>
        /// Whether to remember the last search text across sessions
        /// </summary>
        public bool RememberLastSearch { get; set; } = true;

        /// <summary>
        /// The last search text entered by the user
        /// </summary>
        public string LastSearchText { get; set; }

        /// <summary>
        /// Maximum number of search results to return (default: 500)
        /// </summary>
        public int MaxSearchResults { get; set; } = 500;

        /// <summary>
        /// Whether to use fuzzy search matching
        /// </summary>
        public bool UseFuzzySearch { get; set; } = false;

        /// <summary>
        /// Fuzzy search threshold (Levenshtein distance, default: 2)
        /// Lower = stricter match, Higher = more flexible
        /// </summary>
        public int FuzzyThreshold { get; set; } = 2;

        /// <summary>
        /// Default constructor required for XML serialization
        /// </summary>
        public Settings()
        {
        }
    }
}
