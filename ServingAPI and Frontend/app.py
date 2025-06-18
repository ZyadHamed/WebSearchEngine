from flask import Flask, request, jsonify, render_template
from flask_cors import CORS
import spacy
import re
from string import punctuation
import requests

REDUNDANT_WORDS = {
    "a","an","the", "i","me","my","mine","myself", "you","your","yours","yourself","yourselves",
    "he","him","his","himself","she","her","hers","herself", "it","its","itself", "we","us","our",
    "ours","ourselves", "they","them","their","theirs","themselves", "who","whom","whose","which",
    "what","where","when","why","how", "this","that","these","those", "all","any","both","each",
    "every","few","more","most","much","none","one","other","some","such", "about","above","across",
    "after","against","along","among","around","as","at", "before","behind","below","beneath",
    "beside","between","beyond","by", "down","during","except","for","from", "in","inside","into",
    "near","of","off","on","onto","out","outside","over","past","per","round", "since","through",
    "throughout","to","toward","towards","under","underneath","until","up","upon","with","within",
    "without", "and","but","either","if","neither","nor","or","so","than","that","though","unless",
    "until","when","while","yet","because","although", "am","are","be","been","being","can","could",
    "did","do","does","had","has","have","is","may","might","must","shall","should","was","were",
    "will","would","also","always","away","back","even","ever","first","further","get","go","here",
    "just","know","last","less","many","make","much","never","no","not","now","only","own","perhaps",
    "quite","rather","really","right","said","same","see","seem","s","t","d","ll","m","re","ve","y"
}

