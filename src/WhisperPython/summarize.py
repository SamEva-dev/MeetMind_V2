import sys
import json
from transformers import pipeline

def main():
    try:
        # Si stdin est vide (appel direct avec fichier)
        if sys.stdin.isatty():
            if len(sys.argv) < 2:
                print(json.dumps({"error": "No input provided"}))
                return

            # Lire depuis un fichier passé en argument
            with open(sys.argv[1], 'r', encoding='utf-8') as f:
                text = f.read()
        else:
            # Lire depuis stdin (pipe ou redirection)
            text = sys.stdin.read()

        # Lancer le résumé
        summarizer = pipeline("summarization", model="t5-small")
        summary_list = summarizer(text, max_length=150, min_length=40, do_sample=False)
        summary = summary_list[0]['summary_text']

        # Retourner le résultat en JSON
        print(json.dumps({"summary": summary}))

    except Exception as e:
        print(json.dumps({"error": str(e)}))

if __name__ == "__main__":
    main()
