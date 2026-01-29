import sys
import os
from pathlib import Path
from google import genai

def load_diff(path: str) -> str:
    try:
        with open(path, "r", encoding="utf-8") as f:
            return f.read()
    except Exception as ex:
        return f"Failed to read diff file: {ex}"

def build_prompt(diff: str) -> str:
    return f"""
You are an expert senior software engineer performing a pull request code review.

Analyze the following Git diff and produce a concise, actionable review.
Focus on:
- correctness
- maintainability
- performance
- security
- .NET best practices
- potential bugs
- missing tests

Format the output in clean Markdown.

Git Diff:

{diff}

"""

def run_gemini_review(diff: str, api_key: str) -> str:
    client = genai.Client(api_key=api_key)

    prompt = build_prompt(diff)

    response = client.models.generate_content(
        model="gemini-3-flash-preview", contents=prompt
    )

    return response.text.strip()

def main():
    if len(sys.argv) < 2:
        print("Usage: python ai_review.py <diff_file> [--api-key <key>]")
        sys.exit(1)

    diff_path = sys.argv[1]
    pathExists = Path(diff_path)
    diff = None

    if pathExists.exists():
        diff = load_diff(diff_path)
    else: 
        diff = diff_path

    # Optional: --api-key argument
    api_key = None
    if "--api-key" in sys.argv:
        idx = sys.argv.index("--api-key")
        if idx + 1 < len(sys.argv):
            api_key = sys.argv[idx + 1]

    # Fallback to environment variable if no CLI key provided
    if not api_key:
        api_key = os.getenv("GEMINI_API_KEY")

    print("API IS " + ("Provided" if api_key else "Not Provided"))

    review = run_gemini_review(diff, api_key)
    print(review)

if __name__ == "__main__":
    main()