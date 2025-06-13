const mongoose = require('mongoose');

const InvertedIndexSchema = new mongoose.Schema({
  word: { type: String, unique: true },
  pages: {
    type: Object,
    of: new mongoose.Schema({
      wordCount: Number,
      frequency: Number
    }),
    default: {}
  }
});

module.exports = mongoose.model('InvertedIndex', InvertedIndexSchema);