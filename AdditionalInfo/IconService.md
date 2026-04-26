# Import data to mongo db
MongoDB running in docker

Assumes that `IconDB` database with `IconDocument` collection already exists,
if not, create them

```bash
docker cp /home/mirusser/MyRepos/GitRepos/Simple-Weather-Site/src/IconService/IconService.Api/Icons/Icons-mongo-collection.json mongo_infra:/icons.json
```

```bash
docker exec -it mongo_infra mongoimport   --db IconDB   --collection IconDocument   --file /icons.json   --jsonArray
```
