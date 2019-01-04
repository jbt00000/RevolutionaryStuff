CREATE proc [db].[ColumnPropertySet]
	@tableName sysname,
	@columnName sysname,
	@propertyVal nvarchar(3750),
	@tableSchema sysname=null,
	@propertyName sysname=null
as
begin
	
	set @propertyName = coalesce(@propertyName, 'MS_Description')
	set @tableSchema = coalesce(@tableSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @tableSchema, N'TABLE', @tableName, N'COLUMN', @columnName)

	if (@cnt>0)
	begin
		EXEC sys.sp_dropextendedproperty @name=@propertyName, @level0type=N'SCHEMA',@level0name=@tableSchema, @level1type=N'TABLE',@level1name=@tableName, @level2type=N'COLUMN',@level2name=@columnName
	end
	EXEC sys.sp_addextendedproperty @name=@propertyName, @value=@propertyVal ,  @level0type=N'SCHEMA',@level0name=@tableSchema, @level1type=N'TABLE',@level1name=@tableName, @level2type=N'COLUMN',@level2name=@columnName

end

GO

create proc [db].[TablePropertySet]
	@tableName sysname,
	@propertyVal sql_variant,
	@tableSchema sysname=null,
	@propertyName sysname=null
as
begin

	set @propertyName = coalesce(@propertyName, 'Comment')
	set @tableSchema = coalesce(@tableSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @tableSchema, N'TABLE', @tableName, null, null)

	if (@cnt>0)
	begin
		EXEC sys.sp_dropextendedproperty   @name=@propertyName, @level0type=N'SCHEMA',@level0name=@tableSchema, @level1type=N'TABLE',@level1name=@tableName
	end
	EXEC sys.sp_addextendedproperty  @name=@propertyName, @value=@propertyVal, @level0type=N'SCHEMA',@level0name=@tableSchema, @level1type=N'TABLE',@level1name=@tableName

end

GO

create view [db].[ColumnProperties]
AS
SELECT s.name SchemaName, t.name TableName, c.name ColumnName, ep.name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.tables AS t with(nolock) 
		ON ep.major_id = t.object_id 
		INNER JOIN 
	sys.schemas AS s with(nolock) 
		on t.schema_id=s.schema_id 
		INNER JOIN
	sys.columns AS c with(nolock) 
		ON ep.major_id = c.object_id 
		AND ep.minor_id = c.column_id
WHERE 
	class = 1 and
	value is not null and
	value <> ''				

GO

create view [db].[TableProperties]
AS
SELECT s.name SchemaName, t.name TableName, ep.Name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.tables AS t with(nolock) 
		ON ep.major_id = t.object_id 
		inner join
	sys.schemas AS s with(nolock) 
		on t.schema_id=s.schema_id
WHERE 
	ep.class = 1 and
	ep.minor_id=0 and
	value is not null and
	value <> ''		

GO

create proc [db].[SprocPropertySet]
	@sprocName sysname,
	@propertyVal sql_variant,
	@routineSchema sysname=null,
	@propertyName sysname=null
as
begin

	set @propertyName = coalesce(@propertyName, 'Comment')
	set @routineSchema = coalesce(@routineSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @routineSchema, N'PROCEDURE', @sprocName, null, null)

	if (@cnt>0)
	begin
		EXEC sys.sp_dropextendedproperty   @name=@propertyName, @level0type=N'SCHEMA',@level0name=@routineSchema, @level1type=N'PROCEDURE',@level1name=@sprocName
	end
	EXEC sys.sp_addextendedproperty  @name=@propertyName, @value=@propertyVal, @level0type=N'SCHEMA',@level0name=@routineSchema, @level1type=N'PROCEDURE',@level1name=@sprocName

end

GO

create proc [db].[SprocParameterPropertySet]
	@sprocName sysname,
	@parameterName sysname,
	@propertyVal nvarchar(3750),
	@sprocSchema sysname=null,
	@propertyName sysname=null
