const express = require("express");
const router = express.Router();
const { searchPages } = require("../controllers/servingController");

// GET /api/search?words=run,walk
router.get("/search", searchPages);

module.exports = router;
