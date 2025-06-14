from flask import Flask, request, jsonify
from flask_cors import CORS
import spacy
import requests

app = Flask(__name__)
CORS(app)
nlp = spacy.load("en_core_web_sm")

@app.route('/lemmatize', methods=['POST'])
def lemmatize():
    text = request.json.get('text', '')
    doc = nlp(text)

    # lemmatize + remove stopwords/punctuation
    cleaned_query = [token.lemma_.lower() for token in doc if not token.is_stop and not token.is_punct]
    query_string = ",".join(cleaned_query)

    try:
        # calling API
        api_url = f"http://localhost:4000/api/search?words={query_string}"
        response = requests.get(api_url)
        response.raise_for_status()  # لو فيه مشكلة يرفع استثناء
        results = response.json()
    except Exception as e:
        return jsonify({"error": str(e)}), 500

    #Match score calculation and results ranking
    scored_results = []
    for item in results:
        match_keywords = set(cleaned_query) & set(item.get("keywords", []))
        if match_keywords:
            score = len(match_keywords) + item.get("pagerank", 0)
            item_with_matches = item.copy()
            item_with_matches["match_words"] = list(match_keywords)
            scored_results.append((score, item_with_matches))

    sorted_results = sorted(scored_results, key=lambda x: x[0], reverse=True)
    final_results = [item for score, item in sorted_results]

    return jsonify({
        "cleaned_query": cleaned_query,
        "sortedResults": final_results
    })

if __name__ == '__main__':
    app.run(debug=True, port=5000)
