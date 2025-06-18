const mongoose = require('mongoose');

const PageData = new mongoose.Schema({
  _id: { type: String }, 
  pageTitle: { type: String, default: "Unknown Title" },
  description: { type: String, default: "No description available" },
  pageRank: { type: Number, default: 0 },
  EmbeddedURLs: [String],
  WebsitesReferencingThisPage: [String]
});

module.exports = mongoose.model('PageData', PageData);
