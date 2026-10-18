-- Connection bootstrap only. No purchase/customer data is accepted yet.
CREATE TABLE server_metadata (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    schema_version INTEGER NOT NULL CHECK (schema_version >= 1)
);
INSERT INTO server_metadata (id, schema_version) VALUES (1, 1);
