# afs-demo-apps

Git repo for AFS workshop.
Contains source code for sample apps.

Apps:

- **id** id service (netcore)
- **api** - sample REST API (node.js)
- **app** - sample app (Angular)

![overview](./components-architecture.png)

The master branch contains applications in the **pre-workshop state**.
Perform following changes in a git branch during the workshop:

- add Dockerfile to each app
- expose connection strings and URL settings for k8s cluster deployment
- create and apply k8s definition file
- etc.

See the workshop agenda for the complete list of tasks.

## How to run te apps

### Build and run ID service

```bash
cd id/
dotnet build
dotnet run
```

Browse to [http://localhost:**8088**/swagger/](http://localhost:8088/swagger/) (check port number in the command like output) and execute /test action to try token generation.
Try it with 'symmetric' or 'asymmetric' sigType query parameter.

Provide username and password in token request as as "*Authorizarion: Basic Base64(username:password)*" header.
For example, api keys in base64 for supported users:

- admin:Qwerty!23 - YWRtaW46UXdlcnR5ITIz
- bruce:Qwerty!23 - YnJ1Y2U6UXdlcnR5ITIz
- kerkoroff:Qwerty!23 - a2Vya29yb2ZmOlF3ZXJ0eSEyMw==

The ID service reads account info for user *username* from mongo db, validates if SHA256 hash of the provider password matches hash in the database, then adds roles from the user record to token claims.

Example from the database:

```json
{
    "username": "admin",
    "name": "",
    "roles": [ "admin" ],
    "password_hash" : "05FF5874F9F5A23CEA0BDF9A790D9B0C2F026F28CBB4E8F72EBEBE8F5B7070E2"
}
```

Note! SHA256 for 'Qwerty!23' is '05FF5874F9F5A23CEA0BDF9A790D9B0C2F026F28CBB4E8F72EBEBE8F5B7070E2'

### Build and run Sample API

```bash
# rm previous docker containers, volumes, images and private network:
docker-compose rm -fs mongo
docker rm -fv mongo-seed
docker rmi -f api_mongo-seed
docker network rm api_default

# start mongo db and seed the users database
docker-compose up -d

# init and start the api
npm init
npm run start
```

Open following URL in a browser:

- [http://localhost:**8080**/user/admin](http://localhost:8080/user/admin)
- [http://localhost:8080/user/bruce](http://localhost:8080/user/bruce)
- [http://localhost:8080/user/kerkoroff](http://localhost:8080/user/kerkoroff)

Provide the "Authorizarion: Bearer $jwt" header with every request!
Sample tokens:

```text
admin, asymmetric
eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ7XCJJZFwiOlwiMDZmYTM5ZWYtNTNjZi00ZDNjLWFmNTktODA2ZDA3OTRjMGRmXCIsXCJVc2VybmFtZVwiOlwiYWRtaW5cIn0iLCJqdGkiOiIwNmZhMzllZi01M2NmLTRkM2MtYWY1OS04MDZkMDc5NGMwZGYiLCJpYXQiOiIwNy8xNS8yMDE4IDEzOjUxOjM1Iiwia2V5aWQiOiIxNTMwNjMzMjA1Iiwicm9sZXMiOiJbXCJhZG1pblwiLFwiY29udHJpYnV0b3JcIixcInJlYWRlclwiXSIsIm5iZiI6MTUzMTY2MjY5NSwiZXhwIjoxNTMxODM1NDk1LCJpc3MiOiJleGFtcGxlLmNvbSIsImF1ZCI6ImV4YW1wbGUuY29tIn0.Yj71tw2sZviaK2TB6vkuO3OLWn7WAr_hdG1txzLh4N-_vRHfiOLni5bzHxKxPAuk_eDMmAups6KwoRmHpXixaonuH56uCYb-BQcWKT_kdWX2uB2cQWtqc6keiEv8vDn1qGTcRB6yhma-Wo8WcJrwar9hRm1D1uMyOdGSxzfJkDcD8cWJ4nJO1OeCcBZjm6qrke6Z6__oyvUa-eyzpaeZDrapH_zQeqGQetJf69EAsqss_Ll1yXw2mVGJ42HpWEWFfYsttzvz3MNroFK7kQH14A3lZZG8lyUCT_jye2O9iY_NqNwbdsIQDXYtBDf-c8yHqn5AKitzOyenoARVWzvnMA

bruce, asymmetric
eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ7XCJJZFwiOlwiZmYwY2IwNmEtYTAxYS00ZDIwLThkYmEtZjg1NWNlZWQxMGRjXCIsXCJVc2VybmFtZVwiOlwiYnJ1Y2VcIn0iLCJqdGkiOiJmZjBjYjA2YS1hMDFhLTRkMjAtOGRiYS1mODU1Y2VlZDEwZGMiLCJpYXQiOiIwNy8xNS8yMDE4IDEzOjUyOjEyIiwia2V5aWQiOiIxNTMwNjMzMjA1Iiwicm9sZXMiOiJbXCJyZWFkZXJcIl0iLCJuYmYiOjE1MzE2NjI3MzIsImV4cCI6MTUzMTgzNTUzMiwiaXNzIjoiZXhhbXBsZS5jb20iLCJhdWQiOiJleGFtcGxlLmNvbSJ9.R4Tgkj-9HiWO6ZcVN9Pk_GZJ5nXeRja_G0TuIQljRp2_4c3BtAXxeFTZalifnWHjPWME-wvLjq-cPP4gofwBQaOy4cGorfN1LeaVZyGtx8I3R0sJr0BeTtyR8ZdHA56SVjhrqTBV9swcBGKvZLqUCO6zQ77XR-g6ULqbRUOKg5jpD6cAYsCA36S8oEevggG7NoGA_92hmKSvWdkknO3Vi7BjuZ7krXXamaWjJ6spNNoHM-jXuWdQobbk99c_H7AECRC5UiTpIQEO4jwsUu0zbOwzZEuwFhLpCBcOGfB6YWSCVsP6gNWGK_zFwNdzuuDpU28ye4bK7nYyZoFZa1-_yg

kerkoroff, asymmetric
eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ7XCJJZFwiOlwiZjgyNGYxZTMtODhiMS00ZWMzLWJiZWQtZDEwOTgxYTNmNzFjXCIsXCJVc2VybmFtZVwiOlwia2Vya29yb2ZmXCJ9IiwianRpIjoiZjgyNGYxZTMtODhiMS00ZWMzLWJiZWQtZDEwOTgxYTNmNzFjIiwiaWF0IjoiMDcvMTUvMjAxOCAxMzo1MzowNyIsImtleWlkIjoiMTUzMDYzMzIwNSIsInJvbGVzIjoiW1wiY29udHJpYnV0b3JcIixcInJlYWRlclwiXSIsIm5iZiI6MTUzMTY2Mjc4NywiZXhwIjoxNTMxODM1NTg3LCJpc3MiOiJleGFtcGxlLmNvbSIsImF1ZCI6ImV4YW1wbGUuY29tIn0.IgQ8Veyw0GWTH5XAepeQQCke2RzXmA3yhMXawekpYJuQ0dopwEv8cPUDh-0HPXG5uK71o3na27LS-JyGcbAFEkJ2CBJ_OQaOywsxQV9OZpcg4-4YN03OOAoO-3H3kD8-b7oYFxuNRxCriFlrgfKYOiSqy9zlnjJIOodQutIu5b8NxtRVLPQQ5t1eqzcPTpPI1CYi9p_8Z87zaigTu3D6g_bFVfxrUYbroLmL9dnScZjazOZifRpU6ZH9SjiXSNn0ZHk_R3cWS6LMNAyMSMEKqK0yI7zIMDceN1KJHE1Rt7j6r0SrRgf6UBSk-joj4s8WCEqS5lsDW3D41PQMjmM2QQ
```

Note! This API uses a mongo DB connection, which is by default set to "mongodb://localhost:**27017**/users". Its value shall be configured during the workshop.

### Build and run Sample App

The sample app is the Vue js application. Open index.html in a browser to view the app.

For deployment, we will use nginx base docker image and serve app files as static contents.