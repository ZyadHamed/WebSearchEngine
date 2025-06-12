const express = require("express");
const mongoose =require("mongoose");
const dotenv=require("dotenv");
dotenv.config();

const InvertedIndex=require("./models/index")

const app =express();
//Connect to database
mongoose
        .connect("mongodb+srv://medoehab2005:MMM11@cluster0.sj3gyko.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0")
        .then(() => {
            console.log("connected successfully");
        })
        .catch((error) => {
            console.log("error with connecting with the DB ", error);
        });

app.use(express.json());

//Entry data
app.post("/Search", async (req, res) => {
    const { word, pagedata } = req.body;

    try {
        let entry = await InvertedIndex.findOne({ word });

        if (!entry) {
            // if word didn't exist before
            entry = new InvertedIndex({ word, pagedata });
        } else {
            // if word is exist update it
            for (const [url, newPageData] of Object.entries(pagedata)) {
                const existingPage = entry.pagedata.get(url);

                if (!existingPage) {
                //if the page new make it
                    entry.pagedata.set(url, newPageData);
                } else {
                    //if page alread exisit update count
                    existingPage.count += newPageData.count;
                    existingPage.frequency = Math.max(existingPage.frequency, newPageData.frequency);

                    
                    const combinedRefs = new Set([
                        ...existingPage.pagesReferencingThisPage,
                        ...newPageData.pagesReferencingThisPage
                    ]);
                    existingPage.pagesReferencingThisPage = Array.from(combinedRefs);

                    entry.pagedata.set(url, existingPage);
                }
            }
        }

        await entry.save();
        res.json({ message: "Entry saved/updated successfully", data: entry });

    } catch (err) {
        res.status(500).json({ message: "Server error", error: err.message });
    }
});


//Make the Search
app.get("/Search/:word",async(req,res) =>{
    const word=req.params.word;

    const result = await InvertedIndex.findOne({ word });
    if (result) {
        res.json(result);
} else {
        res.status(404).json({ message: "didn't found" });
}
});





app.listen(3000, () => {
	console.log("I am listening in port 3000");
});