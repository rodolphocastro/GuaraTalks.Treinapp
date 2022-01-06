# %%
import requests
import json

url = "https://treinapp.com/Sports"

def payload(n):

    payload = json.dumps({
        "name": f'Rocket League Season {n}',
        "description": "E-sport rocket league. RCLS"
    })

    return payload


headers = {
  'Content-Type': 'application/json'
}

c = 1

while True:
    c+=1
    p = payload(c)
    response = requests.request("POST", url, headers=headers, data=p, verify=False)
    print(response.text)
# %%
