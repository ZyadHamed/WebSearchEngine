# Web Crawler API Component

## Overview
A high-performance, multi-threaded web crawling system designed to efficiently traverse and extract data from web pages. This component serves as the foundation of our search engine, implementing sophisticated URL management, content extraction, and text processing capabilities.

## Core Features
- **Concurrent Web Crawling**: Utilizes up to 10 parallel threads, with support for more, for optimal crawling performance
- **Intelligent URL Management**: Implements deduplication and URL normalization
- **Content Processing**: Advanced text analysis including word stemming and frequency calculation
- **Captcha Detection**: Built-in system to identify and handle CAPTCHA challenges
- **Robust Error Handling**: Comprehensive exception management for network issues and malformed pages
- **Database Integration**: SQLite backend for persistent storage of crawled URLs

## Technical Architecture

### Key Components
1. **URL Processing**
   - URLStripper: Normalizes and cleanses URLs
   - Validators: Ensures URL validity and content quality

2. **Content Analysis**
   - WordStemmer: Implements Porter's stemming algorithm
   - RedundantWordsChecker: Filters common stop words
   - PageData: Structured storage of processed page content

3. **Concurrency Management**
   - Thread-safe queue implementation
   - Synchronized data structures for shared resources
   - Cancellation token support for controlled termination

### API Endpoints

```http
POST /api/WebScraping
Body: {
    "URLs": ["url1", "url2", ...],
    "secondsToScrap": integer
}
```

## Performance Features
- Efficient memory management through streaming processing
- Optimized thread pooling for resource utilization
- Intelligent rate limiting to prevent server overload
- URL deduplication to avoid redundant crawling

## Data Structures
- **WebsitesQueue**: Thread-safe queue for URL processing
- **AllScrappedPagesData**: Concurrent collection of processed pages
- **WebsitesReferencingEachSite**: Graph representation of page relationships

## Implementation Details

### Page Processing Pipeline
1. URL validation and normalization
2. HTML content extraction
3. Text processing and analysis
4. Metadata extraction
5. Link discovery and queueing
6. Data persistence

### Thread Safety Mechanisms
- Lock-based synchronization for shared resources
- Atomic operations for counters
- Thread-safe collections for concurrent access

## Configuration
```json
{
    "MaxThreads": 10,
    "RequestTimeout": 30000,
    "UserAgent": "Mozilla/5.0...",
    "BatchSize": 100
}
```

## Dependencies
- HtmlAgilityPack: HTML parsing
- ScrapySharp: Web scraping utilities
- SQLite: Data persistence
- System.Threading.Tasks: Async operations

## Performance Metrics
- Average processing time: ~200ms per page
- Concurrent connections: Up to 10
- Memory footprint: ~100MB baseline

## Error Handling
- Network timeout management
- Invalid URL detection
- Content validation
- Rate limiting responses
- Database connection issues
