const express = require("express");
const mongoose =require("mongoose");
const dotenv=require("dotenv");
dotenv.config();

const InvertedIndex=require("./models/index")

const app =express();
mongoose
        .connect("mongodb+srv://medo:medo123S@cluster0.iwsbycr.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0")
        .then(() => {
            console.log("connected successfully");
        })
        .catch((error) => {
            console.log("error with connecting with the DB ", error);
        });

app.use(express.json());

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