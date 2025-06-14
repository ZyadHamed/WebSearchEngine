const InvertedIndex = require("../models/InvertedIndex");
const PageData = require("../models/PageData");

const searchPages = async (req, res) => {
  const words = req.query.words?.split(",").map(w => w.trim()) || [];

  if (words.length === 0) {
    return res.status(400).json({ message: "Please provide words as a comma-separated query string" });
  }

  try {
    const entries = await InvertedIndex.find({ _id: { $in: words } });

    const foundWords = entries.map(e => e._id);
    const notFound = words.filter(w => !foundWords.includes(w));

    const pageScores = {}; // pageURL → { totalCount, matchedWords }

    for (const entry of entries) {
      for (const [url, data] of Object.entries(entry.pages)) {
        if (!pageScores[url]) {
          pageScores[url] = {
            count: 0,
            match_words: [],
            keywords: [],
          };
        }

        pageScores[url].count += data.wordCount;
        pageScores[url].match_words.push(entry._id);

        if (!pageScores[url].keywords.includes(entry._id)) {
          pageScores[url].keywords.push(entry._id);
        }
      }
    }

    const urls = Object.keys(pageScores);
    const pages = await PageData.find({ _id: { $in: urls } });

    const results = pages.map(page => {
      const score = pageScores[page._id] || {};
      return {
        pageURL: page._id,
        pageTitle: page.pageTitle,
        description: page.description,
        pagerank: page.pageRank,
        count: score.count || 0,
        match_words: score.match_words || [],
        keywords: score.keywords || []
      };
    });

    results.sort((a, b) => b.count - a.count);

    res.json({
      cleaned_query: foundWords,
      not_found: notFound,
      sortedResults: results
    });

  } catch (err) {
    console.error("Search Error:", err);
    res.status(500).json({ message: "Server error", error: err.message });
  }
};

module.exports = { searchPages };
