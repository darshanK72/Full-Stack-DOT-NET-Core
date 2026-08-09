/*
 * =============================================================================
 * 14. TRIGGERS IN SQL SERVER
 * =============================================================================
 *
 * TOPIC: DML triggers (AFTER and INSTEAD OF), inserted/deleted virtual tables,
 *        IF UPDATE, ENABLE/DISABLE, and DROP TRIGGER.
 *
 * WHY IT MATTERS:
 *   Triggers run T-SQL automatically when rows change. They are useful for audit
 *   trails, keeping derived columns in sync, and rules that involve more than
 *   one row — things CHECK constraints and FOREIGN KEY alone cannot cover.
 *
 * WHAT YOU WILL LEARN:
 *   1. AFTER INSERT trigger
 *   2. AFTER UPDATE trigger with inserted and deleted
 *   3. AFTER DELETE trigger
 *   4. DISABLE and ENABLE TRIGGER
 *   5. INSTEAD OF trigger on a view
 *
 * Sample database: BikeStores.
 * This file creates temporary practice objects in dbo and removes them at the end.
 *
 * =============================================================================
 */

USE BikeStores;
GO

/*
 * =========================================================================
 * SECTION 1: PRACTICE TABLES — SOURCE AND AUDIT
 * =========================================================================
 *
 * InventoryLog   | Rows we insert, update, and delete in the demos
 * InventoryAudit | Rows the triggers write for auditing
 * -------------------------------------------------------------------------
 */
