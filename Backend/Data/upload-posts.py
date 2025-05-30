import csv

import requests

BASE_URL = "http://localhost:5224"


def insert_post(access_token: str, content: str):
    repsonse = requests.post(
        BASE_URL + "/posts",
        json={"content": content},
        headers={"Authorization": f"Bearer {access_token}"},
    )

    if repsonse.status_code != 201:
        raise Exception(
            f"Failed to insert post: {repsonse.status_code} - {repsonse.text}")

def main():
    access_token = input()

    with open("posts.csv", "r") as file:
        reader = csv.DictReader(file)
        for row in reader:
            print(f"Inserting post: {row['content']}")
            insert_post(access_token, row["content"])


if __name__ == "__main__":
    main()
