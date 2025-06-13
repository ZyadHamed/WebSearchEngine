const mongoose = require('mongoose');

const PageData = new mongoose.Schema({
  pageURL: { type: String, unique: true },
  EmbeddedURLs: [String],
  WebsitesReferencingThisPage: [String]
});

module.exports = mongoose.model('PageData', PageData);
