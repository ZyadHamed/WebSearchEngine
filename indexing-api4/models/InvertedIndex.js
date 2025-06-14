const mongoose = require('mongoose');

const InvertedIndexSchema = new mongoose.Schema({
  _id: { type: String }, 
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
