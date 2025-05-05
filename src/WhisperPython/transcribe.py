import sys
import json
import whisper

def main():
    if len(sys.argv) < 2:
        print(json.dumps({"error": "No audio file provided"}))
        return

    audio_path = sys.argv[1]

    try:
        model = whisper.load_model("base")  # ou tiny, small, medium
        result = model.transcribe(audio_path)
        print(json.dumps(result))
    except Exception as e:
        print(json.dumps({"error": str(e)}))

if __name__ == "__main__":
    main()