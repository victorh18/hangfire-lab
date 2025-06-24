Write-Host "Initializing DB for local development..."

# Generate mongo-keyfile (uncomment if needed)
# openssl rand -base64 756 | Out-File -Encoding ASCII mongo-keyfile
# icacls mongo-keyfile /inheritance:r /grant:r "$($env:USERNAME):(R)" /c

[System.Convert]::ToBase64String((1..756 | ForEach-Object {Get-Random -Maximum 256})) | Out-File -NoNewline mongo-keyfile

Remove-Item -Force docker-compose.yml
Copy-Item docker-compose-init.yml docker-compose.yml

docker-compose up -d

Start-Sleep -Seconds 5

docker exec -i mongo-primary mongosh --eval "rs.initiate({ _id: 'rs0', members: [{ _id: 0, host: 'mongo-primary:27017' }, { _id: 1, host: 'mongo-secondary:27017' }, { _id: 2, host: 'mongo-arbiter:27017', arbiterOnly: true }] })"
docker exec -i mongo-primary mongosh --eval "db = db.getSiblingDB('admin'); db.createUser({ user: 'root', pwd: 'someSecret', roles: [{ role: 'root', db: 'admin' }] });"
docker exec -i mongo-primary mongosh --eval "db = db.getSiblingDB('testdb'); db.createRole({ role: 'readWriteTestDB', privileges: [{ resource: { db: 'testdb', collection: 'users' }, actions: ['find', 'insert', 'update'] }], roles: [] }); db.createUser({ user: 'testUser', pwd: 'testUserSecret', roles: [{ role: 'readWriteTestDB', db: 'testdb' }] });"

# docker-compose down

# Get-Content docker-compose-auth.yml | Set-Content docker-compose.yml

# docker-compose up -d

# mongosh mongodb://root:someSecret@mongo-primary/admin?directConnection=true
# mongosh mongodb://root:someSecret@mongo-secondary/admin?directConnection=true
# mongosh mongodb://root:someSecret@mongo-arbiter/admin?directConnection=true

# mongosh mongodb://root:someSecret@mongo-primary,mongo-secondary,mongo-arbiter/admin?replicaSet=rs0
# mongosh mongodb://root:someSecret@mongo-primary:27017,mongo-secondary:27018,mongo-arbiter:27019/admin?replicaSet=rs0

## SOMETIMES, WHEN RESTARTING THE CLUSTER, IT WORKS, SOMETIMES, IT DOES NOT
## FOR THIS REASON THIS SCRIPT IS ILLUSTRATORY ONLY