as
begin
	
	set @propertyName = coalesce(@propertyName, 'MS_Description')
	set @sprocSchema = coalesce(@sprocSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @sprocSchema, N'PROCEDURE', @sprocName, N'PARAMETER', @parameterName)

	if (@cnt>0)
	begin
	exec db.printnow 'a'
		EXEC sys.sp_dropextendedproperty   @name=@propertyName, @level0type=N'SCHEMA',@level0name=@sprocSchema, @level1type=N'PROCEDURE',@level1name=@sprocName, @level2type=N'PARAMETER',@level2name=@parameterName
	end
	if (@propertyVal is not null)
	begin
	exec db.printnow 'b'
		EXEC sys.sp_addextendedproperty  @name=@propertyName, @value=@propertyVal, @level0type=N'SCHEMA',@level0name=@sprocSchema, @level1type=N'PROCEDURE',@level1name=@sprocName, @level2type=N'PARAMETER',@level2name=@parameterName
	end

end

GO

create view [db].[SprocProperties]
AS
SELECT s.name SchemaName, t.name SprocName, ep.Name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.procedures AS t with(nolock) 
		ON ep.major_id = t.object_id 
		inner join
	sys.schemas AS s with(nolock) 
		on t.schema_id=s.schema_id
WHERE 
	ep.class = 1 and
	ep.minor_id=0 and
	value is not null and
	value <> ''		

GO

create view [db].[SprocParameterProperties]
AS
SELECT s.name SchemaName, t.name SprocName, p.Name ParameterName, ep.Name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.parameters p with(nolock)
		on p.object_id=ep.major_id
		and p.parameter_id=ep.Minor_Id
		inner join
	sys.procedures AS t with(nolock) 
		ON t.object_id=p.object_id
		inner join
	sys.schemas AS s with(nolock) 
		on t.schema_id=s.schema_id
WHERE 
	ep.class = 2 and
	value is not null and
	value <> ''		

GO

CREATE proc [db].[ViewPropertySet]
	@viewName sysname,
	@propertyVal sql_variant,
	@viewSchema sysname=null,
	@propertyName sysname=null,
	@remove bit=0
as
begin

	set @propertyName = coalesce(@propertyName, 'Comment')
	set @viewSchema = coalesce(@viewSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @viewSchema, N'VIEW', @viewName, null, null)

	if (@cnt>0)
	begin
		EXEC sys.sp_dropextendedproperty   @name=@propertyName, @level0type=N'SCHEMA',@level0name=@viewSchema, @level1type=N'VIEW',@level1name=@viewName
	end
	if (@remove=1) return;
	EXEC sys.sp_addextendedproperty  @name=@propertyName, @value=@propertyVal, @level0type=N'SCHEMA',@level0name=@viewSchema, @level1type=N'VIEW',@level1name=@viewName

end

GO

create view [db].[ViewProperties]
AS
SELECT s.name SchemaName, t.name ViewName, ep.Name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.views AS t with(nolock) 
		ON ep.major_id = t.object_id 
		inner join
	sys.schemas AS s with(nolock) 
		on t.schema_id=s.schema_id
WHERE 
	ep.class = 1 and
	ep.minor_id=0 and
	value is not null and
	value <> ''		

GO

create proc db.ViewColumnPropertySet
	@viewName sysname,
	@columnName sysname,
	@propertyVal nvarchar(3750),
	@viewSchema sysname=null,
	@propertyName sysname=null
as
begin
	
	set @propertyName = coalesce(@propertyName, 'MS_Description')
	set @viewSchema = coalesce(@viewSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @viewSchema, N'VIEW', @viewName, N'COLUMN', @columnName)

	if (@cnt>0)
	begin
		EXEC sys.sp_dropextendedproperty @name=@propertyName, @level0type=N'SCHEMA',@level0name=@viewSchema, @level1type=N'VIEW',@level1name=@viewName, @level2type=N'COLUMN',@level2name=@columnName
	end
	if (@propertyVal is not null)
	begin
		EXEC sys.sp_addextendedproperty @name=@propertyName, @value=@propertyVal ,  @level0type=N'SCHEMA',@level0name=@viewSchema, @level1type=N'VIEW',@level1name=@viewName, @level2type=N'COLUMN',@level2name=@columnName
	end

end

GO

