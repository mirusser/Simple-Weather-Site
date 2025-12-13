# Import data to mongo db
MongoDB running in docker

Assumes that `IconDB` database with `IconDocument` collection already exists,
if not, create them

```bash
docker cp /home/mirusser/MyRepos/GitRepos/Simple-Weather-Site/src/IconService/IconService.Api/Icons/Icons-mongo-collection.json mongo:/icons.json
```

```bash
docker exec -it mongo mongoimport   --db IconDB   --collection IconDocument   --file /icons.json   --jsonArray
```