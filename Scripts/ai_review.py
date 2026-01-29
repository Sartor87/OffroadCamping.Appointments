import sys
import os
import google.generativeai as genai

def load_diff(path: str) -> str:
    """Reads the diff file produced by the GitHub Action."""
    try:
        with open(path, "r", encoding="utf-8") as f:
            return f.read()
    except Exception as ex:
        return f"Failed to read diff file: {ex}"


def build_prompt(diff: str) -> str:
    """Creates the prompt sent to Gemini."""
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


def run_gemini_review(diff: str) -> str:
    """Sends the diff to Gemini and returns the review text."""
    api_key = os.getenv("GEMINI_API_KEY")
    if not api_key:
        return "❌ GEMINI_API_KEY is missing."

    genai.configure(api_key=api_key)

    model = genai.GenerativeModel("gemini-pro")

    prompt = build_prompt(diff)
    response = model.generate_content(prompt)

    return response.text.strip()


def main():
    if len(sys.argv) < 2:
        print("Usage: python ai_review.py <diff_file>")
        sys.exit(1)

    diff_path = sys.argv[1]
    diff = load_diff(diff_path)

    review = run_gemini_review(diff)
    print(review)


if __name__ == "__main__":
    main()