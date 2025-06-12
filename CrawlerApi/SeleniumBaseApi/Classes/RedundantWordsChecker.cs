using System;
using System.Collections.Generic;
using System.Linq; // Required for .Where() and .ToArray() in RemoveRedundantWords

namespace SeleniumBaseApi.Classes
{
    public class RedundantWordsChecker
    {
        private readonly HashSet<string> redundantWords;

        public RedundantWordsChecker()
        {
            // Initialize HashSet with StringComparer.OrdinalIgnoreCase for case-insensitivity
            redundantWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Articles
                "a", "an", "the",

                // Pronouns
                "i", "me", "my", "mine", "myself",
                "you", "your", "yours", "yourself", "yourselves",
                "he", "him", "his", "himself",
                "she", "her", "hers", "herself",
                "it", "its", "itself",
                "we", "us", "our", "ours", "ourselves",
                "they", "them", "their", "theirs", "themselves",
                "who", "whom", "whose", "which", "what", "where", "when", "why", "how",
                "this", "that", "these", "those",
                "all", "any", "both", "each", "every", "few", "more", "most", "much", "none", "one", "other", "some", "such",

                // Prepositions
                "about", "above", "across", "after", "against", "along", "among", "around", "as", "at",
                "before", "behind", "below", "beneath", "beside", "between", "beyond", "by",
                "down", "during", "except", "for", "from",
                "in", "inside", "into",
                "near", "of", "off", "on", "onto", "out", "outside", "over", "past", "per", "round",
                "since", "through", "throughout", "to", "toward", "towards",
                "under", "underneath", "until", "up", "upon",
                "with", "within", "without",

                // Conjunctions
                "and", "but", "either", "if", "neither", "nor", "or", "so", "than", "that",
                "though", "unless", "until", "when", "while", "yet", "because", "although",

                // Auxiliary/Modal Verbs
                "am", "are", "be", "been", "being", "can", "could", "did", "do", "does", "had", "has", "have",
                "is", "may", "might", "must", "shall", "should", "was", "were", "will", "would",

                // Common Adverbs & Other Particles/Determiners
                "also", "always", "away", "back", "even", "ever", "first", "further", "get", "go",
                "had", "has", "have", "here", "just", "know", "last", "less", "many", "make", "may", "much",
                "never", "no", "not", "now", "only", "ought", "own",
                "perhaps", "quite", "rather", "really", "right", "said", "same", "see", "seem",
                "should", "simply", "since", "so", "some", "soon", "still", "such",
                "take", "than", "then", "there", "these", "this", "those", "thus", "too",
                "under", "up", "upon", "use", "very", "want", "well", "went", "where", "while", "why", "will", "would",
                // Common short forms (often result from tokenization)
                "s", "t", "d", "ll", "m", "re", "ve", "y" // e.g., isn't -> is, n't; you're -> you, 're
            };
        }

        /// <summary>
        /// Checks if a given word is considered redundant (a stop word).
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <returns>True if the word is redundant, false otherwise.</returns>
        public bool IsRedundant(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return true; 
            }
            return redundantWords.Contains(word);
        }
    }
}