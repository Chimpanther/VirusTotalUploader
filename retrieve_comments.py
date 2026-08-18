import os
import requests

repo = "Chimpanther/VirusTotalUploader"
url = f"https://api.github.com/repos/{repo}/pulls"
headers = {"Accept": "application/vnd.github.v3+json"}
token = os.environ.get("GITHUB_TOKEN")
if token:
    headers["Authorization"] = f"token {token}"

response = requests.get(url, headers=headers)
prs = response.json()
print(f"Found {len(prs)} open PRs.")
for pr in prs:
    print(f"PR #{pr['number']}: {pr['title']}")
    comments_url = pr["review_comments_url"]
    comments_response = requests.get(comments_url, headers=headers)
    comments = comments_response.json()
    print(f"  {len(comments)} review comments")
    for comment in comments:
        print(f"  - {comment['user']['login']}: {comment['body']}")

    issue_comments_url = pr["comments_url"]
    issue_comments_response = requests.get(issue_comments_url, headers=headers)
    issue_comments = issue_comments_response.json()
    print(f"  {len(issue_comments)} issue comments")
    for comment in issue_comments:
        print(f"  - {comment['user']['login']}: {comment['body']}")
