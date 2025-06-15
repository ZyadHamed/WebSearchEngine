const InvertedIndex = require("../models/InvertedIndex");
const PageData = require("../models/PageData");

async function indexWords(wordsData, pageURL) {
  if (wordsData && typeof wordsData === "object") {
    const bulkOps = [];

    for (let [word, wordData] of Object.entries(wordsData)) {
      if (
        !wordData ||
        typeof wordData.count !== "number" ||
        typeof wordData.frequency !== "number"
      ) {
        continue; // skip invalid word entry
      }

      bulkOps.push({
        updateOne: {
          filter: { _id: word },
          update: {
            $set: {
              [`pages.${pageURL.replace(/\./g, "~dot~")}`]: {
                frequency: wordData.frequency,
              },
            },
          },
          upsert: true,
        },
      });
    }

    if (bulkOps.length > 0) {
      try {
        await InvertedIndex.bulkWrite(bulkOps, {
          ordered: false,
        });
      } catch (error) {
        console.error("Bulk write error:", error);
      }
    }
  }
}
const indexPage = async (req, res) => {
  const pages = req.body;

  if (!Array.isArray(pages) || pages.length === 0) {
    return res
      .status(400)
      .json({ error: "Request must include a non-empty 'pages' array" });
  }

  try {
    for (const pageData of pages) {
      const {
        pageURL,
        pageTitle,
        pagesReferencingThisPage,
        wordsData,
        embeddedURLs,
        description,
      } = pageData;

      if (!pageURL) {
        console.warn("Skipping entry with missing pageURL:", pageData);
        continue; // skip invalid entry
      }

      // 1. Store or update words (InvertedIndex)
      await indexWords(wordsData, pageURL);
      // 3. Store or update the current page
      const page = await PageData.findById(pageURL);

      if (!page) {
        await PageData.create({
          _id: pageURL,
          pageTitle: pageTitle || "Undefined",
          description: description || "Undefined",
          pageRank: 0.15,
          WebsitesReferencingThisPage: pagesReferencingThisPage,
          EmbeddedURLs: embeddedURLs || [],
        });
      } else {
        page.pageTitle = pageTitle || "Undefined";
        page.description = description || "Undefined";

        const existingLinks = new Set(page.WebsitesReferencingThisPage || []);

        let newLinks = pagesReferencingThisPage.filter(
          (link) => !existingLinks.has(link)
        );

        if (newLinks.length > 0) {
          page.WebsitesReferencingThisPage.push(...newLinks);
        }

        await page.save();
      }
    }

    res.json({ message: "All pages indexed successfully" });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Internal Server Error" });
  }
};

module.exports = { indexPage };
