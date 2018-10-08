# SqlUpdateTool

This tool was designed to read Excel files and prepare SQL update statements. It assumes that:

- the data is on Sheet1 of the selected Excel file
- the column headers are in the first row
- the column headers are the field names for keys or fields to be updated, based on the following syntax:

For keys: Keys.Table.Field
For values: Table.Field

For keys, the first word is literally the word "Keys"; for Table and Field substitute the appropriate table and field name.

Multiple tables can be addressed by concatenating with single pipes; for example, two tables to be updated with a common key name could have headers:

Keys.Table1Name|Table2Name.CommonKeyFieldName, Table1Name.ValueField1Name, Table2Name.ValueField2Name

Database providers could be incorporated to apply the update statements directly, along with some additional logic to check for the uniqueness of keys, that the provided keys are the correct fields, etc.
