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


app.get("/Search", async (req, res) => {
    const words = req.query.words?.split(",") || [];

    if (words.length === 0) {
        return res.status(400).json({ message: "Please provide words as a comma-separated query string" });
    }

    try {
        const results = await InvertedIndex.find({ word: { $in: words } });

        if (results.length < words.length) {
            return res.status(404).json({ message: "Not all words were found" });
        }

        // extract URL for words 
        const allPageURLLists = results.map(entry => entry.pagedata.map(page => page.pageURL));

        // find the common page 
        const commonPages = allPageURLLists.reduce((acc, curr) => acc.filter(url => curr.includes(url)));

        // return data
        const finalPages = results[0].pagedata.filter(p => commonPages.includes(p.pageURL));

        res.json({ commonPages: finalPages });

    } catch (err) {
        res.status(500).json({ message: "Server error", error: err.message });
    }
});

app.listen(3000, () => {
	console.log("I am listening in port 3000");
});