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
            // First time word is seen — save as an array
            entry = new InvertedIndex({ word, pagedata: Object.values(pagedata) });
        } else {
            // Word exists — update its pagedata
            for (const newPage of Object.values(pagedata)) {
                const existingPage = entry.pagedata.find(p => p.pageURL === newPage.pageURL);

                if (!existingPage) {
                    entry.pagedata.push(newPage);
                } else {
                    existingPage.count += newPage.count;
                    existingPage.frequency = Math.max(existingPage.frequency, newPage.frequency);

                    const combinedRefs = new Set([
                        ...existingPage.pagesReferencingThisPage,
                        ...newPage.pagesReferencingThisPage
                    ]);
                    existingPage.pagesReferencingThisPage = Array.from(combinedRefs);
                }
            }
        }

        await entry.save();
        res.json({ message: "Entry saved/updated successfully", data: entry });

    } catch (err) {
        console.error("Server Error:", err);
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