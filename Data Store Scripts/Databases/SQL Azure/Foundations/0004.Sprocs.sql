create function dbo.LeftOf(@s nvarchar(4000), @pivot nvarchar(4000))
returns nvarchar(4000)
begin

	declare @z int
	set @z = charindex(@pivot, @s)
	if (@z is not null)
	begin
		set @s = left(@s, @z-1)
	end
	return @s

end

GO

create function dbo.RightOf(@s nvarchar(4000), @pivot nvarchar(4000))
returns nvarchar(4000)
begin

	declare @z int
	set @z = charindex(@pivot, @s)
	if (@z is not null)
	begin
		set @s = right(@s, len(@s)-@z)
	end
	else begin
		set @s = null
	end
	return @s

end

GO

select dbo.LeftOf('jason.thomas', '.'), dbo.RightOf('jason.thomas', '.')

GO

CREATE FUNCTION SplitString(@csv_str NVARCHAR(4000), @delimiter nvarchar(20))
 RETURNS @splittable table (val nvarchar(max), pos int)
AS
BEGIN  
 
-- Check for NULL string or empty sting
    IF  (LEN(@csv_str) < 1 OR @csv_str IS NULL)
    BEGIN
        RETURN
    END
 
    ; WITH csvtbl(i,j, pos)
    AS
    (
        SELECT i=1, j= CHARINDEX(@delimiter,@csv_str+@delimiter), 1
 
        UNION ALL 
 
        SELECT i=j+1, j=CHARINDEX(@delimiter,@csv_str+@delimiter,j+1), pos+1
        FROM csvtbl
        WHERE CHARINDEX(@delimiter,@csv_str+@delimiter,j+1) <> 0
    )   
    INSERT  INTO @splittable(val, pos)
    SELECT  SUBSTRING(@csv_str,i,j-i), pos
    FROM    csvtbl 
 
    RETURN
END  

GO

CREATE FUNCTION [dbo].[Seq](@start int, @cnt int, @inc float)
RETURNS @items TABLE (
   Val int not null,
   Pos int not null
) 
AS
BEGIN

	if (@start is not null and @inc is not null and @cnt is not null)
	begin
		declare @val float = cast(@start as float)
		declare @pos int=0
		while (@cnt>0) begin
			insert into @items values (round(@val, 0), @pos);
			set @pos = @pos + 1
			set @val = @val + @inc
			set @cnt = @cnt - 1
		end
	end 
   RETURN;
END;
GO

select * from dbo.seq(0,10,3)

GO

create proc db.DeleteFromTable
	@schemaName sysname,
	@tableName sysname,
	@conditions nvarchar(max)=null,
	@batchSize int=null
as
begin


	set nocount on
	set @schemaName = coalesce(@schemaName, 'dbo')
	set @batchSize = coalesce(@batchSize, 1000)
	declare @sql nvarchar(4000)


	set @sql = 'select @tot=count(*) from '+quotename(@schemaName)+'.'+quotename(@tableName)
	if (@conditions is not null)
	begin
		set @sql = @sql + ' where ' + @conditions
	end

	declare @tot int
	exec sp_executesql @sql, N'@tot int output',@tot=@tot output
	
	 


	set @sql = 'delete top('+cast(@batchSize as nvarchar(10))+') from '+quotename(@schemaName)+'.'+quotename(@tableName)
	if (@conditions is not null)
	begin
		set @sql = @sql + ' where ' + @conditions
	end

	exec db.PrintNow 'db.DeleteFromTable({n0}) => {s0}', @tot, @s0=@sql

	declare @cnt int
	declare @rt int=0

	Again:

		exec(@sql)
		set @cnt=@@rowcount
		set @rt=@rt+@cnt
		exec db.PrintNow 'Deleted {n0} rows ({n1}/{n2}) from {s0}.{s1}', @n0=@cnt, @n1=@rt, @n2=@tot, @s0=@schemaName, @s1=@tableName
		if (@cnt>0) goto Again

end

GO
