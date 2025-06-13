const InvertedIndex = require('../models/InvertedIndex');
const PageData = require('../models/PageData');

const indexPage = async (req, res) => {
  const { pageURL, words, embeddedURLs } = req.body; //Object

  if (!pageURL) {
    return res.status(400).json({error: 'Missing pageURL'});
  }

  try {

    //Store or update words in InvertedIndex
    for (let [word, wordData] of Object.entries(words)) {
      const doc = await InvertedIndex.findOne({ word });

      if (doc) { // word exist already
        const pagesCopy = { ...(doc.pages || {}) }; 

        pagesCopy[pageURL] = { //two cases(page exist &not exist)
          wordCount: wordData.wordCount,     
          frequency: wordData.frequency      
        };

        doc.pages = pagesCopy;
        await doc.save();
      } else {
        await InvertedIndex.create({
          word,
          pages: {
            [pageURL]: {
              wordCount: wordData.wordCount,
              frequency: wordData.frequency
            }
          }
        });
      }
    }

    //Store or update backlinks)(WebsitesReferencingThisPage) in PageData
    if (embeddedURLs?.length > 0) {
      for (let target of embeddedURLs) {
        const targetDoc = await PageData.findOne({ pageURL: target });

        if (targetDoc) {
          targetDoc.WebsitesReferencingThisPage ??= [];

          if (!targetDoc.WebsitesReferencingThisPage.includes(pageURL)) {
            targetDoc.WebsitesReferencingThisPage.push(pageURL);
            await targetDoc.save();
          }
        } else {
          await PageData.create({
            pageURL: target,
            WebsitesReferencingThisPage: [pageURL],
            EmbeddedURLs: []
          });
        }
      }
    }

    //Store or update the current page & EmbeddedURLs
    const page = await PageData.findOne({ pageURL }); 

    if (!page) { // not exist
      await PageData.create({
        pageURL,
        WebsitesReferencingThisPage: [],
        EmbeddedURLs: embeddedURLs || []
      });
    } else {
      page.EmbeddedURLs = embeddedURLs || [];
      await page.save();
    }

    res.json({ message: 'Page indexed successfully' });

  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Internal Server Error' });
  }
};

module.exports = { indexPage };
