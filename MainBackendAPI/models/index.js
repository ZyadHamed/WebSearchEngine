const mongoose = require("mongoose");
const Schema = mongoose.Schema;

const pageSchema=new Schema({
    pageURL: String,
    pageTitle: String,
    pagesReferencingThisPage:[String],
    count: Number,
    frequency: Number,
},{ _id: false });

const indexschema =new Schema({
    word: String,    
    pagedata:{
        type: Map,
        of : pageSchema
    }
});



const InvertedIndex = mongoose.model("InvertedIndex", indexschema);
module.exports=InvertedIndex;