create view [db].[ViewColumnProperties]
AS
SELECT s.name SchemaName, t.name ViewName, c.name ColumnName, ep.name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.views AS t with(nolock) 
		ON ep.major_id = t.object_id 
		INNER JOIN 
	sys.schemas AS s with(nolock) 
		on t.schema_id=s.schema_id 
		INNER JOIN
	sys.columns AS c with(nolock) 
		ON ep.major_id = c.object_id 
		AND ep.minor_id = c.column_id
WHERE 
	class = 1 and
	value is not null and
	value <> ''				

GO

create proc [db].[TriggerPropertySet]
	@tableName sysname,
	@triggerName sysname,
	@propertyVal sql_variant,
	@tableSchema sysname=null,
	@propertyName sysname=null,
	@remove bit=0
as
begin

	set @propertyName = coalesce(@propertyName, 'Comment')
	set @tableSchema = coalesce(@tableSchema, 'dbo')

	declare @cnt int

	select @cnt=count(*)
	from sys.fn_listextendedproperty(@propertyName, N'SCHEMA', @tableSchema, N'TABLE', @tableName, N'TRIGGER', @triggerName)
	exec db.PrintNow 'TriggerPropertySet: There are {n0} existing matching properties ({s3}) on trigger {s0}.{s1}.{s2}', @n0=@cnt, @s0=@tableSchema, @s1=@tableName, @s2=@triggerName, @s3=@propertyName

	if (@cnt>0)
	begin
		EXEC sys.sp_dropextendedproperty @name=@propertyName, @level0type=N'SCHEMA',@level0name=@tableSchema, @level1type=N'TABLE',@level1name=@tableName, @level2type=N'TRIGGER',@level2name=@triggerName
		exec db.PrintNow 'TriggerPropertySet: Deleting property {s0}', @s0=@propertyName
	end
	if (@propertyVal is not null)
	begin
		EXEC sys.sp_addextendedproperty @name=@propertyName, @value=@propertyVal ,  @level0type=N'SCHEMA',@level0name=@tableSchema, @level1type=N'TABLE',@level1name=@tableName, @level2type=N'TRIGGER',@level2name=@triggerName
		declare @sv nvarchar(4000)
		set @sv = try_cast(@propertyVal as nvarchar(4000))
		exec db.PrintNow 'TriggerPropertySet: Adding property {s0}=[{s1}]', @s0=@propertyName, @s1=@sv
	end

end

GO

create view [db].[TriggerProperties]
AS
SELECT s.name SchemaName, ta.name TableName, tr.name TriggerName, ep.name PropertyName, value PropertyValue
FROM 
	sys.extended_properties AS ep with(nolock) 
		INNER JOIN 
	sys.triggers AS tr with(nolock) 
		ON ep.major_id = tr.object_id 
		INNER JOIN 
	sys.tables as ta with(nolock)
		ON ta.object_id=tr.parent_id
		INNER JOIN
	sys.schemas AS s with(nolock) 
		on ta.schema_id=s.schema_id 
WHERE 
	value is not null and
	value <> ''				

GO


CREATE TABLE [db].[ProcResultsetColumns](
	[TABLE_SCHEMA] [sysname] NOT NULL,
	[TABLE_NAME] [sysname] NOT NULL,
	[RESULT_SET] [int] NOT NULL,
	[ResultType] [char](1) NOT NULL,
	[COLUMN_NAME] [sysname] NOT NULL,
	[ORDINAL_POSITION] [int] NOT NULL,
	[IS_NULLABLE] [varchar](3) NOT NULL,
	[DATA_TYPE] [nvarchar](128) NULL,
	[CHARACTER_MAXIMUM_LENGTH] [int] NULL,
	[ErrorMessage] [nvarchar](4000) NULL
)

GO

