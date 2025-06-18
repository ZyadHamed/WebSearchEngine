# Indexing API Component

## Overview
A sophisticated indexing system that processes and stores web page data using MongoDB, featuring advanced PageRank calculations, inverted indexing, and efficient search capabilities. This component serves as the central data processing hub of the search engine.

## Core Features
- **Advanced PageRank Implementation**: Dynamic calculation of page importance
- **Inverted Index System**: Efficient word-to-document mapping
- **TF-IDF Scoring**: Relevance calculation for search results
- **Batch Processing**: Efficient handling of large data sets
- **Real-time Updates**: Continuous data synchronization
- **MongoDB Integration**: Scalable data storage solution

## Technical Architecture

### Database Schema
1. **Inverted Index Collection**
   ```javascript
   {
     _id: String,  // Word
     pages: {
       [pageURL]: {
         wordCount: Number,
         frequency: Number
       }
     }
   }
   ```

2. **Page Data Collection**
   ```javascript
   {
     _id: String,  // PageURL
     pageTitle: String,
     description: String,
     pageRank: Number,
     EmbeddedURLs: [String],
     WebsitesReferencingThisPage: [String]
   }
   ```

### API Endpoints

```http
POST /api/index
- Processes and stores crawled page data
- Updates inverted index
- Calculates PageRank

GET /api/search
- Performs text search
- Supports multiple sorting options (tfidf, pagerank, count)
- Returns relevant page matches
```

## Core Components

### 1. Indexing Engine
- Batch processing of crawled pages
- Word frequency calculation
- Document metadata extraction
- Link graph maintenance

### 2. PageRank Calculator
- Iterative rank computation
- Link analysis
- Damping factor implementation
- Convergence checking

### 3. Search Processor
- Query parsing
- TF-IDF calculation
- Result ranking
- Multi-criteria sorting

## Performance Features
- Bulk database operations
- Efficient data structures
- Optimized query execution
- Caching mechanisms
- Parallel processing capabilities

## Implementation Details

### Indexing Pipeline
1. Data reception and validation
2. Word processing and frequency calculation
3. Inverted index updates
4. PageRank computation
5. Metadata storage
6. Link graph updates

### Search Pipeline
1. Query preprocessing
2. Index lookup
3. Score calculation
4. Result ranking
5. Response formatting

## Configuration
```javascript
{
    "mongodb": {
        "uri": "mongodb+srv://...",
        "options": {
            "retryWrites": true,
            "w": "majority"
        }
    },
    "pageRank": {
        "dampingFactor": 0.85,
        "maxIterations": 100,
        "tolerance": 0.0001
    },
    "indexing": {
        "batchSize": 1000,
        "updateInterval": 200
    }
}
```

## Performance Metrics
- Average indexing time: <100ms per page
- Search response time: <200ms
- PageRank calculation: <1s per page
- Database write throughput: 1000+ ops/second

## Error Handling
- Invalid data detection
- Database connection management
- Timeout handling
- Duplicate entry management
- Query validation
## System Requirements
- Node.js 14+
- MongoDB 4.4+
- 8GB RAM minimum
- Multi-core processor
- Fast storage for database

## Installation
1. Clone repository
2. Install dependencies
3. Configure MongoDB connection
4. Set environment variables
5. Start server

## Usage
```javascript
// Start server
npm start

// Run indexing
POST /api/index
{
  "pages": [...]
}

// Perform search
GET /api/search?words=keyword1,keyword2&sort=tfidf
```

## Future Enhancements
- Distributed indexing
- Advanced caching
- Real-time PageRank updates
- Query suggestion system
- Relevance feedback

