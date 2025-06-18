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

async function bulkRetrieveReferencingPages(targetPageURL, options = {}) {
  // Default options
  const defaultOptions = {
    limit: 1000, // Maximum number of referencing pages to retrieve
    offset: 0, // Pagination offset
    includeFields: ["pageURL", "WebsitesReferencingThisPage", "pageRank"],
  };

  // Merge default options with provided options
  const mergedOptions = { ...defaultOptions, ...options };

  try {
    // Construct efficient database query
    const query = {
      WebsitesReferencingThisPage: {
        $elemMatch: { $eq: targetPageURL }, // Find pages that reference the target URL
      },
    };

    // Perform bulk retrieval with projection and pagination
    const referencingPages = await PageModel.find(query)
      .select(mergedOptions.includeFields)
      .limit(mergedOptions.limit)
      .skip(mergedOptions.offset)
      .lean(); // Use lean for performance optimization

    return {
      pages: referencingPages,
      total: await PageModel.countDocuments(query),
      retrieved: referencingPages.length,
    };
  } catch (error) {
    console.error("Bulk Retrieval Error:", error);
    throw new Error("Failed to retrieve referencing pages");
  }
}
async function getPageRank(pageUrl) {
  try {
    console.log("\n=== Getting PageRank for: ${pageUrl} ===");

    // Step 1: Check if webpage has computed pagerank in PageInformationCollection
    const pageDoc = await this.collection.findOne({ pageURL: pageUrl });

    if (!pageDoc) {
      console.log("Page ${pageUrl} not found in database");
      return null;
    }

    // Check if CurrentPageRank exists and is not 0 (computed)
    if (pageDoc.CurrentPageRank && pageDoc.CurrentPageRank > 0) {
      console.log("Found pre-computed PageRank: ${pageDoc.CurrentPageRank}");
      return pageDoc.CurrentPageRank;
    }

    console.log("PageRank not computed yet, calculating...");

    // Step 2: Send null as response (in practice, we'll calculate and return)
    // Step 3: Send subsequent request to getpagerandata route
    // Step 4-6: Calculate PageRank using the adjacency matrix approach

    const calculatedRank = await this.calculatePageRank(pageUrl);

    // Step 7: Update document entry in PageInformationCollection
    await this.collection.updateOne(
      { pageURL: pageUrl },
      { $set: { CurrentPageRank: calculatedRank } }
    );

    console.log("Calculated and stored PageRank: ${calculatedRank}");
    return calculatedRank;
  } catch (error) {
    console.error("Error getting PageRank:", error);
    throw error;
  }
}

// Calculate PageRank using the iterative algorithm
async function calculatePageRank(targetUrl) {
  try {
    // Get all pages that reference the target page
    const referencingPages = await this.getReferencingPages(targetUrl);
    console.log(
      "Found ${referencingPages.length} pages referencing ${targetUrl}"
    );

    if (referencingPages.length === 0) {
      return 0.15; // Base PageRank for pages with no incoming links
    }

    // Build adjacency information for PageRank calculation
    const adjacencyData = await this.buildAdjacencyMatrix(
      targetUrl,
      referencingPages
    );

    // Calculate PageRank using power iteration method
    const pageRank = this.computePageRankValue(adjacencyData, targetUrl);

    return pageRank;
  } catch (error) {
    console.error("Error calculating PageRank:", error);
    throw error;
  }
}

// Get pages that reference the target page (from WebsitesReferencingThisPage)
async function getReferencingPages(targetUrl) {
  const targetDoc = await this.collection.findOne({ pageURL: targetUrl });

  if (!targetDoc || !targetDoc.WebsitesReferencingThisPage) {
    return [];
  }

  return targetDoc.WebsitesReferencingThisPage;
}

// Build adjacency matrix data for PageRank calculation
async function buildAdjacencyMatrix(targetUrl, referencingPages) {
  const adjacencyData = {
    pages: [targetUrl, ...referencingPages],
    outLinks: new Map(),
    inLinks: new Map(),
  };

  // Initialize maps
  adjacencyData.pages.forEach((page) => {
    adjacencyData.outLinks.set(page, []);
    adjacencyData.inLinks.set(page, []);
  });

  // For each referencing page, get its embedded URLs (outgoing links)
  for (const referencingPage of referencingPages) {
    const pageDoc = await this.collection.findOne({ pageURL: referencingPage });

    if (pageDoc && pageDoc.EmbeddedURLs) {
      // Store outgoing links
      adjacencyData.outLinks.set(referencingPage, pageDoc.EmbeddedURLs);

      // Update incoming links for referenced pages
      pageDoc.EmbeddedURLs.forEach((linkedUrl) => {
        if (adjacencyData.inLinks.has(linkedUrl)) {
          adjacencyData.inLinks.get(linkedUrl).push(referencingPage);
        }
      });
    }
  }

  // Ensure target page has its incoming links set
  adjacencyData.inLinks.set(targetUrl, referencingPages);

  return adjacencyData;
}

// Compute PageRank value using simplified algorithm
function computePageRankValue(adjacencyData, targetUrl) {
  const pages = adjacencyData.pages;
  const n = pages.length;

  if (n <= 1) {
    return 0.15;
  }

  // Initialize PageRank values
  let pageRanks = new Map();
  pages.forEach((page) => {
    pageRanks.set(page, 1.0 / n);
  });

  // Iterative calculation
  for (let iteration = 0; iteration < this.maxIterations; iteration++) {
    const newPageRanks = new Map();
    let hasConverged = true;

    pages.forEach((page) => {
      let rank = (1 - this.dampingFactor) / n;

      // Add contributions from pages that link to this page
      const incomingLinks = adjacencyData.inLinks.get(page) || [];

      incomingLinks.forEach((linkingPage) => {
        const linkingPageRank = pageRanks.get(linkingPage) || 0;
        const outgoingLinksCount = (
          adjacencyData.outLinks.get(linkingPage) || []
        ).length;

        if (outgoingLinksCount > 0) {
          rank += this.dampingFactor * (linkingPageRank / outgoingLinksCount);
        }
      });

      newPageRanks.set(page, rank);

      // Check convergence
      const oldRank = pageRanks.get(page) || 0;
      if (Math.abs(rank - oldRank) > this.tolerance) {
        hasConverged = false;
      }
    });

    pageRanks = newPageRanks;

    if (hasConverged) {
      console.log("PageRank converged after ${iteration + 1} iterations");
      break;
    }
  }

  return pageRanks.get(targetUrl) || 0.15;
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
    for (const pageData of pages) {
      if (!pageData.pageURL) continue;
      const pageRank = await getPageRank(pageData.pageURL);
    }

    res.json({ message: "All pages indexed successfully" });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: "Internal Server Error" });
  }
};

module.exports = { indexPage };