class WordStemmer:
    def __init__(self):
        self.doubles = {"bb","dd","ff","gg","mm","nn","pp","rr","tt"}
        self.valid_li_endings = {"c","d","e","g","h","k","m","n","r","t"}
        self.step1b_replacements = [
            ("eedly","ee"),("ingly",""),("edly",""),("eed","ee"),("ing",""),("ed","")
        ]
        self.step2_replacements = [
            ("ization","ize"),("iveness","ive"),("fulness","ful"),("ational","ate"),
            ("ousness","ous"),("biliti","ble"),("tional","tion"),("lessli","less"),
            ("fulli","ful"),("entli","ent"),("ation","ate"),("aliti","al"),
            ("iviti","ive"),("ousli","ous"),("alism","al"),("abli","able"),
            ("anci","ance"),("alli","al"),("izer","ize"),("enci","ence"),
            ("ator","ate"),("bli","ble"),("ogi","og"),("li","")
        ]
        self.step3_replacements = [
            ("ational","ate"),("tional","tion"),("alize","al"),("icate","ic"),
            ("iciti","ic"),("ative",""),("ical","ic"),("ness",""),("ful","")
        ]
        self.step4_replacements = [
            "ement","ment","able","ible","ance","ence","ate",
            "iti","ion","ize","ive","ous","ant","ism","ent","al","er","ic"
        ]
        self.exceptions = {
            "skis":"ski","skies":"sky","dying":"die","lying":"lie","tying":"tie",
            "idly":"idl","gently":"gentl","ugly":"ugli","early":"earli","only":"onli",
            "singly":"singl","sky":"sky","news":"news","howe":"howe","atlas":"atlas",
            "cosmos":"cosmos","bias":"bias","andes":"andes"
        }
        self.exceptions2 = {
            "inning","outing","canning","herring","earring",
            "proceed","exceed","succeed"
        }

    def is_vowel(self, s, i):
        return s[i] in "aeiouy"

    def is_short_syllable(self, s, i):
        return (
            (i==0 and self.is_vowel(s,0) and not self.is_vowel(s,1)) or
            (0<i<len(s)-1 and self.is_vowel(s,i) and not self.is_vowel(s,i+1)
             and s[i+1] not in "wxy" and not self.is_vowel(s,i-1))
        )

    def is_short_word(self, s, r1):
        return r1 >= len(s) and self.is_short_syllable(s, len(s)-2)

    def change_y(self, s):
        chars = list(s)
        if chars and chars[0]=='y':
            chars[0]='Y'
        for i in range(1, len(chars)):
            if chars[i]=='y' and self.is_vowel(chars, i-1):
                chars[i]='Y'
        return "".join(chars)

    def compute_r1_r2(self, s):
        r1 = len(s)
        r2 = len(s)
        if s.startswith(("gener","arsen")):
            r1 = 5
        elif s.startswith("commun"):
            r1 = 6
        else:
            for i in range(1, len(s)):
                if not self.is_vowel(s,i) and self.is_vowel(s,i-1):
                    r1 = i+1
                    break
        for i in range(r1+1, len(s)):
            if not self.is_vowel(s,i) and self.is_vowel(s,i-1):
                r2 = i+1
                break
        return r1, r2

    def step0(self, w):
        return re.sub(r"('s'|'s|')$", "", w)

    def step1a(self, w):
        if w.endswith("sses"):
            return w[:-2]
        if w.endswith(("ied","ies")):
            return w[:-3] + ("ie" if len(w)<=4 else "i")
        if w.endswith(("us","ss")):
            return w
        if w.endswith("s") and any(self.is_vowel(w, i) for i in range(len(w)-2)):
            return w[:-1]
        return w

    def step1b(self, w, r1):
        for suf, rep in self.step1b_replacements:
            if w.endswith(suf):
                base = w[:-len(suf)]
                if suf in ("eed","eedly") and len(base)>=r1:
                    return base + rep
                if any(self.is_vowel(w, i) for i in range(len(w)-len(suf))):
                    w = base + rep
                    if w.endswith(("at","bl","iz")):
                        return w+"e"
                    if w[-2:] in self.doubles:
                        return w[:-1]
                    if self.is_short_word(w, r1):
                        return w+"e"
                break
        return w

    def step1c(self, w):
        if w.endswith(("y","Y")) and len(w)>2 and not self.is_vowel(w, len(w)-2):
            return w[:-1]+"i"
        return w

    def step2(self, w, r1):
        for suf, rep in self.step2_replacements:
            if w.endswith(suf) and len(w)-len(suf)>=r1:
                if suf=="ogi" and w[-len(suf)-1]=="l":
                    return w[:-len(suf)] + rep
                if suf=="li" and w[-len(suf)-1] in self.valid_li_endings:
                    return w[:-2]
                if suf not in ("ogi","li"):
                    return w[:-len(suf)] + rep
        return w

    def step3(self, w, r1, r2):
        for suf, rep in self.step3_replacements:
            if w.endswith(suf) and len(w)-len(suf)>=r1:
                if suf=="ative" and len(w)-len(suf)<r2:
                    return w
                return w[:-len(suf)] + rep
        return w

    def step4(self, w, r2):
        for suf in self.step4_replacements:
            if w.endswith(suf) and len(w)-len(suf)>=r2:
                if suf=="ion" and w[-len(suf)-1] in "st":
                    return w[:-len(suf)]
                elif suf!="ion":
                    return w[:-len(suf)]
        return w

    def step5(self, w, r1, r2):
        if w.endswith("e"):
            base = w[:-1]
            if len(base)>=r2 or (len(base)>=r1 and not self.is_short_syllable(w, len(w)-3)):
                return base
        if w.endswith("ll") and len(w)-1>=r2:
            return w[:-1]
        return w

    def stem(self, word):
        if len(word)<3:
            return word
        w = word.lower().lstrip("'")
        if w in self.exceptions:
            return self.exceptions[w]
        w = self.change_y(w)
        r1, r2 = self.compute_r1_r2(w)
        w = self.step0(w)
        w = self.step1a(w)
        if w in self.exceptions2:
            return w
        w = self.step1b(w, r1)
        w = self.step1c(w)
        w = self.step2(w, r1)
        w = self.step3(w, r1, r2)
        w = self.step4(w, r2)
        w = self.step5(w, r1, r2)
        return w.lower()

stemmer = WordStemmer()
def cleaned_query_tokens(text: str):
    result = []
    # Tokenize using a simple regex: words with optional apostrophes inside
    tokens = re.findall(r"\b\w[\w']*\b", text.lower())
    for token in tokens:
        # Remove trailing punctuation and skip redundant words
        cleaned = token.strip(punctuation)
        if cleaned in REDUNDANT_WORDS:
            continue
        stemmed = stemmer.stem(cleaned)
        result.append(stemmed)
    return result

app = Flask(__name__)
CORS(app)
nlp = spacy.load("en_core_web_sm")

@app.route('/lemmatize', methods=['POST'])
def lemmatize():
    text = request.json.get('text', '')
    # lemmatize + remove stopwords/punctuation
    cleaned_query = cleaned_query_tokens(text)
    query_string = ",".join(cleaned_query)

    try:
        # calling API
        api_url = f"http://localhost:4000/api/search?words={query_string}"
        response = requests.get(api_url)
        response.raise_for_status()  # لو فيه مشكلة يرفع استثناء
        results = response.json()
    except Exception as e:
        return jsonify({"error": str(e)}), 500

    return jsonify({
        "cleaned_query": cleaned_query,
        "sortedResults": results["sortedResults"]
    })

@app.route('/')
def home():
    return render_template('dev.html')  # renders templates/index.html

if __name__ == '__main__':
    app.run(debug=True, port=5000)
