# Advanced Web Search Engine

## Overview
A sophisticated, multi-component search engine implementing modern information retrieval techniques, web crawling, and search algorithms. Built with scalability and performance in mind, featuring real-time indexing and intelligent ranking systems.

https://github.com/user-attachments/assets/c65c675c-8ef3-4f9b-ba09-6f49c7827358

## Architecture
The system consists of three main components:

1. **Crawling API** (.NET)
   - Multi-threaded web crawler
   - Content extraction
   - Link discovery
   - Real-time processing
   
   See detailed architecture: [Crawling Plan.png](Plans/Crawling%20Plan.png) \
   See detailed description of crawler API: [Crawling API readme](CrawlerApi/)

3. **Indexing API** (Node.js)
   - Inverted indexing
   - PageRank calculation
   - TF-IDF scoring
   - MongoDB integration
   
   See detailed architecture: [Indexing Plan.png](Plans/Indexing%20Plan.png) \
   See detailed description of indexing API: [Indexing API readme](IndexingAPI/)

5. **Serving API** (Python/Flask)
   - Query processing
   - Results serving
   - Web interface
   - Text analysis
   
   See detailed description of serinvg API and front end: [Serving readme](ServingAPI%20and%20Frontend/)

For a complete system overview, see: [General Plan.png](Plans/General%20Plan.png)

## Features

### Web Crawler
- Concurrent webpage processing
- Intelligent URL management
- Content extraction
- Link graph building

### Indexing System
- Real-time document processing
- Advanced ranking algorithms
- Efficient data structures
- Scalable storage solution

A video showcasing the database structure as well as the updating process within the indexing database happening in real time:

https://github.com/user-attachments/assets/e690ea7f-f165-4960-a631-9cd0b1c27a6d


### Search Interface
- Clean, responsive design
- Real-time results
- Advanced ranking display
- Rich snippets

![UI](https://github.com/user-attachments/assets/b6e5343a-f463-4339-bfcf-e0ec8cf76ae7)

## Technical Stack
- **Crawler**: .NET 6.0, C#
- **Indexer**: Node.js, MongoDB
- **Server**: Python, Flask
- **Frontend**: HTML5, CSS3, JavaScript

