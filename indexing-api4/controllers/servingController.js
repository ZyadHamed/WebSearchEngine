const InvertedIndex = require("../models/InvertedIndex");
const PageData = require("../models/PageData");

const searchPages = async (req, res) => {
  const words = req.query.words?.split(",").map(w => w.trim()) || [];
  const sortBy = req.query.sort || "tfidf"; // tfidf هو الافتراضي

  if (words.length === 0) {
    return res.status(400).json({ message: "Please provide words as a comma-separated query string" });
  }

  try {
    const entries = await InvertedIndex.find({ _id: { $in: words } });
    const totalDocs = await PageData.countDocuments();

    const foundWords = entries.map(e => e._id);
    const notFound = words.filter(w => !foundWords.includes(w));

    const pageScores = {}; // pageURL → { count, tfidf, match_words, keywords }

    for (const entry of entries) {
      const idf = Math.log((totalDocs + 1) / (Object.keys(entry.pages).length + 1));

      for (const [url, data] of Object.entries(entry.pages)) {
        const pageDoc = await PageData.findById(url);
        const description = pageDoc?.description || "";
        const totalWords = description.trim().split(/\s+/).length || 100;

        const tf = totalWords > 0 ? data.wordCount / totalWords : 0;
        const tfidf = tf * idf;

        if (!pageScores[url]) {
          pageScores[url] = {
            count: 0,
            tfidf: 0,
            match_words: [],
            keywords: []
          };
        }

        pageScores[url].count += data.wordCount || 0;
        pageScores[url].tfidf += tfidf;
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
        tfidf: score.tfidf || 0,
        match_words: score.match_words || [],
        keywords: score.keywords || []
      };
    });

    if (sortBy === "pagerank") {
      results.sort((a, b) => b.pagerank - a.pagerank);
    } else if (sortBy === "count") {
      results.sort((a, b) => b.count - a.count);
    } else {
      results.sort((a, b) => b.tfidf - a.tfidf); // default: tfidf
    }

    res.json({
      cleaned_query: foundWords,
      not_found: notFound,
      sortedResults: results
    });

  } catch (err) {
    console.error("GET /Search Error:", err);
    res.status(500).json({ message: "Server error", error: err.message });
  }
};


module.exports = { searchPages };
