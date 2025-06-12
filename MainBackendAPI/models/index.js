const mongoose = require("mongoose");
const Schema = mongoose.Schema;

const pageSchema = new Schema({
    pageURL: String,
    pageTitle: String,
    pagesReferencingThisPage: [String],
    count: Number,
    frequency: Number
}, { _id: false });

const indexSchema = new Schema({
    word: String,
    pagedata: [pageSchema]  
});

const InvertedIndex = mongoose.model("InvertedIndex", indexSchema);
module.exports = InvertedIndex;
