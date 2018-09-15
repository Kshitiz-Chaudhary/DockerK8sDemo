FROM mongo

COPY init.json /init.json
CMD mongoimport --host mongo --db users --collection users --type json --file /init.json --jsonArray