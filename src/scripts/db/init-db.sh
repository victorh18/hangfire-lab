echo "Initializing DB for local development..."
#openssl rand -base64 756 > mongo-keyfile
#chmod 400 mongo-keyfile

rm docker-compose.yml
cp docker-compose-init.yml docker-compose.yml

docker-compose up -d

sleep 5

docker exec -it mongo-primary mongosh --eval 'rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "mongo-primary:27017" }, { _id: 1, host: "mongo-secondary:27017" }, { _id: 2, host: "mongo-arbiter:27017", arbiterOnly: true }] })'
docker exec -it mongo-primary mongosh --eval 'db = db.getSiblingDB("admin"); db.createUser({ user: "root", pwd: "someSecret", roles: [{ role: "root", db: "admin" }] });'
docker exec -it mongo-primary mongosh --eval 'db = db.getSiblingDB("testdb");db.createRole({ role: "readWriteTestDB", privileges: [{ resource: { db: "testdb", collection: "users" }, actions: ["find", "insert", "update"] }], roles: [] });db.createUser({ user: "testUser", pwd: "testUserSecret", roles: [{ role: "readWriteTestDB", db: "testdb" }] });'
docker exec -it mongo-primary mongosh --eval 'db = db.getSiblingDB("hangfire"); db.createUser({ user: "hangfire", pwd: "hangfirePassword", roles: [ { role: "dbOwner", db: "hangfire" } ]});'

docker-compose down

cat docker-compose-auth.yml > docker-compose.yml

docker-compose up -d

## SOMETIMES, WHEN RESTARTING THE CLUSTER, IT WORKS, SOMETIMES, IT DOES NOT
## FOR THIS REASON THIS SCRIPT IS ILLUSTRATORY ONLY