-- Run this to get the schema.table.column of the PK and FK in established relationship.

select kcu2.table_schema + '.' + kcu2.table_name + '.' + kcu2.column_name as PrimaryKey
, kcu1.table_schema + '.' + kcu1.table_name + '.' + kcu1.column_name as ForeignKey
from information_schema.referential_constraints rc
join information_schema.key_column_usage kcu1
on kcu1.constraint_catalog = rc.constraint_catalog 
   and kcu1.constraint_schema = rc.constraint_schema
   and kcu1.constraint_name = rc.constraint_name
join information_schema.key_column_usage kcu2
on kcu2.constraint_catalog = rc.unique_constraint_catalog 
   and kcu2.constraint_schema = rc.unique_constraint_schema
   and kcu2.constraint_name = rc.unique_constraint_name
   and kcu2.ordinal_position = kcu1.ordinal_position