CREATE proc [db].[ProcResultsetColumnsPopulate] 
AS 
BEGIN 

	set nocount on 

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED   

	declare @z table 
	( 
		TABLE_SCHEMA sysname not null, 
		TABLE_NAME sysname not null, 
		RESULT_SET int not null, 
		ResultType char(1) not null,
		COLUMN_NAME sysname null, 
		ORDINAL_POSITION int not null, 
		IS_NULLABLE varchar(3) not null, 
		DATA_TYPE nvarchar(128), 
		CHARACTER_MAXIMUM_LENGTH int,
		ErrorMessage nvarchar(4000)
	) 

    declare @t table 
    ( 
        is_hidden bit, 
        column_ordinal int, 
        name sysname null, 
        is_nullable bit, 
        system_type_id int, 
        system_type_name nvarchar(128), 
        max_length int, 
        precision int, 
        scale int, 
        collation_name nvarchar(128), 
        user_type_id nvarchar(max), 
        user_type_database sysname null, 
        user_type_schema sysname null, 
        user_type_name sysname null, 
        assembly_qualified_type_name nvarchar(max), 
        xml_collection_id nvarchar(max), 
        xml_collection_database sysname null, 
        xml_collection_schema sysname null, 
        xml_collection_name sysname null, 
        is_xml_document bit, 
        is_case_sensitive bit, 
        is_fixed_length_clr_type bit, 
        source_server sysname null, 
        source_database sysname null, 
        source_schema sysname null, 
        source_table sysname null, 
        source_column sysname null, 
        is_identity_column bit, 
        is_part_of_unique_key bit, 
        is_updateable bit, 
        is_computed_column bit, 
        is_sparse_column_set bit, 
        ordinal_in_order_by_list int, 
        order_by_is_descending int, 
        order_by_list_length int, 
        tds_type_id int, 
        tds_length int, 
        tds_collation_id int, 
        tds_collation_sort_id int         
    ) 

    declare c CURSOR FOR 
    select routine_Schema, routine_name 
    from [INFORMATION_SCHEMA].[ROUTINES] 
    where routine_Type='PROCEDURE'
    order by routine_Schema, routine_name 

    declare @sprocSchema sysname 
    declare @sprocName sysname 
    declare @sql nvarchar(max) 

    open c 

NextItem: 
          
    FETCH NEXT FROM c   
    INTO @sprocSchema, @sprocName   
  
    if @@FETCH_STATUS = 0   
    BEGIN 

        set @sql = N'exec '+quotename(@sprocSchema)+'.'+quotename(@sprocName) 

        exec db.printnow 'Checking => sp_describe_first_result_set @tsql = N''{s0}''', @s0=@sql 

        begin try 

                insert into @t 
                exec sp_describe_first_result_set @tsql = @sql 

				if (@@rowcount=0)
				begin
					exec db.PrintNow 'no columns'
					insert into @z
					values (@sprocSchema, @sprocName, 0, '0', 'void', 0, 'no', 'void', 0, null)
				end
        end try 
        begin catch 

                declare @ErrorNumber int=ERROR_NUMBER() 
                declare @ErrorSeverity int=ERROR_SEVERITY() 
                declare @ErrorState int=ERROR_STATE() 
                declare @ErrorProcedure nvarchar(128)=ERROR_PROCEDURE() 
                declare @ErrorLine int=ERROR_LINE() 
                declare @ErrorMessage nvarchar(4000)=ERROR_MESSAGE() 
        
                exec db.PrintNow 
                        'EXCEPTION: 
ErrorNumber={n0} 
ErrorSeverity={n1} 
ErrorState={n2} 
ErrorProcedure={s0} 
ErrorLine={n3} 
ErrorMessage={s1}', 
                        @n0=@ErrorNumber, 
                        @n1=@ErrorSeverity, 
                        @n2=@ErrorState, 
                        @s0=@ErrorProcedure, 
                        @n3=@ErrorLine, 
                        @s1=@ErrorMessage; 

			insert into @z
			values (@sprocSchema, @sprocName, 0, 'e', 'void', 0, 'no', 'void', 0, @ErrorMessage)

        end catch 

        insert into @z 
        select 
            @sprocSchema, 
            @sprocName, 
            0, 
			'c',
            case when t.name is null then 'Field'+cast(t.column_ordinal as varchar(10)) else t.name end, 
            t.column_ordinal, 
            t.is_nullable, 
			case when charindex('(', t.system_type_name)=0 then t.system_type_name else left(t.system_type_name, charindex('(', t.system_type_name)-1) end, 
            t.max_length,
			null 
        from 
            @t t 

        delete from @t 

        goto NextItem 

    END 
          
    close c 

    deallocate c 

	begin tran
		truncate table db.ProcResultsetColumns
		insert into db.ProcResultsetColumns
		select * from @z
	commit tran

