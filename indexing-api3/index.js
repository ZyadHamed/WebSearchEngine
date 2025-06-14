const express = require('express');
const mongoose = require('mongoose');
require('dotenv').config();

const app = express();
app.use(express.json());

mongoose.connect(process.env.MONGO_URI || 'mongodb://localhost:27017/search_engine')
  .then(() => console.log('Connected to MongoDB'))
  .catch(err => console.error('MongoDB connection error:', err));

app.use('/api', require('./routes/indexingRoutes'));
app.use('/api', require('./routes/servingRoutes'));


app.listen(4000, () => console.log('Server running on port 4000'));