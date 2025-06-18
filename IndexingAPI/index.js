const express = require("express");
const mongoose = require("mongoose");

require("dotenv").config();

const app = express();
app.use(express.json({ limit: "30mb" }));

mongoose
  .connect(
    "mongodb+srv://MENNA:1234@data.dfz6bak.mongodb.net/?retryWrites=true&w=majority&appName=Datae"
  )
  .then(() => console.log("Connected to MongoDB"))
  .catch((err) => console.error("MongoDB connection error:", err));

app.use("/api", require("./routes/indexingRoutes"));
app.use("/api", require("./routes/servingRoutes"));

app.listen(4000, () => console.log("Server running on port 4000"));

const fetch = (...args) =>
  import("node-fetch").then(({ default: fetch }) => fetch(...args));

const sendToCrawler = async () => {
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
  const urlsToCrawl = [
    "https://en.wikipedia.org/wiki/Chemistry",
    "https://en.wikipedia.org/wiki/Science",
    "https://en.wikipedia.org/wiki/History",
    "https://en.wikipedia.org/wiki/Philosophy",
    "https://en.wikipedia.org/wiki/Mathematics",
    "https://en.wikipedia.org/wiki/Technology",
    "https://en.wikipedia.org/wiki/Engineering",
    "https://en.wikipedia.org/wiki/Medicine",
    "https://en.wikipedia.org/wiki/Artificial_intelligence",
    "https://en.wikipedia.org/wiki/Computer_science",
    "https://www.britannica.com/",
    "https://www.nature.com/",
    "https://www.sciencedaily.com/",
    "https://www.sciencemag.org/",
    "https://arxiv.org/",
    "https://www.pnas.org/",
    "https://www.cell.com/",
    "https://academic.oup.com/",
    "https://www.springer.com/",
    "https://journals.sagepub.com/",
    "https://www.mdpi.com/",
    "https://www.frontiersin.org/",
    "https://elifesciences.org/",
    "https://www.researchgate.net/",
    "https://pubmed.ncbi.nlm.nih.gov/",
    "https://www.who.int/",
    "https://www.cdc.gov/",
    "https://www.nih.gov/",
    "https://www.nasa.gov/",
    "https://earthobservatory.nasa.gov/",
    "https://earthdata.nasa.gov/",
    "https://weather.com/",
    "https://www.noaa.gov/",
    "https://www.nationalgeographic.com/",
    "https://www.greenpeace.org/international/",
    "https://www.ipcc.ch/",
    "https://www.unep.org/",
    "https://www.iucn.org/",
    "https://www.wwf.org/",
    "https://www.statista.com/",
    "https://data.worldbank.org/",
    "https://ourworldindata.org/",
    "https://www.oecd.org/",
    "https://www.imf.org/",
    "https://www.un.org/en/",
    "https://www.worldbank.org/",
    "https://www.bbc.com/news",
    "https://www.cnn.com/",
    "https://www.reuters.com/",
    "https://www.nytimes.com/",
    "https://www.theguardian.com/international",
    "https://www.washingtonpost.com/",
    "https://www.aljazeera.com/",
    "https://www.economist.com/",
    "https://www.wsj.com/",
    "https://www.forbes.com/",
    "https://www.bloomberg.com/",
    "https://www.ft.com/",
    "https://www.npr.org/",
    "https://www.vox.com/",
    "https://www.politico.com/",
    "https://foreignpolicy.com/",
    "https://www.hbr.org/",
    "https://www.wired.com/",
    "https://www.technologyreview.com/",
    "https://www.ted.com/talks",
    "https://www.khanacademy.org/",
    "https://www.coursera.org/",
    "https://www.edx.org/",
    "https://ocw.mit.edu/",
    "https://www.udacity.com/",
    "https://www.codecademy.com/",
    "https://www.freecodecamp.org/",
    "https://www.geeksforgeeks.org/",
    "https://www.tutorialspoint.com/",
    "https://www.w3schools.com/",
    "https://developer.mozilla.org/",
    "https://stackoverflow.com/",
    "https://github.com/",
    "https://gitlab.com/",
    "https://bitbucket.org/",
    "https://www.python.org/",
    "https://www.r-project.org/",
    "https://cran.r-project.org/",
    "https://www.djangoproject.com/",
    "https://flask.palletsprojects.com/",
    "https://nodejs.org/",
    "https://reactjs.org/",
    "https://vuejs.org/",
    "https://angular.io/",
    "https://nextjs.org/",
    "https://www.java.com/",
    "https://kotlinlang.org/",
    "https://developer.android.com/",
    "https://developer.apple.com/",
    "https://www.swift.org/",
    "https://isocpp.org/",
    "https://www.mathworks.com/",
    "https://www.wolframalpha.com/",
    "https://mathworld.wolfram.com/",
    "https://www.desmos.com/",
    "https://www.symbolab.com/",
    "https://www.geogebra.org/",
    "https://plato.stanford.edu/",
    "https://iep.utm.edu/",
    "https://www.poetryfoundation.org/",
    "https://www.goodreads.com/",
    "https://www.literaryhub.com/",
    "https://www.gutenberg.org/",
    "https://openlibrary.org/",
    "https://www.archive.org/",
    "https://www.smithsonianmag.com/",
    "https://www.history.com/",
    "https://www.ancient.eu/",
    "https://www.metmuseum.org/",
    "https://www.louvre.fr/en",
    "https://www.rijksmuseum.nl/en",
    "https://www.britishmuseum.org/",
    "https://www.moma.org/",
    "https://www.tate.org.uk/",
    "https://www.getty.edu/",
    "https://www.vatican.va/",
    "https://www.loc.gov/",
    "https://www.nobelprize.org/",
    "https://www.ieee.org/",
    "https://www.acm.org/",
    "https://www.ams.org/",
    "https://www.elsevier.com/",
    "https://www.cambridge.org/",
    "https://www.jstor.org/",
    "https://www.scopus.com/",
    "https://www.webmd.com/",
    "https://www.healthline.com/",
    "https://www.mayoclinic.org/",
    "https://www.medicalnewstoday.com/",
    "https://psychcentral.com/",
    "https://www.apa.org/",
    "https://www.psychologytoday.com/",
    "https://www.ncbi.nlm.nih.gov/",
    "https://www.drugs.com/",
    "https://www.rxlist.com/",
    "https://www.scientificamerican.com/",
    "https://www.discovermagazine.com/",
    "https://www.space.com/",
    "https://www.spacex.com/",
    "https://www.blueorigin.com/",
    "https://www.esa.int/",
    "https://www.nasa.gov/",
    "https://solarsystem.nasa.gov/",
    "https://hubblesite.org/",
    "https://webb.nasa.gov/",
    "https://www.seti.org/",
    "https://www.faa.gov/",
    "https://www.icao.int/",
    "https://www.iata.org/",
    "https://www.transportation.gov/",
    "https://www.energy.gov/",
    "https://www.doe.gov/",
    "https://www.epa.gov/",
    "https://www.fda.gov/",
    "https://www.sec.gov/",
    "https://www.irs.gov/",
    "https://www.census.gov/",
    "https://www.bls.gov/",
    "https://fred.stlouisfed.org/",
    "https://tradingeconomics.com/",
    "https://www.investopedia.com/",
    "https://www.marketwatch.com/",
    "https://finance.yahoo.com/",
    "https://www.nasdaq.com/",
    "https://www.nyse.com/",
    "https://www.coinmarketcap.com/",
    "https://www.coindesk.com/",
    "https://www.ethereum.org/",
    "https://bitcoin.org/en/",
    "https://www.openai.com/",
    "https://deepmind.com/",
    "https://www.anthropic.com/",
    "https://huggingface.co/",
    "https://www.lmsys.org/",
    "https://mlcommons.org/",
    "https://paperswithcode.com/",
    "https://www.kaggle.com/",
    "https://www.tensorflow.org/",
    "https://pytorch.org/",
    "https://scikit-learn.org/",
    "https://numpy.org/",
    "https://pandas.pydata.org/",
    "https://matplotlib.org/",
    "https://seaborn.pydata.org/",
    "https://jupyter.org/",
    "https://colab.research.google.com/",
    "https://www.datacamp.com/",
    "https://www.analyticsvidhya.com/",
    "https://towardsdatascience.com/",
  ];

  try {
    const scrapingKickStarterRequest = await fetch(
      "https://localhost:7072/api/WebScraping?secondsToScrap=3",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(urlsToCrawl),
      }
    );

    const rawResponse = await scrapingKickStarterRequest.text(); // instead of res.json()
    const data = tryParseJSON(rawResponse);

    const scrapingKickStarterResultsStoreRequest = await fetch(
      "http://localhost:4000/api/index",
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      }
    );

    console.log(scrapingKickStarterResultsStoreRequest.json());
  } catch (err) {
    console.error("Failed to reach crawler:", err.message);
  }
};

function tryParseJSON(raw) {
  try {
    return JSON.parse(raw); // Success
  } catch (err) {
    console.warn("Initial parse failed:", err.message);

    const match = err.message.match(/position (\d+)/);
    if (!match) return null;

    const pos = parseInt(match[1]);

    // Try to slice up to error point and close brackets
    const truncated = raw.slice(0, pos);

    // Attempt to close brackets manually
    const fixed = truncated.replace(/,\s*$/, "") + "]";

    try {
      return JSON.parse(fixed);
    } catch (err2) {
      console.warn("Repair failed:", err2.message);
      return null;
    }
  }
}

//sendToCrawler();
