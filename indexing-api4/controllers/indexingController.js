const InvertedIndex = require("../models/InvertedIndex");
const PageData = require("../models/PageData");

const indexPage = async (req, res) => {
  const { pageURL, words, embeddedURLs, pageTitle, description } = req.body;

  if (!pageURL) {
    return res.status(400).json({ error: "Missing pageURL" });
  }

  try {
    // Store or update words (InvertedIndex)
    if (words) {
      for (let [word, wordData] of Object.entries(words)) {
        const doc = await InvertedIndex.findById(word); // الكلمة هي _id

        if (doc) {
          const pagesCopy = { ...(doc.pages || {}) };

          pagesCopy[pageURL] = {
            wordCount: wordData.wordCount,
            frequency: wordData.frequency,
          };

          doc.pages = pagesCopy;
          await doc.save();
        } else {
          await InvertedIndex.create({
            _id: word,
            pages: {
              [pageURL]: {
                wordCount: wordData.wordCount,
                frequency: wordData.frequency,
              },
            },
          });
        }
      }
    }

    //Store or update backlinks (WebsitesReferencingThisPage)
    if (embeddedURLs?.length > 0) {
      for (let target of embeddedURLs) {
        const targetDoc = await PageData.findById(target); // URL هو _id

        if (targetDoc) {
          targetDoc.WebsitesReferencingThisPage ??= [];

          if (!targetDoc.WebsitesReferencingThisPage.includes(pageURL)) {
            targetDoc.WebsitesReferencingThisPage.push(pageURL);
            await targetDoc.save();
          }
        } else {
          await PageData.create({
            _id: target,
            pageTitle: "Undefined",
            description: "Undefined",
            pageRank: 0,
            WebsitesReferencingThisPage: [pageURL],
            EmbeddedURLs: [],
          });
        }
      }
    }

    // Store or update the current page
    const page = await PageData.findById(pageURL);

    if (!page) {
      await PageData.create({
        _id: pageURL,
        pageTitle: pageTitle || "Undefined",
        description: description || "Undefined",
        pageRank: 0,
        WebsitesReferencingThisPage: [],
        EmbeddedURLs: embeddedURLs || [],
      });
    } else {
      page.pageTitle = pageTitle || page.pageTitle || "Undefined";
      page.description = description || page.description || "Undefined";

      if (embeddedURLs?.length > 0) {
        const currentSet = new Set(page.EmbeddedURLs || []);
        for (let url of embeddedURLs) {
          currentSet.add(url);
        }
        page.EmbeddedURLs = Array.from(currentSet);
      }

      await page.save();
    }

    res.json({ message: "Page indexed successfully" });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Internal Server Error" });
  }
};

module.exports = { indexPage };