CREATE TABLE dbo.InventoryLog
(
    item_id      INT IDENTITY(1, 1) PRIMARY KEY,
    item_name    VARCHAR(50) NOT NULL,
    quantity     INT NOT NULL,
    last_updated DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.InventoryAudit
(
    audit_id     INT IDENTITY(1, 1) PRIMARY KEY,
    item_id      INT NOT NULL,
    item_name    VARCHAR(50) NULL,
    old_quantity INT NULL,
    new_quantity INT NULL,
    action_type  VARCHAR(10) NOT NULL,
    audit_time   DATETIME NOT NULL DEFAULT GETDATE()
);
GO

INSERT INTO dbo.InventoryLog (item_name, quantity)
VALUES
    ('Helmet', 25),
    ('Pump', 40);
GO

/*
 * =========================================================================
 * SECTION 2: AFTER INSERT TRIGGER
 * =========================================================================
 *
 * AFTER INSERT fires once the new rows exist in the target table.
 * inserted holds the rows that were just added.
 * -------------------------------------------------------------------------
 */
CREATE TRIGGER dbo.trg_InventoryLog_AfterInsert
ON dbo.InventoryLog
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.InventoryAudit (item_id, item_name, old_quantity, new_quantity, action_type)
    SELECT
        item_id,
        item_name,
        NULL,
        quantity,
        'INSERT'
    FROM inserted;
END;
GO

INSERT INTO dbo.InventoryLog (item_name, quantity)
VALUES ('Lights', 15);
GO

SELECT * FROM dbo.InventoryAudit WHERE action_type = 'INSERT';
GO

/*
 * =========================================================================
 * SECTION 3: AFTER UPDATE TRIGGER — inserted AND deleted
 * =========================================================================
 *
 * On UPDATE, inserted has the new values and deleted has the old values.
 * IF UPDATE(column) limits work to changes on that column.
 * -------------------------------------------------------------------------
 */
CREATE TRIGGER dbo.trg_InventoryLog_AfterUpdate
ON dbo.InventoryLog
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(quantity)
    BEGIN
        INSERT INTO dbo.InventoryAudit (item_id, item_name, old_quantity, new_quantity, action_type)
        SELECT
            i.item_id,
            i.item_name,
            d.quantity,
            i.quantity,
            'UPDATE'
        FROM inserted AS i
        INNER JOIN deleted AS d ON i.item_id = d.item_id
        WHERE i.quantity <> d.quantity;
    END
END;
GO

UPDATE dbo.InventoryLog
SET quantity = 30
WHERE item_name = 'Helmet';
GO

SELECT * FROM dbo.InventoryAudit WHERE action_type = 'UPDATE';
GO

/*
 * =========================================================================
 * SECTION 4: AFTER DELETE TRIGGER
 * =========================================================================
 *
 * deleted holds rows removed from the target table.
 * -------------------------------------------------------------------------
 */
CREATE TRIGGER dbo.trg_InventoryLog_AfterDelete
ON dbo.InventoryLog
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.InventoryAudit (item_id, item_name, old_quantity, new_quantity, action_type)
    SELECT
        item_id,
        item_name,
        quantity,
        NULL,
        'DELETE'
    FROM deleted;
END;
GO

DELETE FROM dbo.InventoryLog
WHERE item_name = 'Pump';
GO

SELECT * FROM dbo.InventoryAudit WHERE action_type = 'DELETE';
GO

/*
 * =========================================================================
 * SECTION 5: DISABLE AND ENABLE TRIGGER
 * =========================================================================
 *
 * Temporarily turn a trigger off without dropping it — useful for bulk loads
 * or troubleshooting.
 * -------------------------------------------------------------------------
 */
DISABLE TRIGGER dbo.trg_InventoryLog_AfterInsert ON dbo.InventoryLog;
GO

INSERT INTO dbo.InventoryLog (item_name, quantity)
VALUES ('Gloves', 10);   -- no audit row because the trigger is disabled

SELECT *
FROM dbo.InventoryAudit
WHERE item_name = 'Gloves';   -- returns no rows
GO

ENABLE TRIGGER dbo.trg_InventoryLog_AfterInsert ON dbo.InventoryLog;
GO

INSERT INTO dbo.InventoryLog (item_name, quantity)
VALUES ('Gloves', 12);   -- audit row is written again

SELECT *
FROM dbo.InventoryAudit
WHERE item_name = 'Gloves';
GO

/*
 * =========================================================================
 * SECTION 6: INSTEAD OF TRIGGER ON A VIEW
 * =========================================================================
 *
 * Views are not always directly updatable. INSTEAD OF intercepts the DML and
 * you decide what happens on the base table(s).
 * -------------------------------------------------------------------------
 */
CREATE VIEW dbo.vw_InventoryByName
AS
SELECT item_id, item_name, quantity
FROM dbo.InventoryLog;
GO

CREATE TRIGGER dbo.trg_vw_InventoryByName_InsteadOfInsert
ON dbo.vw_InventoryByName
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.InventoryLog (item_name, quantity)
    SELECT item_name, quantity
    FROM inserted;
END;
GO

INSERT INTO dbo.vw_InventoryByName (item_name, quantity)
VALUES ('Lock', 8);
GO

SELECT * FROM dbo.InventoryLog WHERE item_name = 'Lock';
GO

/*
 * =========================================================================
 * SECTION 7: CLEANUP
 * =========================================================================
 *
 * Drop practice objects so BikeStores is left unchanged after the tutorial.
 * -------------------------------------------------------------------------
 */
DROP TRIGGER IF EXISTS dbo.trg_vw_InventoryByName_InsteadOfInsert ON dbo.vw_InventoryByName;
DROP TRIGGER IF EXISTS dbo.trg_InventoryLog_AfterInsert ON dbo.InventoryLog;
DROP TRIGGER IF EXISTS dbo.trg_InventoryLog_AfterUpdate ON dbo.InventoryLog;
DROP TRIGGER IF EXISTS dbo.trg_InventoryLog_AfterDelete ON dbo.InventoryLog;
GO

DROP VIEW IF EXISTS dbo.vw_InventoryByName;
DROP TABLE IF EXISTS dbo.InventoryAudit;
DROP TABLE IF EXISTS dbo.InventoryLog;
GO

/*
 * =============================================================================
 * QUICK REFERENCE
 * =============================================================================
 *
 * CREATE TRIGGER name ON table_or_view
 * AFTER | INSTEAD OF INSERT | UPDATE | DELETE
 * AS BEGIN … END;
 *
 * inserted | virtual table of new/changed rows (INSERT, UPDATE)
 * deleted  | virtual table of old/removed rows (UPDATE, DELETE)
 *
 * IF UPDATE(column_name)   -- true when that column appears in an UPDATE
 * DISABLE TRIGGER name ON object;
 * ENABLE TRIGGER name ON object;
 * DROP TRIGGER name ON object;
 *
 * =============================================================================
 */
