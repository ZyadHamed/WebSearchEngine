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

        const foundWords = results.map(r => r.word);
        const missingWords = words.filter(w => !foundWords.includes(w));

        if (results.length === 0) {
            return res.status(404).json({ 
                message: "None of the words were found", 
                missingWords 
            });
        }

        // Step 1: Collect pagedata for each found word
        const wordsData = {};
        const allPageURLLists = [];

        for (const result of results) {
            wordsData[result.word] = result.pagedata;
            allPageURLLists.push(result.pagedata.map(p => p.pageURL));
        }

        // Step 2: Find intersection of page URLs
        const commonURLs = allPageURLLists.reduce((acc, curr) => acc.filter(url => curr.includes(url)));

        // Step 3: Get full page data for the common URLs
        const allPages = results.flatMap(r => r.pagedata);
        const uniquePagesMap = new Map();
        for (const page of allPages) {
            if (commonURLs.includes(page.pageURL)) {
                uniquePagesMap.set(page.pageURL, page); // latest data
            }
        }

        const finalPages = Array.from(uniquePagesMap.values());

        res.json({
            foundWords,
            missingWords,
            wordsData,
            commonPages: finalPages
        });

    } catch (err) {
        res.status(500).json({ message: "Server error", error: err.message });
    }
});


app.listen(3000, () => {
	console.log("I am listening in port 3000");
});