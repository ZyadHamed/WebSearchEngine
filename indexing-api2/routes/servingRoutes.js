const express = require('express');
const router = express.Router();
const { lemmatizeAndSearch } = require('../controllers/servingController');

router.post('/lemmatize', lemmatizeAndSearch);

module.exports = router;