END 

GO

create view db.SchemaTables
as
select e.object_id ObjectId, s.name SchemaName, e.name TableName, e.Table_type TableType
from
	(
		select object_id, name, schema_id, 'VIEW' table_type from sys.views v (nolock)
		union all
		select object_id, name, schema_id, 'BASE TABLE' table_type from sys.tables t (nolock)
	) e
		inner join
	sys.schemas s  (nolock)
		on s.schema_id=e.schema_id

GO

CREATE view [db].[SchemaMeta]
as

with
prc(table_schema, table_name, result_set, resulttype, column_name, ordinal_position, is_nullable, data_type, character_maximum_length, ErrorMessage) as
(
	select *
	from db.ProcResultsetColumns with(nolock)
	union all
	select table_schema, table_name, 0, 'c', column_name, ordinal_position, is_nullable, data_type, character_maximum_length, null
	from [INFORMATION_SCHEMA].[ROUTINE_columns] with(nolock)
),
a(X) as
(
	select *
	from
	(
		select [Tables], Sprocs
		from
		(
			select
			(
				select
					t.table_Schema '@schema',
					t.table_name '@name',
					t.table_type '@tableType',
					coalesce(t.IsNodeTable, cast(0 as bit)) '@isNodeTable',
					coalesce(t.IsEdgeTable, cast(0 as bit)) '@isEdgeTable',
					(
						select 	PropertyName '@name', PropertyValue '@value'
						from
						(
							select 
								p.PropertyName,
								p.PropertyValue
							from 
								db.TableProperties p with (nolock)
							where
								p.SchemaName = t.table_schema and
								p.TableName = t.table_name and
								t.table_type='BASE TABLE'

							union all 

							select 
								p.PropertyName,
								p.PropertyValue
							from 
								db.ViewProperties p with (nolock)
							where
								p.SchemaName = t.table_schema and
								p.ViewName = t.table_name and
								t.table_type='VIEW'
						) a
						for xml path ('Property'), type
					) Properties,
					(
						select 
							c.column_name '@name',
							c.ordinal_position '@position',
							coalesce(pk.isPk,0) '@isPrimaryKey',
							COLUMNPROPERTY(object_id(c.table_schema+'.'+c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') '@isIdentity',
							cast(case when c.is_nullable='YES' then 1 else 0 end as bit) '@isNullable',
							c.data_type '@sqlType',
							c.character_maximum_length '@maxLen',
							c.column_default '@default',
							case when cc.ColumnName is not null then cast (1 as bit) else cast(0 as bit) end '@isComputed',
							ref.TABLE_SCHEMA '@refSchema',
							ref.TABLE_NAME '@refTable',
							c.domain_schema '@domainSchema',
							c.domain_name '@domainName',
							(
								select 
									PropertyName '@name',
									PropertyValue '@value'
								from 
								(
									select p.PropertyName, p.PropertyValue
									from
										db.ColumnProperties p with (nolock)
									where
										p.SchemaName = c.table_schema and
										p.TableName = c.table_name and
										p.ColumnName = c.column_name and
										t.table_type='BASE TABLE'

									union all
							
									select p.PropertyName, p.PropertyValue
									from
										db.ViewColumnProperties p with (nolock)
									where
										p.SchemaName = c.table_schema and
										p.ViewName = c.table_name and
										p.ColumnName = c.column_name and
										t.table_type='VIEW'

                                    union all

                                    select PropertyName, PropertyValue
                                    from
                                    (
                                        select 
	                                        'AccessModifier' PropertyName, 'missing' PropertyValue,
	                                        ss.name SchemaName, st.name TableName, sc.name ColumnName--, sc.*
                                        from 
	                                        sys.columns sc with (nolock)
		                                        inner join
	                                        sys.tables st with (nolock)
		                                        on st.object_id=sc.object_id
		                                        inner join
	                                        sys.schemas ss with (nolock)
		                                        on ss.schema_id=st.schema_id
                                        where
	                                        sc.is_hidden=1 or sc.graph_type is not null
                                    ) stc
                                    where
										stc.SchemaName = c.table_schema and
										stc.TableName = c.table_name and
										stc.ColumnName = c.column_name
								) a
								for xml path ('Property'), type
							) Properties
						from 
							information_Schema.columns c with (nolock)
								left join
							(
								select s.name SchemaName, t.name TableName, c.name ColumnName
								from
									sys.tables t
										inner join
									sys.syscolumns c
										on t.object_id=c.id
										inner join
									sys.schemas s
										on s.schema_id=t.schema_id
								where
									c.iscomputed=1
							) cc 
								on c.table_schema=cc.schemaname 
								and c.table_name=cc.tablename
								and c.column_name=cc.columnName
								outer apply
							(
								select 1 isPk
								from 
									INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc with (nolock)
										inner join
									INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu with (nolock)
										on tc.constraint_name=ccu.constraint_name 
										and tc.table_schema=ccu.table_schema 
										and tc.table_name=ccu.table_name
								where
									tc.[CONSTRAINT_TYPE]='PRIMARY KEY' and
									tc.table_schema=c.table_schema and
									tc.table_name=c.table_name and
									ccu.column_name=c.column_name
							) pk
								outer apply
							(
								select  
									--fk.table_Schema, fk.table_name, fk.COLUMN_NAME,
									ref.TABLE_SCHEMA, ref.TABLE_NAME
								from 
									[INFORMATION_SCHEMA].[CONSTRAINT_COLUMN_USAGE] fk with (nolock)
										inner join
									[INFORMATION_SCHEMA].[REFERENTIAL_CONSTRAINTS] rc with (nolock)
										on rc.CONSTRAINT_NAME=fk.CONSTRAINT_NAME
										and rc.CONSTRAINT_SCHEMA=fk.CONSTRAINT_SCHEMA
										and rc.CONSTRAINT_CATALOG=fk.CONSTRAINT_CATALOG
										inner join
									[INFORMATION_SCHEMA].[CONSTRAINT_TABLE_USAGE] ref with (nolock)
										on ref.CONSTRAINT_NAME=rc.UNIQUE_CONSTRAINT_NAME
										and ref.CONSTRAINT_SCHEMA=rc.UNIQUE_CONSTRAINT_SCHEMA
										and ref.CONSTRAINT_CATALOG=rc.UNIQUE_CONSTRAINT_CATALOG
								where
									fk.table_schema=c.TABLE_SCHEMA and						
									fk.table_name=c.table_name and						
									fk.COLUMN_NAME=c.COLUMN_NAME						
							) ref
						where
							c.table_name=t.table_name and
							c.table_schema=t.table_schema
						order by 
							c.ordinal_position
						for xml path ('Column'), type
					) Columns,
					(
						select 
							SchemaName '@schema', 
							TableName '@table', 
							ColumnName '@column',
							CollectionType '@collectionType'
						from
						(
							select  
								fk.table_Schema SchemaName, 
								fk.table_name TableName, 
								fk.COLUMN_NAME ColumnName,
								'actual' CollectionType
							from 
								[INFORMATION_SCHEMA].[CONSTRAINT_COLUMN_USAGE] fk with (nolock)
									inner join
								[INFORMATION_SCHEMA].[REFERENTIAL_CONSTRAINTS] rc with (nolock)
									on rc.CONSTRAINT_NAME=fk.CONSTRAINT_NAME
									and rc.CONSTRAINT_SCHEMA=fk.CONSTRAINT_SCHEMA
									and rc.CONSTRAINT_CATALOG=fk.CONSTRAINT_CATALOG
									inner join
								[INFORMATION_SCHEMA].[CONSTRAINT_TABLE_USAGE] ref with (nolock)
									on ref.CONSTRAINT_NAME=rc.UNIQUE_CONSTRAINT_NAME
									and ref.CONSTRAINT_SCHEMA=rc.UNIQUE_CONSTRAINT_SCHEMA
									and ref.CONSTRAINT_CATALOG=rc.UNIQUE_CONSTRAINT_CATALOG
							where
								ref.TABLE_SCHEMA = t.table_Schema and
								ref.TABLE_NAME = t.table_name

							union all

							select  
								lt.SchemaName, 
								lt.TableName, 
								lt.ColumnName,
								'virtual' CollectionType
							from
								db.ColumnProperties lt
							where
								lt.propertyname='linksto' and
								dbo.LeftOf(try_cast(lt.propertyValue as nvarchar(4000)), '.')=t.table_Schema and
								dbo.RightOf(dbo.LeftOf(try_cast(lt.PropertyValue as nvarchar(4000)), '('), '.')=t.table_name
						) allcollections
						for xml path ('Collection'), type
					) Collections
				from 
					(
						select ist.*, st.is_node IsNodeTable, st.is_edge IsEdgeTable
						from
							INFORMATION_SCHEMA.tables ist with (nolock)
								inner join
							sys.schemas ss with (nolock)
								on ss.name=ist.table_schema
								left join
							sys.tables st with (nolock)
								on st.schema_id=ss.schema_id
								and st.name=ist.table_name
					) t
				order by
					t.table_Schema,
					t.table_name
				for xml path('Table'), type
			) [Tables]
		) a,
		(
			select
			(
				select
					r.routine_Schema '@schema',
					r.routine_name '@name',
					r.routine_type '@routineType',
					(
						select 
							p.PropertyName '@name',
							p.PropertyValue '@value'
						from 
							db.SprocProperties p with (nolock)
						where
							p.SchemaName = r.specific_schema and
							p.SprocName = r.routine_name
						for xml path ('Property'), type
					) Properties,
					(
						select 
							p.ordinal_position '@position',
							p.parameter_mode '@mode',
							p.parameter_name '@name',
							p.data_type '@sqlType',
							p.character_maximum_length '@maxLen',
							(
								select 
									pp.PropertyName '@name',
									pp.PropertyValue '@value'
								from 
									db.SprocParameterProperties pp with (nolock)
								where
									pp.SchemaName = r.specific_schema and
									pp.SprocName = r.routine_name and
									pp.ParameterName = p.parameter_name
								for xml path ('Property'), type
							) Properties
						from 
							information_Schema.parameters p with (nolock)
						where
							r.specific_name=p.specific_name and
							r.specific_schema=p.specific_schema
						order by 
							p.ordinal_position
						for xml path ('Arg'), type
					) Args,
					(
						select result_set '@resultSetNumber', Cnt '@colCount', case when prc.ErrorMessage is null then 0 else 1 end '@isError', prc.ErrorMessage 'Error',
						(
							select
								COLUMN_NAME '@name',
								ORDINAL_POSITION '@position',
								0 '@isPrimaryKey',
								0 '@isIdentity',
								case when IS_NULLABLE ='yes' then 1 else 0 end '@isNullable',
								DATA_TYPE '@sqlType',
								case when CHARACTER_MAXIMUM_LENGTH=0 then null else CHARACTER_MAXIMUM_LENGTH end '@maxLen'
							from
								prc c
							where
								c.TABLE_SCHEMA=r.specific_schema and
								c.TABLE_NAME=r.specific_name and
								c.result_set = prc.result_set and
								c.ResultType='c'
							for xml path ('Column'), type
						) [Columns]
						from
							(
								select TABLE_SCHEMA, TABLE_NAME, result_set, ResultType, ErrorMessage, sum(case when resulttype='c' then 1 else 0 end) Cnt 
								from prc
								group by TABLE_SCHEMA, TABLE_NAME, result_set, ResultType, ErrorMessage
								) prc
						where
							prc.TABLE_SCHEMA=r.specific_schema and
							prc.TABLE_NAME=r.specific_name
						order by 
							prc.result_set
						for xml path ('ResultSet'), type
					) ResultSets
				from 
					INFORMATION_SCHEMA.routines r with (nolock)
				where
					r.routine_type in ('PROCEDURE', 'FUNCTION')
				order by
					r.routine_Schema,
					r.routine_name
				for xml path('Sproc'), type
			) Sprocs
		) b
		for xml path('SchemaMeta'), type
	) x(SchemaMeta)
)
select db_name() HostDatabaseName, X, cast(X as nvarchar(max)) T
from a

GO
