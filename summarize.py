import sys
import json
from transformers import pipeline

def main():
    if sys.stdin.isatty():
    # If no stdin, expect a file path
        if len(sys.argv) < 2:
            print(json.dumps({"error": "No input provided"}))
            return
            with open(sys.argv[1], 'r', encoding='utf-8') as f:
                text = f.read()
        else:
            # Read from stdin
            text = sys.stdin.read()
            try:
                summarizer = pipeline("summarization", model="t5-small")
                summary_list = summarizer(text, max_length=150, min_length=40, do_sample=False)
                summary = summary_list[0]['summary_text']
                print(json.dumps({"summary": summary}))
            except Exception as e:
                print(json.dumps({"error": str(e)}))