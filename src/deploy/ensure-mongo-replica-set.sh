#!/usr/bin/env bash
set -Eeuo pipefail
trap 'echo "Error on line $LINENO: $BASH_COMMAND"' ERR

MONGO_CONTAINER="mongo_infra"
REPLICA_SET_NAME="rs0"
REPLICA_SET_HOST="mongo:27017"
PRIMARY_WAIT_ATTEMPTS=60

echo "==> Waiting for $MONGO_CONTAINER"
until docker exec "$MONGO_CONTAINER" mongosh --quiet --eval "db.runCommand({ ping: 1 }).ok" | grep -qE '^(1|true)$'; do
    sleep 2
done

echo "==> Ensuring Mongo replica set is initialized"
docker exec "$MONGO_CONTAINER" mongosh --quiet --eval "
const desiredReplicaSet = '$REPLICA_SET_NAME';
const desiredHost = '$REPLICA_SET_HOST';
const primaryWaitAttempts = $PRIMARY_WAIT_ATTEMPTS;

try {
  const status = rs.status();
  if (status.ok === 1) {
    if (status.set !== desiredReplicaSet) {
      print('Replica set is initialized, but not as expected replica set ' + desiredReplicaSet + '.');
      print('Current replica set: ' + status.set);
      quit(1);
    }

    const memberHosts = status.members.map(member => member.name);
    if (!memberHosts.includes(desiredHost)) {
      print('Replica set is initialized, but not with expected member ' + desiredHost + '.');
      print('Current members: ' + memberHosts.join(', '));
      quit(1);
    }

    print('Replica set already initialized');
  }
} catch (error) {
  print('Initializing replica set...');
  rs.initiate({_id: desiredReplicaSet, members: [{_id: 0, host: desiredHost}]});
}

let attempts = primaryWaitAttempts;
while (attempts-- > 0) {
  try {
    const status = rs.status();
    if (status.myState === 1) {
      print('Replica set PRIMARY ready');
      quit(0);
    }
  } catch (error) {
  }
  sleep(1000);
}

print('Timed out waiting for Mongo replica set PRIMARY');
quit(1);
"
