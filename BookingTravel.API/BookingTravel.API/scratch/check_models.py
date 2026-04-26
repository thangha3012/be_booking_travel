import requests

api_key = "AIzaSyDkEEA_Awy1ieB5TJDbXZn_9_qJHTF8RmI"
url = f"https://generativelanguage.googleapis.com/v1/models?key={api_key}"

response = requests.get(url)
print(response.json())
