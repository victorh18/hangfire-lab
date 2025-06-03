//openssl rand -base64 756 > mongo-keyfile
//chmod 400 mongo-keyfile

//rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "mongo-primary:27017" }, { _id: 1, host: "mongo-secondary:27017" }, { _id: 2, host: "mongo-arbiter:27017", arbiterOnly: true }] })

// Creating root user
db = db.getSiblingDB("admin");

db.createUser({ user: "root", pwd: "someSecret", roles: [{ role: "root", db: "admin" }] });

// Creating custom database
db = db.getSiblingDB("testdb");

db.createRole({ role: "readWriteTestDB", privileges: [{ resource: { db: "testdb", collection: "users" }, actions: ["find", "insert", "update"] }], roles: [] });

db.createUser({ user: "testUser", pwd: "testUserSecret", roles: [{ role: "readWriteTestDB", db: "testdb" }] });